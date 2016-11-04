using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System.IO;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUser user = new ApplicationUser();
        private TicketAssignHelper ticketHelper = new TicketAssignHelper();
        private UserRoleAssignHelper roleHelper = new UserRoleAssignHelper();

        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            //var tickets = db.Tickets.Include(t => t.AssignedTo).Include(t => t.Priority).Include(t => t.Project).Include(t => t.Status).Include(t => t.Submitter).Include(t => t.TicketType);
            List<Ticket> ticketList = new List<Ticket>();
            List<Ticket> ProjectMgrTicketList = new List<Ticket>();
            List<Ticket> DeveloperTicketList = new List<Ticket>();
            List<Ticket> SubmitterTicketList = new List<Ticket>();

            if (User.IsInRole("Administrator"))
            {
                // get all tickets
                ticketList = db.Tickets.ToList();
            }
            else
            {
                List<Ticket> tempList = new List<Ticket>();
                // user could be any number of following roles
                if (User.IsInRole("ProgramManager"))
                {
                    foreach (var project in db.Projects.Where (p => p.OwnerId == userId))
                    {
                        ProjectMgrTicketList.AddRange(project.Tickets.ToList());
                    }
                }
                if (User.IsInRole("Developer"))
                {
                    DeveloperTicketList = db.Tickets.Where(t => t.AssignedToId == userId).ToList();
                }
                if (User.IsInRole("Submitter"))
                {
                    SubmitterTicketList = db.Tickets.Where(t => t.SubmitterId == userId).ToList();
                }
                //combine all lists into one
                tempList.AddRange(ProjectMgrTicketList);
                tempList.AddRange(DeveloperTicketList);
                tempList.AddRange(SubmitterTicketList);
                // remove any duplicates from list
                ticketList = tempList.Distinct().ToList();
            }
            
            return View(ticketList);
        }

        // GET: Tickets/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles="Submitter")]
        public ActionResult Create(int projectId)
        {
            if (projectId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.Title = "Create Ticket";

            Ticket ticket = new Ticket();
            ticket.ProjectId = projectId;

            TicketAssignViewModel viewModel = new TicketAssignViewModel();
            viewModel.ticket = ticket;
            viewModel.ticketPriorities = new SelectList(db.Priorities, "Id", "Name", ticket.PriorityId);
            viewModel.ticketTypes = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            return View(viewModel);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,ProjectId,TicketTypeId,PriorityId,StatusId")] Ticket ticket, List<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(ticket.Title))
                {
                    ModelState.AddModelError("Title", "Must Assign Title to Ticket");
                    return View(ticket);
                };

                ticket.Created = DateTime.Now;
                ticket.SubmitterId = User.Identity.GetUserId();
                
                ticket.StatusId = ticketHelper.GetStatusId("Open");

                ////TODO: make this a separate view/control
                //if (files.Count > 0)
                //{
                //    // create separate folders for each project based on project Id
                //    string projectPath = "~/uploads/proj" + ticket.ProjectId;
                //    System.IO.Directory.CreateDirectory(Server.MapPath(projectPath));

                //    foreach (var file in files)
                //    {
                //        if (ImageUploadValidator.IsWebFriendlyImage(file))
                //        {
                //            var fileName = Path.GetFileName(file.FileName);
                //            string fullFilePath = Path.Combine(Server.MapPath(projectPath), fileName);
                //            file.SaveAs(fullFilePath);

                //            TicketAttachment attachment = new TicketAttachment();
                //            attachment.UserId = ticket.SubmitterId;
                //            attachment.Created = DateTime.Now;
                //            attachment.FileUrl = fullFilePath;
                //            //TODO: not sure what to do for attachment Title or Description ??
                //            db.TicketAttachments.Add(attachment);
                //            db.SaveChanges();
                //        }
                //        // if specified image is not valid, give user opportunity to try again
                //        else
                //        {
                //            ModelState.AddModelError("TicketAttachments", file.FileName + " is either too large (> 2MB) or too small (< 10KB). Please try again.");
                //            return View(ticket);
                //        }
                //    }
                //}
                
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // need to recreate the ViewBag properties
            ViewBag.Priorities = new SelectList(db.Priorities, "Id", "Name");
            ViewBag.TicketTypes = new SelectList(db.TicketTypes, "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Edit Ticket";
            TicketAssignViewModel viewModel = new TicketAssignViewModel();
            viewModel.ticket = ticket;
            viewModel.ticketPriorities = new SelectList(db.Priorities, "Id", "Name", ticket.PriorityId);
            viewModel.ticketStatuses = new SelectList(db.Statuses, "Id", "Name", ticket.StatusId);
            viewModel.ticketTypes = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            viewModel.developerList = new SelectList(roleHelper.UsersInRole("Developer"), "Id", "DisplayName", ticket.AssignedToId);
            
            return View(viewModel);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TicketAssignViewModel model) //TODO: add attachments  , HttpPostedFileBase imgFile)
        {
            if (ModelState.IsValid)
            {
                var ticket = db.Tickets.Find(model.ticket.Id);
                ticket.Title = model.ticket.Title;
                ticket.Description = model.ticket.Description;
                ticket.TicketTypeId = model.ticket.TicketTypeId;
                ticket.StatusId = model.ticket.StatusId;
                ticket.PriorityId = model.ticket.PriorityId;
                ticket.AssignedToId = model.ticket.AssignedToId;

                ticket.Updated = DateTime.Now;

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Details", new { id = ticket.Id });
            }
            
            return View(model);

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
