using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> listOfProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(listOfProducts);
        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });

            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"]= CategoryList;

            ProductVM productVM = new()
            {
                CategoryList = CategoryList,
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitOfWork.Product.Get(u=>u.Id == id);
                return View(productVM);
            }
            
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? formFile) //IFormFile is for image
        {

            //Image store process

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if(formFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    //delete the old image
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                    if(System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                }
                
                using ( var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create ))
                {
                    formFile.CopyTo(fileStream);
                }
                //path will be store in DB
                productVM.Product.ImageUrl = @"\images\product\" + fileName;
            }

            if(productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
            }

                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";

                return RedirectToAction("Index", "Product");
         

        }

        /// <summary>
        /// We have used Upsert to club the both Create or Update
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        //public IActionResult Edit(int? id)
        //{
        //    if (id == null && id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product product = _unitOfWork.Product.Get(u => u.Id == id);
            
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);

        //}

        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(product);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product updated successfully";

        //        return RedirectToAction("Index", "Product");
        //    }

        //    return View();
        //}


        // Used API call to delete the product. check in Api sections


        //public IActionResult Delete(int id)
        //{
        //    if (id == null && id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product product = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int id)
        //{
        //    Product product = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(product);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";

        //    return RedirectToAction("Index");

        //}

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> listOfProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = listOfProducts});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(x=> x.Id == id);
            if(productToBeDeleted == null)
            {
                return Json (new { success = false, message = "Error while deleting" });
            }
            //first remove the image
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
            if(System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            //then delete the product
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json( new { success = true, message = "Delete Successful"} );

        }

        #endregion

    }
}
