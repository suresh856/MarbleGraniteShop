using MarbleGraniteShop.DataAccess.Repository.IRepository;
using MarbleGraniteShop.Models;
using MarbleGraniteShop.Models.ViewModels;
using MarbleGraniteShop.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MarbleGraniteShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        private TwilioSettings _twilioOptions { get; set; }
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }


        public static string EmailTemlateForCompnay = @"Hi companyName,<br > You have appointment with
                        customerName scheduled on appointmentDate.<br >Product(s) selected by customer are <br > productsForAppointment.
                        <br ><br >Regards MarbleGraniteShop
                       ";

        public static string EmailTemlateForCustomer = @"Hi customerName,<br > You have appointment with
                        companyName scheduled on appointmentDate.<br >Product(s) selected by you are <br > productsForAppointment.
                        <br ><br >Regards MarbleGraniteShop
                       ";
        public static string EmailSubject = "Appointment with partyName  on appointmentDate";

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender,
            UserManager<IdentityUser> userManager, IOptions<TwilioSettings> twilionOptions)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _twilioOptions = twilionOptions.Value;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                        .GetFirstOrDefault(u => u.Id == claim.Value,
                                                        includeProperties: "Company");

            foreach (var list in ShoppingCartVM.ListCart)
            {

                ShoppingCartVM.OrderHeader.OrderTotal += (list.Product.Price * list.Count);
                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }


            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty!");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return RedirectToAction("Index");

        }


        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product");
            cart.Count += 1;

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product");

            if (cart.Count == 1)
            {
                var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            }
            else
            {
                cart.Count -= 1;

                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault
                            (c => c.Id == cartId, includeProperties: "Product");

            var cnt = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                ShoppingCartVM = new ShoppingCartVM()
                {
                    OrderHeader = new Models.OrderHeader(),
                    ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value,
                                                                includeProperties: "Product")
                };

                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                                .GetFirstOrDefault(c => c.Id == claim.Value,
                                                                    includeProperties: "Company");

                foreach (var list in ShoppingCartVM.ListCart)
                {

                    ShoppingCartVM.OrderHeader.OrderTotal += (list.Product.Price * list.Count);
                }
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
                ShoppingCartVM.OrderHeader.Email = ShoppingCartVM.OrderHeader.ApplicationUser.Email;

                return View(ShoppingCartVM);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Enable this method when Order processing is needed.Now commented because using Appointent system
        //[HttpPost]
        //[ActionName("Summary")]
        //[ValidateAntiForgeryToken]
        //public IActionResult SummaryPost(string stripeToken)
        //{
        //    if (string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.Name) || string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.PhoneNumber)
        //        || string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.City) || string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.State)
        //        || string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.PostalCode)
        //        )
        //    {
        //        return RedirectToAction(nameof(Summary));
        //    }

        //    try
        //    {
        //        var claimsIdentity = (ClaimsIdentity)User.Identity;
        //        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //        ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
        //                                                        .GetFirstOrDefault(c => c.Id == claim.Value,
        //                                                                includeProperties: "Company");

        //        ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart
        //                                    .GetAll(c => c.ApplicationUserId == claim.Value,
        //                                    includeProperties: "Product");

        //        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        //        ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
        //        ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
        //        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

        //        _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
        //        _unitOfWork.Save();

        //        foreach (var item in ShoppingCartVM.ListCart)
        //        {

        //            OrderDetails orderDetails = new OrderDetails()
        //            {
        //                ProductId = item.ProductId,
        //                OrderId = ShoppingCartVM.OrderHeader.Id,
        //                Price = item.Product.Price,
        //                Count = item.Count
        //            };
        //            ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
        //            _unitOfWork.OrderDetails.Add(orderDetails);

        //        }

        //        _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
        //        _unitOfWork.Save();
        //        HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

        //        if (stripeToken == null)
        //        {
        //            //order will be created for delayed payment for authroized company
        //            ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
        //            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
        //            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        //        }
        //        else
        //        {
        //            //process the payment
        //            var options = new ChargeCreateOptions
        //            {
        //                Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
        //                Currency = "usd",
        //                Description = "Order ID : " + ShoppingCartVM.OrderHeader.Id,
        //                Source = stripeToken
        //            };

        //            var service = new ChargeService();
        //            Charge charge = service.Create(options);

        //            if (charge.Id == null)
        //            {
        //                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
        //            }
        //            else
        //            {
        //                ShoppingCartVM.OrderHeader.TransactionId = charge.Id;
        //            }
        //            if (charge.Status.ToLower() == "succeeded")
        //            {
        //                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
        //                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
        //                ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
        //            }
        //        }

        //        _unitOfWork.Save();

        //        return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ShoppingCartVM ShoppingCartVM)
        {
            // email sending part is remaining 
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value,
                                                        includeProperties: "Product");

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                            .GetFirstOrDefault(c => c.Id == claim.Value,
                                                                includeProperties: "Company");


            ShoppingCartVM.OrderHeader.AppointmentDate = ShoppingCartVM.OrderHeader.AppointmentDate
                                                            .AddHours(ShoppingCartVM.OrderHeader.AppointmentTime.Hour)
                                                            .AddMinutes(ShoppingCartVM.OrderHeader.AppointmentTime.Minute);

            List<int> companyIds = new List<int>();
            foreach (var list in ShoppingCartVM.ListCart)
            {

                companyIds.Add(list.Product.CompanyId);
            }

            companyIds = companyIds.Distinct().ToList();
            int appointmentId=0;
            foreach (int id in companyIds)
            {
                Appointment appointments = new Appointment();
                appointments.AppointmentDate = ShoppingCartVM.OrderHeader.AppointmentDate;
                appointments.CustomerName = ShoppingCartVM.OrderHeader.Name;
                appointments.CustomerPhoneNumber = ShoppingCartVM.OrderHeader.PhoneNumber;
                appointments.CustomerEmail = ShoppingCartVM.OrderHeader.Email;
                appointments.CompanyId = id;
                appointments.AppointmentDate = ShoppingCartVM.OrderHeader.AppointmentDate;
                // find another solution for this operation as this hits db for each entries
                _unitOfWork.Appointment.Add(appointments);
                _unitOfWork.Save();
                appointmentId = appointments.Id;

                var productList = ShoppingCartVM.ListCart.Where(c => c.Product.CompanyId == id).ToList();
                string productsForAppointment = @"";
                foreach(var item in productList)
                {
                    productsForAppointment = productsForAppointment +  "Product Name : "+ item.Product.Name;
                    productsForAppointment = productsForAppointment  + "  Product Code : "+ item.Product.Id;
                    productsForAppointment = productsForAppointment + ("<br >");
                    ProductsSelectedForAppointment productsSelectedForAppointment = new ProductsSelectedForAppointment()
                    {
                        AppointmentId = appointmentId,
                        ProductId = item.ProductId
                    };
                    _unitOfWork.ProductsSelectedForAppointment.Add(productsSelectedForAppointment);
                }
                Company company = _unitOfWork.Company.Get(id);
                string mailSubject = EmailSubject;
                mailSubject = mailSubject.Replace("appointmentDate", ShoppingCartVM.OrderHeader.AppointmentDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture));
                string mailSubjectForCustomer = mailSubject;
                string mailSubjectForCompany = mailSubject;
                mailSubjectForCustomer = mailSubjectForCustomer.Replace("partyName", company.Name);
                mailSubjectForCompany = mailSubjectForCompany.Replace("partyName", ShoppingCartVM.OrderHeader.Name);
                string messageCompany = EmailTemlateForCompnay;
                messageCompany = messageCompany.Replace("companyName",company.Name);
                messageCompany = messageCompany.Replace("customerName", ShoppingCartVM.OrderHeader.Name);
                messageCompany = messageCompany.Replace("appointmentDate", ShoppingCartVM.OrderHeader.AppointmentDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture));
                messageCompany = messageCompany.Replace("productsForAppointment", productsForAppointment);
                string messageCustomer = EmailTemlateForCustomer;
                messageCustomer = messageCustomer.Replace("companyName", company.Name);
                messageCustomer = messageCustomer.Replace("customerName", ShoppingCartVM.OrderHeader.Name);
                messageCustomer = messageCustomer.Replace("appointmentDate", ShoppingCartVM.OrderHeader.AppointmentDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture));
                messageCustomer = messageCustomer.Replace("productsForAppointment", productsForAppointment);

                _emailSender.SendEmailAsync(ShoppingCartVM.OrderHeader.Email, mailSubjectForCustomer, messageCustomer);
                _emailSender.SendEmailAsync(company.Email, mailSubjectForCompany, messageCompany);
              
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

            return RedirectToAction("AppointmentConfirmation", "Cart", new { Id = appointmentId });

        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            //TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            //try
            //{
            //    var message = MessageResource.Create(
            //        body: "Order Placed on Bulky Book. Your Order ID:" + id,
            //        from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
            //        to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
            //        );
            //}
            //catch(Exception ex)
            //{
            //    throw ex;
            //}



            return View(id);
        }

        public IActionResult AppointmentConfirmation(int id)
        {
            ShoppingCartVM = new ShoppingCartVM();
            ShoppingCartVM.Products = new List<Models.Product>();


            ShoppingCartVM.Appointment = _unitOfWork.Appointment.Get(id);
            List<ProductsSelectedForAppointment> objProdList = _unitOfWork.ProductsSelectedForAppointment.GetAll(p => p.AppointmentId == id).ToList();

            foreach (ProductsSelectedForAppointment prodAptObj in objProdList)
            {
                ShoppingCartVM.Products.Add(_unitOfWork.Product.GetAll(p => p.Id == prodAptObj.ProductId,includeProperties:"Category,SpecialTag").FirstOrDefault());
            }
            HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);
            return View(ShoppingCartVM);
        }

    }
}
