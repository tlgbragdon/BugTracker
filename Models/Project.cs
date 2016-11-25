using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Project
    {
        //Project properties
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Name")] public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        [DisplayName("Media")]
        public string MediaUrl { get; set; }

        [Required]
        [DisplayName("Project Manager") ]
        public string OwnerId { get; set; }

        public bool Archived { get; set; }

        //Project Constructor
        public Project()
        {
            this.Tickets = new HashSet<Ticket>();
            this.AssignedUsers = new HashSet<ApplicationUser>();
            this.Archived = false;
        }

        //Project navigational properties for children
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<ApplicationUser> AssignedUsers { get; set; }




    }
}