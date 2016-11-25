using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class AdminUserViewModel
    {
        public ApplicationUser user { get; set; }
        public List<string> roles { get; set; }
    }

    public class AdminUserTicketViewModel
    {
        public ApplicationUser user { get; set; }
        public List<Ticket> assignedTickets { get; set; }
        public List<Ticket> submittedTickets { get; set; }
        public Boolean developer { get; set; }
        public Boolean submitter { get; set; }
    }
}