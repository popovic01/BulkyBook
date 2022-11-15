using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverTypeRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            if (id == null || id == 0)
            {
                //create product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                //update product
                productVM.Product = _unitOfWork.ProductRepository.GetFirstOrDefault(i => i.Id == id);
                return View(productVM);
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM prod, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                //getting wwwRoth path
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    //generating new name for uploaded image
                    string fileName = Guid.NewGuid().ToString();
                    //final location where image needs to be uploaded
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    //gets extension of the image
                    var extension = Path.GetExtension(file.FileName);

                    //deleting old image if we update
                    if (prod.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, prod.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        //copying the file
                        file.CopyTo(fileStreams);
                    }
                    //saving to the db
                    prod.Product.ImageUrl = @"\images\products\" + fileName + extension;

                }
                if (prod.Product.Id == 0)
                {
                    _unitOfWork.ProductRepository.Add(prod.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(prod.Product);
                    TempData["success"] = "Product updated successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(prod);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = products }); 
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var prod = _unitOfWork.ProductRepository.GetFirstOrDefault(p => p.Id == id);
            if (prod == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            //deleting old image
            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, prod.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.ProductRepository.Remove(prod);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted successfully" }); ;
        }

        #endregion
    }
}
