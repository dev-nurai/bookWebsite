﻿
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
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
            //HttpContext.Session.SetInt32(SD.GuestSessionId, _unitOfWork.ShoppingCart.GetAll(x => x.SessionId ).Count());
            
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            
            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(x => x.Id == productId, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = productId
                
            };
           
            return View(shoppingCart);
        }

        [HttpPost]
        //[Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count >= 1)
            {
                //Session

                string sessionId = HttpContext.Session.Id;

                //we need user id to add cart
                var claimsIdentity = (ClaimsIdentity)User.Identity; //default method by .Net team

                if(claimsIdentity.Name != null)
                {
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                   shoppingCart.ApplicationUserId = userId;
                }
                

                shoppingCart.SessionId = sessionId;

                //Check shoppingcart count from DB

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.SessionId == sessionId && x.ProductId == shoppingCart.ProductId);

                if (cartFromDb != null)
                {
                    //shopping cart exists, Update the data
                    cartFromDb.Count += shoppingCart.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                    _unitOfWork.Save();
                }
                else
                {
                    //Add new record
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                    _unitOfWork.Save();
                    HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.SessionId == sessionId).Count());
                }

                TempData["success"] = "Cart updated successfully";


                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Invalid Quantity";

            ShoppingCart returnShoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(x => x.Id == shoppingCart.ProductId, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = shoppingCart.ProductId

            };

            return View(returnShoppingCart);

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