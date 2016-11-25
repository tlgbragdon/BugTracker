using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketType
    {
        //TicketType properties
        public int Id { get; set; }
        [DisplayName("Type")]
        public string Name { get; set; }
        public string IconName { get; set; }
        public string Description { get; set; }
        
        // TickeType constructor
        public TicketType ()
        {
            this.Tickets = new HashSet<Ticket>();
        }

        //TicketType navigation properties for children
        public virtual ICollection<Ticket>Tickets { get; set; }

    }
}