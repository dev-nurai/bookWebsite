using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop.Implementation;
using System.Data;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            

            return View();
        }

        public IActionResult RoleManegement(string userId)
        {
            string RoleId = _applicationDbContext.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;

            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _applicationDbContext.ApplicationUsers.Include(x=>x.Company).FirstOrDefault(y=>y.Id == userId),
                RoleList = _applicationDbContext.Roles.Select(i=> new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name,
                }),
                CompanyList = _applicationDbContext.Companies.Select(i=>new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };

            roleManagementVM.ApplicationUser.Role = _applicationDbContext.Roles.FirstOrDefault(x => x.Id == RoleId).Name;


            return View(roleManagementVM);
        }

        [HttpPost]
        public IActionResult RoleManegement(RoleManagementVM roleManagementVM)
        {
            string RoleId = _applicationDbContext.UserRoles.FirstOrDefault(x => x.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _applicationDbContext.Roles.FirstOrDefault(x => x.Id == RoleId).Name;

            if(!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                //a role was updated
                ApplicationUser applicationUser = _applicationDbContext.ApplicationUsers.FirstOrDefault(x=>x.Id == roleManagementVM.ApplicationUser.Id);

                if(roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _applicationDbContext.SaveChanges();

                //Remove old role [need helper method - UserManager
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();

                //Assign new role
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }
           

            return RedirectToAction("Index");
        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> listOfUsers = _applicationDbContext.ApplicationUsers.Include(x=>x.Company).ToList();

            //This will give details from the UserRoles table - DB
            var userRoles = _applicationDbContext.UserRoles.ToList();


            //This will give Roles details from the Roles Table - DB
            var roles = _applicationDbContext.Roles.ToList();

            foreach (var user in listOfUsers)
            {

                //Get the role id from the userRole table
                var roleId = userRoles.FirstOrDefault(x=> x.UserId == user.Id).RoleId;

                //Get the role using roleId from the role table.
                user.Role = roles.FirstOrDefault(x => x.Id == roleId).Name;

                if(user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = "",
                    };
                }
            }

            return Json(new {data = listOfUsers });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            
            var objFromDb = _applicationDbContext.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            if (objFromDb == null)
            {
                return Json (new {success = false, message = "Error while Locking/Unlocking"});
            }

            if(objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _applicationDbContext.SaveChanges();

            return Json(new { success = true, message = "Operation Successfull!" });

        }

        #endregion

    }
}
