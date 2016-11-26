using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public bool PwdReset { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string ProfileImage { get; set; }
        public string ProfileIcon { get; set; }
        public string ProfileColor { get; set; }


        public ApplicationUser()
        {
            this.Attachments = new HashSet<TicketAttachment>();
            this.Comments = new HashSet<TicketComment>();
            this.ChangesHistory = new HashSet<TicketHistory>();
            this.Notifications = new HashSet<TicketNotification>();
            this.AssignedProjects = new HashSet<Project>();
 
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        // navigational relationship with children

        public virtual ICollection<TicketAttachment>Attachments { get; set; }
        public virtual ICollection<TicketComment>Comments { get; set; }
        public virtual ICollection<TicketHistory>ChangesHistory { get; set; }
        public virtual ICollection<TicketNotification>Notifications { get; set; }
        public virtual ICollection<Project>AssignedProjects { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
      

    }
}