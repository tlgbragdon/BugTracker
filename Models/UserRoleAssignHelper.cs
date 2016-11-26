using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class UserRoleAssignHelper
    {
        private UserManager<ApplicationUser> userManager = 
                    new UserManager<ApplicationUser>(
                         new UserStore<ApplicationUser>(new ApplicationDbContext()));
        private ApplicationDbContext db = new ApplicationDbContext();

        // return whether user is in specified role
        public bool IsUserInRole(string userId, string roleName)
        {
            return userManager.IsInRole(userId, roleName);
        }

        // return user's list of roles
        public ICollection<string> ListUserRoles (string userId)
        {
            return userManager.GetRoles(userId);
        }

        // add user to specified role
        public bool AddUserToRole(string userId, string roleName)
        {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        // remove user from role
        public bool RemoveUserFromRole (string userId, string roleName)
        {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        // return all users in a role
        public ICollection<ApplicationUser> UsersInRole (string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (IsUserInRole(user.Id, roleName))
                    resultList.Add(user);
            }
            return resultList;
        }

        //return all users not in specified role
        public ICollection<ApplicationUser> UsersNotInRole (string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();

            foreach (var user in List)
            {
                if (!IsUserInRole(user.Id, roleName))
                    resultList.Add(user);
            }
            return resultList;
        }

    }
}