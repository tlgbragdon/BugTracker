using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class UserRoleViewModel
    {
        public ApplicationUser user { get; set; }
        public List<string> role { get; set; }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public MultiSelectList UserRoles { get; set; }
        public string[] SelectedRoles { get; set; }
    }
}