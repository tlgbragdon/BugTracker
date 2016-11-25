using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        // TicketHistory properties
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Property { get; set; }
        [DisplayName("Old Value")]
        public string OldValue { get; set; }
        [DisplayName("New Value")]
        public string NewValue { get; set; }
        public string UserId { get; set; }
        public DateTime Changed { get; set; }

        //TicketHistory navigation for parent
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}