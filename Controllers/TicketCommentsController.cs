using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    [RequireHttps]

    public class TicketCommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectAssignHelper projectHelper = new ProjectAssignHelper();
        private TicketHistoryHelper historyHelper = new TicketHistoryHelper();

        // GET: TicketComments
        public ActionResult Index(int ticketId)
        {
            // anyone on the project can view tickets for the project
            var userId = User.Identity.GetUserId();
            Ticket ticket = db.Tickets.Find(ticketId);
            List<TicketComment> ticketComments;

            if (User.IsInRole("Administrator") ||
                (projectHelper.IsUserOnProject(userId, ticket.ProjectId) &&
                    (User.IsInRole("ProjectManager") || (ticket.SubmitterId == userId) || (ticket.AssignedToId == userId))))
            {
                //ticketComments = db.TicketComments.Where(c => c.TicketId == ticketId).ToList();
                // order by newest comments first
                ticketComments = ticket.TicketComments.OrderByDescending (c=>c.Created).ToList();
                return View(ticketComments);
            }
            // if user doesn't have access to this ticket, return list of tickets for project
            return RedirectToAction("PTIndex", "Tickets", new { projectId = ticket.ProjectId });
        }

        // GET: TicketComments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            return View(ticketComment);
        }

        // GET: TicketComments/Create
        [Authorize]
        public ActionResult Create(int ticketId)
        {
            TicketComment ticketComment = new TicketComment();
            ticketComment.TicketId = ticketId;
            
            return View();
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TicketId,Body")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                var ticket = db.Tickets.Find(ticketComment.TicketId);
                ticketComment.UserId = User.Identity.GetUserId();
                var pm = User.IsInRole("ProjectManager");
                var dev = User.IsInRole("Developer");
 
                // verify user has authority to comment on this ticket
                if (User.IsInRole("Administrator") ||
                       (User.IsInRole("ProjectManager") && (ticketComment.UserId == ticket.Project.OwnerId)) ||
                       (User.IsInRole("Developer") && (ticketComment.UserId == ticket.AssignedToId)) ||
                       (User.IsInRole("Submitter") && (ticketComment.UserId == ticket.SubmitterId)))
                {

                    ticketComment.Created = DateTime.Now;
                    ticketComment.UserId = User.Identity.GetUserId();

                    db.TicketComments.Add(ticketComment);
                    db.SaveChanges();
                    var status = historyHelper.Create(ticketComment.TicketId, "Comment", ticketComment.UserId);
                    return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
                }
                else
                {
                    ModelState.AddModelError("Body", " You are not authorized to add comments to this ticket.");
                };
            }

            return View(ticketComment);
        }

        // GET: TicketComments/Edit/
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            // check authorization for deleting comments:
            // user must be Admin, PM of Project, or creator of comment
            var currentUser = User.Identity.GetUserId();
            if (User.IsInRole("Administrator") ||
                (User.IsInRole("ProjectManager") && (ticketComment.Ticket.Project.OwnerId == currentUser)) ||
                (currentUser == ticketComment.UserId))
            {
                return View(ticketComment);
            }
            else
            {
                ModelState.AddModelError("Body", " You are not authorized to add comments to this ticket.");
                return View();            }
        }

        // POST: TicketComments/Edit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,Body")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                TicketComment oldComment = db.TicketComments.AsNoTracking().FirstOrDefault(c =>c.Id ==ticketComment.Id);
                ticketComment.Updated = DateTime.Now;
                db.TicketComments.Attach(ticketComment);
                db.Entry(ticketComment).Property("Body").IsModified = true;
                db.Entry(ticketComment).Property("Updated").IsModified = true;
                db.Entry(ticketComment).Property("Created").IsModified = false;
                db.Entry(ticketComment).Property("TicketId").IsModified = false;
                db.Entry(ticketComment).Property("UserId").IsModified = false;
                db.Entry(ticketComment).Property("Id").IsModified = false;
                db.SaveChanges();
                // log any changes to the comment
                // comment Body is the only field we need to check
                if (!oldComment.Body.Equals(ticketComment.Body))
                {
                    historyHelper.Create(oldComment.TicketId, "Comment Description", oldComment.Body, ticketComment.Body, oldComment.UserId);
                }

                return RedirectToAction("Details", "Tickets", new { id = oldComment.TicketId });
            }
           
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }

            // check authorization for deleting comments:
            // user must be Admin, PM of Project, or creator of comment
            var currentUser = User.Identity.GetUserId();
            if (User.IsInRole("Administrator") ||
                (User.IsInRole("ProjectManager") && (ticketComment.Ticket.Project.OwnerId == currentUser)) ||
                (currentUser == ticketComment.UserId))
            {
                return PartialView("_Delete", ticketComment);
            }
            else
            {
                return PartialView("_Unauthorized", ticketComment);
            }
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketComment ticketComment = db.TicketComments.Find(id);
            var ticketId = ticketComment.TicketId;
            // log the comment deletion to the history
            historyHelper.Create(ticketComment.TicketId, "Comment", ticketComment.Body, "Deleted", ticketComment.UserId);

            db.TicketComments.Remove(ticketComment);
            db.SaveChanges();

            return RedirectToAction("Details", "Tickets", new { id = ticketId });
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
