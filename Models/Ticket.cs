﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public int ProjectId { get; set; }
        [DisplayName("Type")]
        public int TicketTypeId { get; set; }
        [Required]
        [DisplayName("Priority")]
        public int PriorityId { get; set; }
        [DisplayName("Status")]
        public int StatusId { get; set; }
        [DisplayName("Submitter")]
        public string SubmitterId { get; set; }
        [DisplayName("Developer")]
        public string AssignedToId { get; set; }


        //Ticket Constructor
        public Ticket ()
        {
            this.TicketAttachments = new HashSet<TicketAttachment>();
            this.TicketComments = new HashSet<TicketComment>();
            this.TicketHistories = new HashSet<TicketHistory>();
            this.TicketNotifications = new HashSet<TicketNotification>();
        }

        //Ticket navigational properties for children
        public virtual ICollection<TicketAttachment>TicketAttachments { get; set; }
        public virtual ICollection<TicketComment>TicketComments{ get; set; }
        public virtual ICollection<TicketHistory>TicketHistories { get; set; }
        public virtual ICollection<TicketNotification>TicketNotifications { get; set; }

        //Ticket navigation for parents
        public virtual Project Project { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Status Status { get; set; }

        // connect Ticket to Users
        [DisplayName("Submitter")]
        public virtual ApplicationUser Submitter { get; set; }
        [DisplayName("Assigned To")]
        public virtual ApplicationUser AssignedTo { get; set; }

    }
}