using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SendGrid;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class NotificationsHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private EmailService email = new EmailService();

        public async Task SendNotification(string userId, int ticketId, string mesg)
        {
            // update notification records
            TicketNotification notification = new TicketNotification();
            notification.TicketId = ticketId;
            notification.UserId = userId;
            notification.Message = mesg;
            notification.Created = DateTime.Now;
            db.TicketNotifications.Add(notification);
            db.SaveChanges();

            // send email to affected user

            IdentityMessage msg = new IdentityMessage();
            Ticket ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketId);
            msg.Destination = db.Users.FirstOrDefault(u => u.Id == userId).Email;
            msg.Subject = ("BugTracker UPDATE: " + mesg);
            msg.Body = string.Format("{0} : {1} <br> Check out all the <a href=\'http://tbragdon-bugtracker.azurewebsites.net/Tickets/Details/{2}\'> details here</a>", mesg, ticket.Title, ticket.Id);
            await email.SendAsync(msg);

        }

        // used on _layout, so needs to be static method
        public static int notificationCount()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(n => n.UserId == userId).Count();
        }
    }
}