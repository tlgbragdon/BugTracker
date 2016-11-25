using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketComment
    {
        //TicketComment properties
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        //TicketComment navigation for parent
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}