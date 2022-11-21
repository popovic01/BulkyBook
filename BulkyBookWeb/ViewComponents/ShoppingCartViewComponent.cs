using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        //backend code - getting shopping cart list
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //if user is logged in, we want to get their session
            var claimsIndentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIndentity.FindFirst(ClaimTypes.NameIdentifier);

            //checks if user is logged in
            if (claim != null)
            {
                //checks if session is null
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                {
                    //session is already set
                    //retreiving value from session
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                else
                {
                    //session is null
                    //we need to get count from db
                    var count = _unitOfWork.ShoppingCartRepository.GetAll(c => c.ApplicationUserId == claim.Value).ToList().Count;
                    HttpContext.Session.SetInt32(SD.SessionCart, count);
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
            }
            else
            {
                //removing session
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
