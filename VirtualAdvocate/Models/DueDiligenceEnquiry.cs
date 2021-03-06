//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VirtualAdvocate.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DueDiligenceEnquiry
    {
        public int EnquiryId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegName { get; set; }
        public string BusinessName { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string CertificateTitleNo { get; set; }
        public string PlotNumber { get; set; }
        public string BlockNumber { get; set; }
        public string Area { get; set; }
        public string Municipality { get; set; }
        public string Region { get; set; }
        public int EnquiryType { get; set; }
        public int UserId { get; set; }
        public Nullable<bool> PaidStatus { get; set; }
        public Nullable<bool> IsEnabled { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> TimeLine { get; set; }
        public Nullable<decimal> Cost { get; set; }
        public string ReportDocument { get; set; }
        public string InvoiceDocument { get; set; }
        public Nullable<bool> ReplyStatus { get; set; }
    }
}
