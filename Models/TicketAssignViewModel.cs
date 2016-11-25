using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketAssignViewModel
    {
        public Ticket ticket { get; set; }
        public SelectList ticketPriorities { get; set; }
        public SelectList ticketTypes { get; set; }
        public SelectList ticketStatuses { get; set; }
        public SelectList developerList { get; set; }
        public IEnumerable<TicketAttachment> ticketAttachments { get; set; }
    }

    public class TicketCreateViewModel
    {
        public int projectId { get; set; }
        public string projectName { get; set; }
        [Required]
        [DisplayName("Title")]
        public string ticketTitle { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [DisplayName("Description")]
        public string ticketDescription { get; set; }
        [Required]
        public int typeId { get; set; }
        [Required]
        public int priorityId { get; set; }
        public SelectList ticketPriorities { get; set; }
        public SelectList ticketTypes { get; set; }
        public string attachmentsDescription { get; set; }
    }
}