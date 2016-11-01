using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Priority
    {
        //Priority properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        //TODO: Priority Constructor

      // navigation to children
      public virtual ICollection<Ticket> Tickets { get; set; }
    
    }
}