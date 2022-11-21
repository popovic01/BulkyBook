using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> coverTypes = _unitOfWork.CoverTypeRepository.GetAll();
            return View(coverTypes);
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverTypeRepository.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Cover type created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            CoverType ctFromDb = _unitOfWork.CoverTypeRepository.GetFirstOrDefault(c => c.Id == id);

            if (ctFromDb == null)
            {
                return NotFound();
            }

            return View(ctFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType ct)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverTypeRepository.Update(ct);
                _unitOfWork.Save();
                TempData["success"] = "Cover type updated successfully";
                return RedirectToAction("Index");
            }
            return View(ct);
        }

        //GET
        public IActionResult Delete(int? id)
        {
            CoverType ct = _unitOfWork.CoverTypeRepository.GetFirstOrDefault(c => c.Id == id);
            if (ct == null)
            {
                return NotFound();
            }
            return View(ct);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int id)
        {
            CoverType ct = _unitOfWork.CoverTypeRepository.GetFirstOrDefault(c => c.Id == id);
            if (ct == null)
            {
                return NotFound();
            }
            _unitOfWork.CoverTypeRepository.Remove(ct);
            _unitOfWork.Save();
            TempData["success"] = "Cover type deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
