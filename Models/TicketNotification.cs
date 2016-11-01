﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketNotification
    {
        // TicketNotification properties
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }

        //TicketNotification navigation for parents
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}