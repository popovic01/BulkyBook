using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;   

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;   
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category,CoverType");
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Count = 1, //default value
                ProductId = productId,
                Product = _unitOfWork.ProductRepository.GetFirstOrDefault(p => p.Id == productId, includeProperties: "Category,CoverType")
            };

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //only logged in users can access post action method from this page
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //getting user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //nameidentifier is user id
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value; //id

            //cart where we already have product we want to add now and for current user
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.GetFirstOrDefault(c => 
                c.ProductId == shoppingCart.ProductId && c.ApplicationUserId == claim.Value);

            //if is null we don't have cart like that
            if (cartFromDb == null)
            {
                _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                _unitOfWork.Save();
                //sessions are key-value pairs
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCartRepository.GetAll(s => s.ApplicationUserId == claim.Value).ToList().Count);
            }
            else
            {
                //updating cart's count 
                _unitOfWork.ShoppingCartRepository.IncrementCount(cartFromDb, shoppingCart.Count);
                _unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}