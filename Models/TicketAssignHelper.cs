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

        public string Status(int? statusId)
        {
            if (statusId != null)
            {
                if (statusId == 0)
                {
                    return "Unassigned";
                }
                else
                return (db.Statuses.Find(statusId).Name);
            }
            return "";
        }

        public List<Status> ListStatuses()
        {
            return (db.Statuses.ToList());
        }

        public int GetPriorityId(string priority)
        {
            return (db.Priorities.FirstOrDefault(s => s.Name == priority).Id);
        }

        public string Priority(int priorityId)
        {
                return (db.Priorities.Find(priorityId).Name);
        }

        public int GetTicketTypeId(string type)
        {
            return (db.TicketTypes.FirstOrDefault(s => s.Name == type).Id);
        }

        public string TicketType(int typeId)
        {
            return (db.TicketTypes.Find(typeId).Name);
        }

        public bool IsUserAssignedToTicket(string userId, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);

            return (ticket.AssignedToId == userId);
        }

        public bool IsUserTicketSubmitter(string userId, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);

            return (ticket.SubmitterId == userId);
        }

        // return number of tickets submitted by specified user
        public int SubmittedTicketsCount(string userId)
        {
            List<Ticket> tickets = db.Tickets.Where(t => t.SubmitterId == userId).ToList();
            return tickets.Count();
            //int count = 0;
            //foreach (var ticket in tickets)
            //{
            //    if (ticket.SubmitterId == userId)
            //    { count++; }
            //}
            //return count;
        }

        // return number of tickets submitted by specified user in specified project
        public int SubmittedTicketsCount(string userId, int projectId)
        {
            Project project = db.Projects.Find(projectId);
            List<Ticket> tickets = db.Tickets.ToList();
            int count = 0;
            foreach (var ticket in project.Tickets)
            {
                if (ticket.SubmitterId == userId)
                { count++; }
            }
            return count;
        }

    }
}