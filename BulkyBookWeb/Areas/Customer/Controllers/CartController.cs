using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize] //only authorized user can access this controller
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork; 
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }
        public IActionResult Index()
        {
            //retreiving all carts for the particular user
            //we need id of logged in user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            /*we want to populate navigation property of product 
             * because we need details about products*/
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == claim.Value,
                    includeProperties: "Product")
            };
            foreach (var item in ShoppingCartVM.ListCart)
            {
                //final price of product
                item.Price = GetPriceBasedOnQuantity(
                    item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.CartTotal += (item.Price * item.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            return View();
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => c.Id == cartId);
            if (cartFromDb.Count == 1)
                _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            
            else
                _unitOfWork.ShoppingCartRepository.DecrementCount(cartFromDb, 1);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCartRepository.IncrementCount(cartFromDb, 1);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
                return price;
            if (quantity <= 100)
                return price50;
            return price100;

        }
    }
}
