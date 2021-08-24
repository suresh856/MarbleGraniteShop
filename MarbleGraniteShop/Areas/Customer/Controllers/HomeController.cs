using MarbleGraniteShop.DataAccess.Data;
using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;
using MarbleGraniteShop.Models.ViewModels;
using MarbleGraniteShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarbleGraniteShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _applicationDbContext;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext applicationDbContext)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            this._applicationDbContext = applicationDbContext;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var productList = from s in _applicationDbContext.Products
                              select s;
            productList = productList.Include("Category");
            productList = productList.Include("SpecialTag");

            if (searchString != null)
            {
                page = 1;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                productList = productList.Where(p => p.Name.Contains(searchString)
                                       || p.Title.Contains(searchString) || p.ShadeColor.Contains(searchString));
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
            }


            var products = _unitOfWork.Product.GetAll(includeProperties: "Category,SpecialTag");
            return View(products);
        }

        public IActionResult Details(int id)
        {
            List<FeedBack> feedBacks = _unitOfWork.FeedBack.GetAll(f => f.ProductId == id, includeProperties: "ApplicationUser").ToList();
            var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,SpecialTag");
            productFromDb.Images = (List<Image>)_unitOfWork.Images.GetAll(x => x.ProductId == id);
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id,
                FeedBackList = feedBacks
            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product"
                    );

                if (cartFromDb == null)
                {
                    //no records exists in database for that product for that user
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    //_unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList().Count();

                //HttpContext.Session.SetObject(SD.ssShoppingCart, CartObject);
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,SpecialTag");
                productFromDb.Images = (List<Image>)_unitOfWork.Images.GetAll(x => x.ProductId == CartObject.Product.Id);
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            }


        }

        [HttpPost]
        [Authorize]
        public IActionResult FeedBack(ShoppingCart shoppingCart)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            FeedBack feedBack = new FeedBack();
            feedBack = shoppingCart.FeedBack;
            feedBack.ApplicationUserId = claim.Value;
            feedBack.ProductId = shoppingCart.ProductId;
            Product product = _unitOfWork.Product.Get(shoppingCart.ProductId);
            product.Rating = Math.Round((product.Rating + Convert.ToDouble(feedBack.Rating)) / 2);
            _unitOfWork.FeedBack.Add(feedBack);
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Details), new { id = shoppingCart.ProductId });
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
