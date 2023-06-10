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
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }
        public IActionResult Index()
        {
            List<Company> listOfCompanys = _unitOfWork.Company.GetAll().ToList();

            return View(listOfCompanys);
        }

        public IActionResult Upsert(int? id)
        {
            

            if(id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company companyEdit = _unitOfWork.Company.Get(u=>u.Id == id);
                return View(companyEdit);
            }
            
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if(ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";

                return RedirectToAction("Index", "Company");
            }
            else
            {
                return View(company);
            }
            
         

        }

      

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> listOfCompanys = _unitOfWork.Company.GetAll().ToList();
            return Json(new {data = listOfCompanys});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var CompanyToBeDeleted = _unitOfWork.Company.Get(x=> x.Id == id);
            if(CompanyToBeDeleted == null)
            {
                return Json (new { success = false, message = "Error while deleting" });
            }
           
            //delete the Company
            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();

            return Json( new { success = true, message = "Delete Successful"} );

        }

        #endregion

    }
}
