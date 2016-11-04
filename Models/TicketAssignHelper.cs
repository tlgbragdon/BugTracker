using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketAssignHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public int GetStatusId (string status)
        {
            return (db.Statuses.FirstOrDefault(s => s.Name == status).Id);
        }

        public List<Status> ListStatuses()
        {
            return (db.Statuses.ToList());
        }

        public int GetPriorityId(string priority)
        {
            return (db.Priorities.FirstOrDefault(s => s.Name == priority).Id);
        }

        public int GetTicketTypeId(string type)
        {
            return (db.TicketTypes.FirstOrDefault(s => s.Name == type).Id);
        }

        public bool IsUserAssignedToTicket(string userId, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);

            return (ticket.AssignedToId == userId);
        }

        public void AssignDevToTicket(string userId, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);
            ticket.AssignedToId = userId;
            //ticket.

            //db.SaveChanges();
        }

    }
}