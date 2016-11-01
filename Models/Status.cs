using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Status
    {
        //Status properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //Constructor
        public Status()
        {
            this.Tickets = new HashSet<Ticket>();
        }

        //navigation to Status children
        public virtual ICollection<Ticket> Tickets { get; set; }


    }
}