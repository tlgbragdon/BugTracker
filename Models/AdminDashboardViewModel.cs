using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class AdminDashboardViewModel
    {
        public string adminUserName { get; set; }
        public List<ApplicationUser> users { get; set; }
        public List<string> roles { get; set; }
        public List<Project> projects { get; set; }
        public List<Ticket> unassignedTickets { get; set; }
        public List<Ticket> assignedTickets { get; set; }
        public List<Ticket> closedTickets { get; set; }
        public List<TicketNotification> notifications { get; set; }
        
    }
}