using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        // TicketAttachment properties
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Update { get; set; }
        [DisplayName("File to Attach")]
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public string IconName { get; set; }

        // TicketAttachment parent navigation
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}