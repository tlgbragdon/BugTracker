using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using System.IO;
using Microsoft.AspNet.Identity;

namespace BugTracker
{
    [RequireHttps]

    public class TicketAttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHistoryHelper historyHelper = new TicketHistoryHelper();

        // GET: TicketAttachments
        public ActionResult Index()
        {
            var ticketAttachments = db.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketAttachments.ToList());
        }

        // GET: TicketAttachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        public ActionResult Create(int ticketId)
        {
            TicketAttachment ticketAttachment = new TicketAttachment();
            ticketAttachment.TicketId = ticketId;
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TicketId,Title,Description")] TicketAttachment ticketAttachment, IEnumerable<HttpPostedFileBase> uploadedFiles)
        {
            if (ModelState.IsValid)
            {
                var projectId = db.Tickets.Find(ticketAttachment.TicketId).ProjectId;
                if (uploadedFiles != null)
                {
                    if (uploadedFiles.Count() > 0)
                    {
                        // create separate folders for each project based on project Id
                       
                        string projectPath = "~/uploads/proj" + projectId + "/";
                        System.IO.Directory.CreateDirectory(Server.MapPath(projectPath));

                        foreach (var file in uploadedFiles)
                        {
                            if (FileUploadValidator.IsValidUploadFile(file))
                            {
                                TicketAttachment attachment = new TicketAttachment();
                                var fileName = Path.GetFileName(file.FileName);
                                file.SaveAs(Path.Combine(Server.MapPath(projectPath), fileName));
                                
                                if (string.IsNullOrEmpty(ticketAttachment.Title))
                                {
                                    attachment.Title = fileName;
                                }
                                else
                                {
                                    attachment.Title = ticketAttachment.Title;
                                }
                                if (string.IsNullOrEmpty(ticketAttachment.Description))
                                {
                                    attachment.Description = fileName;
                                }
                                else
                                {
                                    attachment.Description = ticketAttachment.Description;
                                }
                                attachment.TicketId = ticketAttachment.TicketId;
                                attachment.UserId = User.Identity.GetUserId();
                                attachment.Created = DateTime.Now;
                                attachment.FileUrl = projectPath + fileName;
                                attachment.FileType = Path.GetExtension(file.FileName);
                                attachment.IconName = FileUploadValidator.FileIcon(file.FileName);
                                

                                db.TicketAttachments.Add(attachment);
                                // TODO:  ?? should I save once for each attachment or can I wait until they are all added and then do one save for all?
                                db.SaveChanges();
                                // log creation of attachment to history
                                historyHelper.Create(ticketAttachment.TicketId, "Attachment", User.Identity.GetUserId());

                            }
                            // if specified image is not valid, give user opportunity to try again
                            else
                            {
                                ModelState.AddModelError("TicketAttachment", file + " is either too large (> 2MB) or too small (< 10KB). Please try again.");
                                return View(ticketAttachment);
                            }
                          
                        }
                    }
                }
                return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
            }

            ViewBag.TicketId = ticketAttachment.TicketId;
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description")] TicketAttachment ticketAttachment, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                 
                Ticket ticket = db.TicketAttachments.AsNoTracking().FirstOrDefault(t => t.Id == ticketAttachment.Id).Ticket;
                if (file != null)
                {
                    if (FileUploadValidator.IsValidUploadFile(file))
                    {
                        db.TicketAttachments.Attach(ticketAttachment);

                        string projectPath = "~/uploads/proj" + ticket.ProjectId + "/";
                        var fileName = Path.GetFileName(file.FileName);
                        file.SaveAs(Path.Combine(Server.MapPath(projectPath), fileName));
                        ticketAttachment.FileUrl = projectPath + fileName;
                        ticketAttachment.FileType = Path.GetExtension(file.FileName);
                        ticketAttachment.IconName = FileUploadValidator.FileIcon(file.FileName);

                        if (string.IsNullOrEmpty(ticketAttachment.Title))
                        {
                            ticketAttachment.Title = fileName;
                        }

                        if (string.IsNullOrEmpty(ticketAttachment.Description))
                        {
                            ticketAttachment.Description = fileName;
                        }

                        db.Entry(ticketAttachment).Property("Title").IsModified = true;
                        db.Entry(ticketAttachment).Property("Description").IsModified = true;
                        db.Entry(ticketAttachment).Property("FileUrl").IsModified = true;
                        db.Entry(ticketAttachment).Property("FileType").IsModified = true;
                        db.Entry(ticketAttachment).Property("IconName").IsModified = true;

                        db.Entry(ticketAttachment).Property("Id").IsModified = false;
                        db.Entry(ticketAttachment).Property("TicketId").IsModified = false;
                        db.Entry(ticketAttachment).Property("Created").IsModified = false;
                        db.Entry(ticketAttachment).Property("UserId").IsModified = false;

                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("TicketAttachment", file + " is either too large (> 2MB) or too small (< 10KB). Please try again.");
                        return View(ticketAttachment);
                    }
                }
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }

            // check authorization for deleting attachments:
            // user must be Admin, PM of Project, or creator of attachment
            var currentUser = User.Identity.GetUserId();
            if (User.IsInRole("Administrator") ||
                (User.IsInRole("ProjectManager") && (ticketAttachment.Ticket.Project.OwnerId == currentUser )) ||
                (currentUser == ticketAttachment.UserId))
            {
                return PartialView("_Delete", ticketAttachment);
            }
            else
            {
                return PartialView("_Unauthorized", ticketAttachment);
            }
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            var ticketId = ticketAttachment.Ticket.Id;
            // log the comment deletion to the history
            historyHelper.Create(ticketAttachment.TicketId, "Attachment", ticketAttachment.Title, "Deleted", ticketAttachment.UserId);
  
            db.TicketAttachments.Remove(ticketAttachment);
            db.SaveChanges();
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }



        public FileResult Download(int attachmentId)
        {

            var fileName = db.TicketAttachments.Find(attachmentId).FileUrl;
            //return File(fileName, System.Net.Mime.MediaTypeNames.Application.Octet);
            return File(fileName, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(fileName));
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
