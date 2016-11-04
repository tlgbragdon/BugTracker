using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketAssignViewModel
    {
        public Ticket ticket { get; set; }
        public SelectList ticketPriorities { get; set; }
        public SelectList ticketTypes { get; set; }
        public SelectList ticketStatuses { get; set; }
        public SelectList developerList { get; set; }
    }
}