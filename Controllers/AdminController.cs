using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRoleAssignHelper userRole = new UserRoleAssignHelper();
        private ProjectAssignHelper projHelper = new ProjectAssignHelper();

        // GET: Admin/Index
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            List<AdminUserViewModel> userList = new List<AdminUserViewModel>();
            foreach (var user in db.Users.ToList()) 
            {
                var userInfo = new  AdminUserViewModel();
                userInfo.user = user;
                userInfo.roles = userRole.ListUserRoles(user.Id).ToList();
                userList.Add(userInfo);
            }
            return View(userList);
        }


        //GET: SelectRole
        [Authorize(Roles = "Administrator")]
        public ActionResult SelectRole (string userId)
        {
            var user = db.Users.Find(userId);
            var roleUser = new UserRoleViewModel();
            roleUser.Id = userId;
            roleUser.FirstName = user.FirstName;
            roleUser.LastName = user.LastName;
            roleUser.SelectedRoles = userRole.ListUserRoles(user.Id).ToArray();
            roleUser.UserRoles = new MultiSelectList(db.Roles, "Name", "Name", roleUser.SelectedRoles);

            return View(roleUser);
        }

        //POST: SelectRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectRole(UserRoleViewModel model)
        {

                var user = db.Users.Find(model.Id);
                foreach (var roleName in db.Roles.Select(r => r.Name).ToList())
                {
                    userRole.RemoveUserFromRole(model.Id, roleName);
                }
                foreach (var roleadd in model.SelectedRoles)
                {
                    userRole.AddUserToRole(model.Id, roleadd);
                }
                return RedirectToAction("Index");
        }

    }
}