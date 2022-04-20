using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualAdvocate.Models
{
    public class DocumentDetailsViewModel
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Document field is required")]
        public int DocumentId { get; set; }

        public string Name { get; set; }
        public string Documentname { get; set; }
        //[Required(ErrorMessage = "Vendor field is required")]
        public string Vendor { get; set; }
        
        public string Purpose { get; set; }

        //[Required(ErrorMessage = "Date Handed field is required")]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        //public System.DateTime DateHanded { get; set; }

        //[Required(ErrorMessage = "Date field is required")]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        //public System.DateTime DateToBeSubmitted { get; set; }
        
        public string DateHanded { get; set; }
        
        public string DateToBeSubmitted { get; set; }

        public SelectList DocumentStatus { get; set; }
        public string RejectionReason { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool Status { get; set; }
        public int DelayedBy { get; set; }
        public int DocumentStatusId { get; set; }
    }

    public enum DocumentDetailStatus
    {
        Pending = 1,
        PendingApproval,
        Complete,
        Reject,
        Accept
    }

    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ChangeStatusParam
    {
        public int DocumentId { get; set; }
        public int StatusId { get; set; }
        public int ChangeFrom { get; set; }
        public string RejectionReason { get; set; }
    }
}