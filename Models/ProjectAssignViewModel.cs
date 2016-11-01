using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class ProjectAssignViewModel
    {
        public Project project { get; set; }
        //public int projectId { get; set; }
        //public List<ApplicationUser> projectUsers { get; set; }
        //public List<ApplicationUser> projectNonUsers { get; set; }
        public MultiSelectList usersOnProject { get; set; }
        public string[] usersToRemove { get; set; }
        public MultiSelectList usersNotOnProject { get; set; }
        public string[] usersToAdd { get; set; }
        public SelectList projectMgrs { get; set; }

        
    }
}

