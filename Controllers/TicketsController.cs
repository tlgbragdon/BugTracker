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
    [RequireHttps]

    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUser user = new ApplicationUser();
        private ProjectAssignHelper projectHelper = new ProjectAssignHelper();
        private TicketAssignHelper ticketHelper = new TicketAssignHelper();
        private UserRoleAssignHelper roleHelper = new UserRoleAssignHelper();
        private TicketHistoryHelper historyHelper = new TicketHistoryHelper();


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
                // user could be any number of following roles
                if (User.IsInRole("ProjectManager"))
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
                List<Ticket> tempList = new List<Ticket>();
                tempList.AddRange(ProjectMgrTicketList);
                tempList.AddRange(DeveloperTicketList);
                tempList.AddRange(SubmitterTicketList);
                // remove any duplicates from list
                ticketList = tempList.Distinct().ToList();
            }

            ViewBag.Title = "All Tickets";
            return View(ticketList);
        }

        // GET: Tickets of specified Project
        // this ActionResult overloads the Index and uses the same Index View
        [ActionName("PTIndex")]
        [Authorize]
        public ActionResult Index(int projectId)
        {
            List<Ticket> ticketList = new List<Ticket>();
            List<Ticket> DeveloperTicketList = new List<Ticket>();
            List<Ticket> SubmitterTicketList = new List<Ticket>();

            // get current user and project
            Project project = db.Projects.Find(projectId);
            var userId = User.Identity.GetUserId();
            
            if (User.IsInRole("Administrator") || (User.IsInRole("ProjectManager") && (project.OwnerId == userId)))
            {
                // get all tickets in project
                ticketList = project.Tickets.ToList();
            }
            else
            {
                // user could be one or both of the following roles
                if (User.IsInRole("Developer"))
                {
                    DeveloperTicketList = db.Tickets.Where(t=> t.ProjectId == projectId).Where(t => t.AssignedToId == userId).ToList();
                }
                if (User.IsInRole("Submitter"))
                {
                   
                    SubmitterTicketList = db.Tickets.Where(t => t.ProjectId == projectId).Where(t => t.SubmitterId == userId).ToList();
                }
                //combine developer and submitter lists into one

                List<Ticket> tempList = new List<Ticket>();
                tempList.AddRange(DeveloperTicketList);
                tempList.AddRange(SubmitterTicketList);
                // remove any duplicates from list
                ticketList = tempList.Distinct().ToList();
            }
            ViewBag.Title = "Tickets for " + project.Name;

            return View("Index", ticketList);
        }

        // GET: Tickets/UserTickets
        [Authorize(Roles="Administrator")]
        public ActionResult UserTickets(string userId)
        {
            UserRoleAssignHelper roleHelper = new Models.UserRoleAssignHelper();
            AdminUserTicketViewModel viewModel = new AdminUserTicketViewModel();

            viewModel.user = db.Users.Find(userId);
            viewModel.assignedTickets = db.Tickets.Where(t => t.AssignedTo.Id == userId).ToList();
            viewModel.submittedTickets = db.Tickets.Where(t => t.SubmitterId == userId).ToList();
            viewModel.developer = roleHelper.IsUserInRole(userId, "Developer");
            viewModel.submitter = roleHelper.IsUserInRole(userId, "Submitter");

            return View(viewModel);
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
            
            ViewBag.Title = "Create Ticket";

            TicketCreateViewModel viewModel = new TicketCreateViewModel();
            viewModel.projectId = projectId;
            viewModel.projectName = db.Projects.Find(projectId).Name;
            viewModel.ticketPriorities = new SelectList(db.Priorities, "Id", "Name", viewModel.priorityId);
            viewModel.ticketTypes = new SelectList(db.TicketTypes, "Id", "Name", viewModel.typeId);
            // TODO: this doesnt work - trying to add icon to name in selectlist
            //var query = db.TicketTypes.Select(t => new SelectListItem
            //{
            //    Value = t.Id.ToString(),
            //    Text = t.Name + " <i class=" + t.IconName + "></i>",
            //    Selected = false
            //});
            //viewModel.ticketTypes = (SelectList)query.AsEnumerable();

            return View(viewModel);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TicketCreateViewModel model, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.ticketTitle))
                {
                    ModelState.AddModelError("Title", "Must Assign Title to Ticket");
                    return View(model);
                };

                Ticket ticket = new Ticket();
                ticket.ProjectId = model.projectId;
                ticket.Title = model.ticketTitle;
                ticket.Description = model.ticketDescription;
                ticket.TicketTypeId = model.typeId;
                ticket.PriorityId = model.priorityId;
                ticket.Created = DateTime.Now;
                ticket.SubmitterId = User.Identity.GetUserId();
                ticket.StatusId = ticketHelper.GetStatusId("Open");

                // Save Ticket
                db.Tickets.Add(ticket);
                db.SaveChanges();

                // log creation of ticket to history
                var status = historyHelper.Create(ticket.Id, "Ticket", User.Identity.GetUserId());

                //Note: Need to save Ticket to db before creating attachment
                //TODO: make this a separate view/control
                if (uploadedFiles != null)
                {
                    if (uploadedFiles.Count() > 0)
                    {
                        // create separate folders for each project based on project Id
                        string projectPath = "~/uploads/proj" + ticket.ProjectId + "/";
                        System.IO.Directory.CreateDirectory(Server.MapPath(projectPath));

                        foreach (var file in uploadedFiles)
                        {
                            if (file != null)
                            {
                                if (FileUploadValidator.IsValidUploadFile(file))
                                {
                                    var fileName = Path.GetFileName(file.FileName);
                                    string fullFilePath = Path.Combine(Server.MapPath(projectPath), fileName);
                                    file.SaveAs(fullFilePath);

                                    TicketAttachment attachment = new TicketAttachment();
                                    attachment.UserId = ticket.SubmitterId;
                                    attachment.Created = DateTime.Now;
                                    attachment.FileUrl = projectPath + fileName;
                                    attachment.FileType = Path.GetExtension(file.FileName);
                                    attachment.Title = fileName;
                                    attachment.Description = model.attachmentsDescription;
                                    attachment.TicketId = ticket.Id;

                                    // save attachments
                                    db.TicketAttachments.Add(attachment);
                                    db.SaveChanges();
                                    // log creation of attachment to history
                                    status = historyHelper.Create(ticket.Id, "Attachment", User.Identity.GetUserId());
                                }
                                // if specified image is not valid, give user opportunity to try again
                                else
                                {
                                    model.ticketPriorities = new SelectList(db.Priorities, "Id", "Name", model.priorityId);
                                    model.ticketTypes = new SelectList(db.TicketTypes, "Id", "Name", model.typeId);
                                    ViewBag.FileError = file.FileName + " is either too large (> 2MB) or too small (< 10KB). Please try again.";
                                    return View(model);
                                }
                            }
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            model.ticketPriorities = new SelectList(db.Priorities, "Id", "Name", model.priorityId);
            model.ticketTypes = new SelectList(db.TicketTypes, "Id", "Name", model.typeId);
            return View(model);
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
        public ActionResult Edit(TicketAssignViewModel model)
        {

            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();
                Ticket oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == model.ticket.Id);

                var ticket = db.Tickets.Find(model.ticket.Id);
                ticket.Title = model.ticket.Title;
                ticket.Description = model.ticket.Description;
                ticket.TicketTypeId = model.ticket.TicketTypeId;
                ticket.PriorityId = model.ticket.PriorityId;
                ticket.AssignedToId = model.ticket.AssignedToId;
                ticket.Updated = DateTime.Now;

                // if ticket is assigned, but is still marked as Open, update status
                if (!string.IsNullOrEmpty(model.ticket.AssignedToId) && (ticketHelper.GetStatusId("Open") == ticket.StatusId))
                {
                        ticket.StatusId = ticketHelper.GetStatusId("Assigned");
                }
                // if developer assigned to ticket is not already on project, add them now
                if (!string.IsNullOrEmpty(model.ticket.AssignedToId) && !projectHelper.IsUserOnProject(ticket.AssignedToId, ticket.ProjectId))
                {
                        projectHelper.AddUserToProject(ticket.AssignedToId, ticket.ProjectId);
                }

                //log any Ticket changes to history
                historyHelper.logTicketChanges(oldTicket, ticket, userId);

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
