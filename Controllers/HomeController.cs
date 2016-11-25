using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            DemoLoginViewModel demoLogin = new Models.DemoLoginViewModel();
            demoLogin.UserRoles = new SelectList(db.Roles, "Name", "Name", "ProjectManager");

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Projects");
            }
            return View(demoLogin);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


    }
}