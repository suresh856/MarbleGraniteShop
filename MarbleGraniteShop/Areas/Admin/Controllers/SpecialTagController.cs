using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;
using MarbleGraniteShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarbleGraniteShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class SpecialTagController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecialTagController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            SpecialTag SpecialTag = new SpecialTag();
            if (id == null)
            {
                //this is for create
                return View(SpecialTag);
            }
            //this is for edit
            SpecialTag = _unitOfWork.SpecialTag.Get(id.GetValueOrDefault());
            if (SpecialTag == null)
            {
                return NotFound();
            }
            return View(SpecialTag);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(SpecialTag SpecialTag)
        {
            if (ModelState.IsValid)
            {
                if (SpecialTag.Id == 0)
                {
                    _unitOfWork.SpecialTag.Add(SpecialTag);

                }
                else
                {
                    _unitOfWork.SpecialTag.Update(SpecialTag);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(SpecialTag);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.SpecialTag.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {

            var objFromDb = _unitOfWork.SpecialTag.Get(id);
            if (objFromDb == null)
            {
                TempData["Error"] = "Error deleting Category";
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.SpecialTag.Remove(objFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Category successfully deleted";
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}
