using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModel;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
	public class OrderController : Controller
	{
        IUnitOfWork _unitOfWork;
        //when we post data, OrderVm will be binded
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }
		public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(h => h.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetailRepository.GetAll(d => d.OrderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_Pay_Now()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(h => h.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetailRepository.GetAll(d => d.Id == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //proccesing payment
            //stripe settings 
            var domain = "https://localhost:44350/";
            //we are creating session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader headerFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(o => o.Id == orderHeaderId);

            //company user
            if (headerFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                //getting existing session
                Session session = service.Get(headerFromDb.SessionId);

                //check the stripe status 
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderHeaderId, headerFromDb.SessionId, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderId, headerFromDb.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        [HttpPost]
        //only admin or employee can do this action
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var headerFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(h => h.Id == OrderVM.OrderHeader.Id);
            headerFromDb.Name = OrderVM.OrderHeader.Name;
            headerFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            headerFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            headerFromDb.City = OrderVM.OrderHeader.City;
            headerFromDb.State = OrderVM.OrderHeader.State;
            headerFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (OrderVM.OrderHeader.Carrier != null)
                headerFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            if (OrderVM.OrderHeader.TrackingNumber != null)
                headerFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = headerFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var headerFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(h => h.Id == OrderVM.OrderHeader.Id);
            headerFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            headerFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            headerFromDb.OrderStatus = SD.StatusShipped;
            headerFromDb.ShippingDate = DateTime.Now;
            if (headerFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
                headerFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var headerFromDb = _unitOfWork.OrderHeaderRepository.GetFirstOrDefault(h => h.Id == OrderVM.OrderHeader.Id);
            if (headerFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                //we need to refund because payment is already approved
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = headerFromDb.PaymentIntentId
                    //by default amount is same as it was when paying was done
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(headerFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(headerFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Order Canceled Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = headerFromDb.Id });
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            //getting all order headers
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                //admin and employee can see all orders
                orderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //customer or company can only see their orders
                orderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(
                    o => o.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            //filtering orders
            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusInProcess);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
