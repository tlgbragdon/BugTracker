using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectAssignHelper projHelper = new ProjectAssignHelper();
        private UserRoleAssignHelper roleHelper = new UserRoleAssignHelper();

        // GET: Projects
        [Authorize]
        public ActionResult Index()
        {
            // get list of projects the user is assigned to
            var userId = User.Identity.GetUserId();
            var projList = projHelper.ListUserProjects(userId); 

            // if admin role, view all projects instead 
            if (roleHelper.IsUserInRole(userId, "Administrator"))
            {
                projList = db.Projects.ToList();
            }

            return View(projList);
        }

        // GET: Projects
        [Authorize(Roles ="Administrator, ProjectManager")]
        public ActionResult AllIndex()
        {
            var projList = db.Projects.ToList();
            return View(projList);
        }


        // GET: Projects/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            // users can only see details for projects they are assigned to 
            // unless they are admin and can see all projects
            var userId = User.Identity.GetUserId();
            UserRoleAssignHelper roleHelper = new UserRoleAssignHelper();
            ProjectAssignHelper projHelper = new ProjectAssignHelper();
            if (roleHelper.IsUserInRole(userId, "Administrator") ||
                projHelper.IsUserOnProject(userId, project.Id) )
            {
                ViewBag.ownerName = db.Users.Find(project.OwnerId).DisplayName;
                ViewBag.assignedUsers = projHelper.ListUsersOnProject(project.Id);
                return View(project);
            }
            else // if access not allowed, return to main project listing
            {
                return RedirectToAction("Index");
            }
        }

        // GET: Projects/Create
        [Authorize (Roles = "Administrator, ProjectManager")]
        public ActionResult Create()
        {
            ViewBag.Title = "Create Project";

            var pmUsers = roleHelper.UsersInRole("ProjectManager");
            ViewBag.ProjMgrs = new SelectList(pmUsers, "Id", "FirstName");

            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,OwnerId,MediaUrl")] Project project, HttpPostedFileBase imgFile)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(project.Name))
                {
                    ModelState.AddModelError("Name", "Must Assign Name to Project");
                    return View(project);
                };

                project.Created = DateTime.Now;
            
                if (imgFile != null)
                {
                    if (ImageUploadValidator.IsWebFriendlyImage(imgFile))
                    {
                        var fileName = Path.GetFileName(imgFile.FileName);
                        imgFile.SaveAs(Path.Combine(Server.MapPath("~/images/"), fileName));
                        project.MediaUrl = "~/images/" + fileName;
                    }
                    // if specified image is not valid, give user opportunity to try again
                    else
                    {
                        ModelState.AddModelError("MediaUrl", imgFile.FileName + " is either too large (> 2MB) or too small (< 10KB). Please try again.");
                        return View(project);
                    }
                 }

                db.Projects.Add(project);
                db.SaveChanges();

                // Now that we have the project,
                // add the owner (Project Manager) to project
                if (project.OwnerId != null)
                {
                    projHelper.AddUserToProject(project.OwnerId, project.Id);
                }
                return RedirectToAction("Index");
            }
            // return User to Create view if there are any issues with model
            return View(project);
        }


        // GET: Projects/Edit
        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult Edit(int projId)
        {
            if (projId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(projId);
            if (project == null)
            {
                return HttpNotFound();
            }

            ProjectAssignViewModel viewModel = new ProjectAssignViewModel();
            //viewModel.project = new Project();
            viewModel.project = db.Projects.Find(projId);

            var pmUsers = roleHelper.UsersInRole("ProjectManager");
            viewModel.projectMgrs = new SelectList(pmUsers, "Id", "DisplayName");


            var projectUsers = projHelper.ListUsersOnProject(projId);
            //.Select(u => new
            //{
            //    RemoveId = u.Id,
            //    DisplayName = u.FirstName + " " + u.LastName
            //}).ToList();

            var projectNonUsers = projHelper.ListUsersNotOnProject(projId);
            //.Select(u => new
            //{
            //    AddId = u.Id,
            //    DisplayName = u.FirstName + " " + u.LastName
            //}).ToList();

            
            viewModel.usersNotOnProject = new MultiSelectList(projectNonUsers, "Id", "DisplayName");
            viewModel.usersOnProject = new MultiSelectList(projectUsers, "Id", "DisplayName");

            return View(viewModel);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( ProjectAssignViewModel model, HttpPostedFileBase imgFile)
        {
            if (ModelState.IsValid)
            {

                var proj = db.Projects.Find(model.project.Id);
                
                proj.Name = model.project.Name;
                proj.Description = model.project.Description;
                // update project manager if that has changed
                if (proj.OwnerId != model.project.OwnerId)
                {
                    // remove old proj mgr from Assignment list
                    projHelper.RemoveUserFromProject(proj.OwnerId, proj.Id);
                    // set new proj mgr as Owner of project and add that person to Project
                    proj.OwnerId = model.project.OwnerId;
                    projHelper.AddUserToProject(model.project.OwnerId, model.project.Id);
                };

                // check file validity and update 
                if (imgFile != null)
                {
                    if (ImageUploadValidator.IsWebFriendlyImage(imgFile))
                    {
                        var fileName = Path.GetFileName(imgFile.FileName);
                        imgFile.SaveAs(Path.Combine(Server.MapPath("~/images/"), fileName));
                        proj.MediaUrl = "~/images/" + fileName;
                    }
                    else
                    // if specified image is not valid, give user opportunity to try again
                    {
                        ModelState.AddModelError("MediaUrl", imgFile.FileName + " is either too large (> 2MB) or too small (< 10KB). Please try again.");
                        //ViewBag.FileError = imgFile + " is either too large (> 2MB) or too small (< 10KB). Please try again.";
                        return View(model);
                    }
                }
              
                proj.Updated = DateTime.Now;

                if (model.usersToAdd != null)
                {
                    foreach (var userId in model.usersToAdd)
                    {
                       projHelper.AddUserToProject(userId, model.project.Id);
                    }
                }
                if (model.usersToRemove != null)
                {
                    foreach (var userId in model.usersToRemove)
                    {
                        projHelper.RemoveUserFromProject(userId, model.project.Id);
                    }
                }

                db.Entry(proj).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public PartialViewResult UserInfo()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            ViewBag.DisplayName = user.DisplayName;
            List<string> roleList = new List<string>();
            foreach (var role in db.Roles)
            {
                if (User.IsInRole(role.Name))
                {
                    roleList.Add(role.Name);
                }
            }
            ViewBag.roles = roleList;

            return PartialView("~/Views/Shared/_LeftNavPartial.cshtml");

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
