using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;
using MarbleGraniteShop.Models.ViewModels;
using MarbleGraniteShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarbleGraniteShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        public async Task<IActionResult> Upsert(int? id)
        {
            IEnumerable<Category> CatList =  _unitOfWork.Category.GetAll();
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                SpecialTagList = _unitOfWork.SpecialTag.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })

            };
            if (id == null)
            {
                //this is for create
                return View(productVM);
            }
            //this is for edit
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                List<Image> images = new List<Image>();
                if (files.Count > 0)
                {

                    for (int i = 0; i < files.Count; i++)
                    {
                        Image image = new Image();
                        string fileName = Guid.NewGuid().ToString();
                        var uploads = Path.Combine(webRootPath, @"images\products");
                        var extenstion = Path.GetExtension(files[0].FileName);

                        if (i == 0 && productVM.Product.ImageUrl != null)
                        {
                            //this is an edit and we need to remove old image
                            //var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                            //if (System.IO.File.Exists(imagePath))
                            //{
                            //    System.IO.File.Delete(imagePath);
                            //}

                            List<Image> imagesList = new List<Image>();
                            imagesList = (List<Image>)_unitOfWork.Images.GetAll(x => x.ProductId == productVM.Product.Id);
                            foreach (Image imageEdit in imagesList)
                            {
                                var imagePath = Path.Combine(webRootPath, imageEdit.ImageUrl.TrimStart('\\'));
                                if (System.IO.File.Exists(imagePath))
                                {
                                    System.IO.File.Delete(imagePath);
                                }
                                _unitOfWork.Images.Remove(imageEdit);
                            }
                            _unitOfWork.Save();

                        }
                        using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                        {
                            files[i].CopyTo(filesStreams);
                        }
                        image.ImageUrl = @"\images\products\" + fileName + extenstion;
                        images.Add(image);
                    }
                    productVM.Product.ImageUrl = images[0].ImageUrl;
                }
                else
                {
                    //update when they do not change the image
                    if (productVM.Product.Id != 0)
                    {
                        Product objFromDb = _unitOfWork.Product.Get(productVM.Product.Id);

                        productVM.Product.ImageUrl = objFromDb.ImageUrl;
                        //var imagesFromDb = _unitOfWork.Images.GetAll(x => x.ProductId == productVM.Product.Id);
                        //productVM.Product.Images = (List<Image>)imagesFromDb;
                    }
                }


                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);

                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                _unitOfWork.Save();
                foreach (Image image in images)
                {
                    image.ProductId = productVM.Product.Id;
                    _unitOfWork.Images.Add(image);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                IEnumerable<Category> CatList =  _unitOfWork.Category.GetAll();
                productVM.CategoryList = CatList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                productVM.SpecialTagList = _unitOfWork.SpecialTag.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                productVM.CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                    var imagesFromDb = _unitOfWork.Images.GetAll(x => x.ProductId == productVM.Product.Id);
                    productVM.Product.Images = (List<Image>)imagesFromDb;
                }
            }
            return View(productVM);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includeProperties: "Category,SpecialTag,Company");
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            string webRootPath = _hostEnvironment.WebRootPath;
            List<Image> images = new List<Image>();
            images = (List<Image>)_unitOfWork.Images.GetAll(x => x.ProductId == id);
            foreach (Image image in images)
            {
                var imagePath = Path.Combine(webRootPath, image.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _unitOfWork.Images.Remove(image);
            }

            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}
