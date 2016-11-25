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
    [RequireHttps]

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
           
            ViewBag.userName = db.Users.Find(userId).DisplayName;

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
                ViewBag.openTickets = db.Tickets.Where(t => t.ProjectId == project.Id).Where(t => t.Status.Name != "Closed").ToList();
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
            ViewBag.ProjMgrs = new SelectList(pmUsers, "Id", "DisplayName");

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
            var projectNonUsers = projHelper.ListUsersNotOnProject(projId);
                 
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
                return RedirectToAction("Details", new { id = model.project.Id });
            }
            return View(model);
        }

        // GET: Projects/AdminDashboard
        [Authorize(Roles = "Administrator, ProjectManager")]
        public ActionResult AdminDashboard()
        {
            AdminDashboardViewModel dashboardModel = new AdminDashboardViewModel();
            dashboardModel.adminUserName = db.Users.Find(User.Identity.GetUserId()).DisplayName;
            if (User.IsInRole("Administrator"))
            {
                dashboardModel.users = db.Users.AsNoTracking().ToList();
                dashboardModel.projects = db.Projects.AsNoTracking().ToList();
                dashboardModel.unassignedTickets = db.Tickets.Where(t=>t.Status.Name == "Open").AsNoTracking().ToList();
                dashboardModel.assignedTickets = db.Tickets.Where(t => t.Status.Name == "Assigned").AsNoTracking().ToList();
                dashboardModel.closedTickets = db.Tickets.Where(t => t.Status.Name == "Closed").AsNoTracking().ToList();
                DateTime lastWeek = DateTime.Now.AddDays(-7);
                dashboardModel.notifications = db.TicketNotifications.Where(n => n.Created >= lastWeek).AsNoTracking().ToList();
            }
            else // Project Manager
            {
                string userId = User.Identity.GetUserId();
                dashboardModel.users = null;
                dashboardModel.projects = db.Projects.Where(p =>p.OwnerId == userId).AsNoTracking().ToList();
                dashboardModel.unassignedTickets = db.Tickets.Where(t=>t.Project.OwnerId == userId).Where(t => t.Status.Name == "Open").AsNoTracking().ToList();
                dashboardModel.assignedTickets = db.Tickets.Where(t => t.Project.OwnerId == userId).Where(t => t.Status.Name == "Assigned").AsNoTracking().ToList();
                dashboardModel.closedTickets = db.Tickets.Where(t => t.Project.OwnerId == userId).Where(t => t.Status.Name == "Closed").AsNoTracking().ToList();
                DateTime lastWeek = DateTime.Now.AddDays(-7);

                dashboardModel.notifications = null; //.TicketNotifications.Where(n => n.Created >= lastWeek).AsNoTracking().ToList();
            }
            

            return View(dashboardModel);
    }

        // get user-specific data for left navigation menu
        public PartialViewResult UserInfo()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            // The list of Tickets Assigned To and Submitted By the current user
            var SubmittedTickets = db.Tickets.Where(t => t.SubmitterId == user.Id).ToList();
            var AssignedTickets = db.Tickets.Where(t => t.AssignedToId == user.Id).ToList();

            List<Ticket> tempList = new List<Ticket>();
            tempList.AddRange(AssignedTickets);
            tempList.AddRange(SubmittedTickets);
            // remove any duplicates from list
            ViewBag.AllTickets = tempList.Distinct().ToList();

            ViewBag.user = user;
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


        // get user-specific notificaitons and history data for right side-bar
        // space is limited, so only report on daily activity
        public PartialViewResult RtSideBar()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            DateTime yesterday = DateTime.Now.AddDays(-1);
            DateTime thisWeek = DateTime.Now.AddDays(-7);

            if (User.IsInRole("Administrator"))
            {
                ViewBag.Notifications = db.TicketNotifications.Where(n => n.Created >= yesterday).OrderByDescending(n => n.Created).ToList();
                ViewBag.History = db.TicketHistories.Where(n => n.Changed >= yesterday).OrderByDescending(n => n.Changed).ToList();
            }
            else if (User.IsInRole("ProjectManager"))
            {
                ViewBag.Notifications = db.TicketNotifications.Where(n => n.Created >= yesterday).Where(n => n.Ticket.Project.OwnerId == user.Id).OrderByDescending(n => n.Created).ToList();
                ViewBag.History = db.TicketHistories.Where(n => n.Changed >= yesterday).Where(n => n.Ticket.Project.OwnerId == user.Id).OrderByDescending(n => n.Changed).ToList();
            }
            else
            {
                ViewBag.Notifications = db.TicketNotifications.Where(n => n.Created >= thisWeek).Where(n => n.UserId == user.Id).OrderByDescending(n => n.Created).ToList();
                ViewBag.History = db.TicketHistories.Where(n => n.Changed >= thisWeek).Where(h => h.UserId == user.Id).OrderByDescending(n => n.Changed).ToList();
            }

            return PartialView("~/Views/Shared/_RightSideBarPartial.cshtml");
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
