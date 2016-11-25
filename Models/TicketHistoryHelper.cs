using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketHistoryHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketAssignHelper ticketHelper = new TicketAssignHelper();
        private ProjectAssignHelper projectHelper = new ProjectAssignHelper();
        private NotificationsHelper notificationHelper = new NotificationsHelper();


        public Ticket OriginalTicket(int id)
        {
            return db.Tickets.FirstOrDefault(t => t.Id == id);
        }

        public void logTicketChanges(Ticket oldTicket, Ticket newTicket, string userId)
        {
            bool status;
            bool ticketModified = false;

            if (oldTicket.AssignedToId != newTicket.AssignedToId)
            {
                status = Create(oldTicket.Id, "Ticket Assigned To", oldTicket.AssignedToId == null ? "Unassigned" : db.Users.Find(oldTicket.AssignedToId).DisplayName, newTicket.AssignedToId == null ? "Unassigned" : db.Users.Find(newTicket.AssignedToId).DisplayName, userId);
                if (newTicket.AssignedToId != null)
                {
                    notificationHelper.SendNotification(newTicket.AssignedToId, oldTicket.Id, "Ticket Assigned To You");
                }
                if (oldTicket.AssignedToId != null)
                {
                    notificationHelper.SendNotification(oldTicket.AssignedToId, oldTicket.Id, "Ticket Reassigned to Another Developer");
                }
            }
            // if developer assigned to ticket is not already on project, add them now
            if (!string.IsNullOrEmpty(newTicket.AssignedToId) && !projectHelper.IsUserOnProject(oldTicket.AssignedToId, oldTicket.ProjectId))
            {
                status = Create(oldTicket.Id, "Assigned To Project", "Not Assigned to Project", db.Users.Find(newTicket.AssignedToId).DisplayName, userId);
                //projectHelper.AddUserToProject(ticket.AssignedToId, ticket.ProjectId);
            }

            if (!oldTicket.Title.Equals(newTicket.Title))
            {
                status = Create(oldTicket.Id, "Ticket Title", oldTicket.Title, newTicket.Title, userId);
                ticketModified = true;
            }
            if (!oldTicket.Description.Equals(newTicket.Description))
            {
                status = Create(oldTicket.Id, "Ticket Description", oldTicket.Description, newTicket.Description, userId);
                ticketModified = true;
            }
            if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
            {
                status = Create(oldTicket.Id, "Ticket Type", ticketHelper.TicketType(oldTicket.TicketTypeId), ticketHelper.TicketType(newTicket.TicketTypeId), userId);
                ticketModified = true;
            }
            if (oldTicket.PriorityId != newTicket.PriorityId)
            {
                status = Create(oldTicket.Id, "Ticket Priority", ticketHelper.Priority(oldTicket.PriorityId), ticketHelper.Priority(newTicket.PriorityId), userId);
                ticketModified = true;
            }
            if (oldTicket.StatusId != newTicket.StatusId)
            {
                status = Create(oldTicket.Id, "Ticket Status", ticketHelper.Status(oldTicket.StatusId), ticketHelper.Status(newTicket.StatusId), userId);
                ticketModified = true;
            }

            if (ticketModified)
            {
                if (newTicket.AssignedToId != null)
                {
                    notificationHelper.SendNotification(newTicket.AssignedToId, oldTicket.Id, "Ticket You are Assigned to has been Modified");
                }
            }

            db.SaveChanges();


            // notify developer(s) of changes
        }

        // TicketHistories/Create
        // log history for ticket, attachment, or comment creation
        public bool Create(int ticketId, string property, string userId)
        {
            string ticketAssignedId = db.Tickets.AsNoTracking().FirstOrDefault(t=>t.Id == ticketId).AssignedToId;
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketId = ticketId;
            ticketHistory.Property = property;
            ticketHistory.NewValue = "Created";
            ticketHistory.Changed = DateTime.Now;
            ticketHistory.UserId = userId;

            db.TicketHistories.Add(ticketHistory);
            db.SaveChanges();
            // send notification of creation
            if (ticketAssignedId != null)
            {
                notificationHelper.SendNotification(ticketAssignedId, ticketId, property + " Created");
            }
            return true;
        }

        // TicketHistories/Create
        // log history for ticket modification
        public bool Create(int ticketId, string property, string oldValue, string newValue, string userId)
        {
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.TicketId = ticketId;
            ticketHistory.Property = property;
            ticketHistory.OldValue = oldValue;
            ticketHistory.NewValue = newValue;
            ticketHistory.Changed = DateTime.Now;
            ticketHistory.UserId = userId;

            db.TicketHistories.Add(ticketHistory);
            db.SaveChanges();
            return true;
        }

    }
}