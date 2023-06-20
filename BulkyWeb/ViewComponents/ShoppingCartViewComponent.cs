using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string sessionId = HttpContext.Session.Id;

            //var claimsIdentity = (ClaimsIdentity)User.Identity;

            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            return View(_unitOfWork.ShoppingCart.GetAll(x=>x.SessionId == sessionId).Count());

            //if (sessionId != null)
            //{
            //    //Sql will not hit;
            //    if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
            //    {
            //        HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.SessionId == sessionId).Count());
            //    }
                
            //    return View(HttpContext.Session.GetInt32(SD.SessionCart) );
            //}
            //else
            //{
            //    HttpContext.Session.Clear();
            //    return View(0);
            //}

        }

    }
}
