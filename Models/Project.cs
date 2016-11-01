using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Project
    {
        //Project properties
        public int Id { get; set; }
        [DisplayName("Project Name")] public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        [DisplayName("Media")] public string MediaUrl { get; set; }
        [DisplayName("Project Manager") ] public string OwnerId { get; set; }

        //Project Constructor
        public Project()
        {
            this.Tickets = new HashSet<Ticket>();
        }

        //Project navigational properties for children
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<ApplicationUser> User { get; set; }


  
    }
}