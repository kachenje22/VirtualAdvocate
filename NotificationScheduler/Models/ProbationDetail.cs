//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NotificationScheduler.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProbationDetail
    {
        public int Id { get; set; }
        public System.DateTime DateOfJoining { get; set; }
        public int ProbationPeriod { get; set; }
        public System.DateTime DateOfExpiry { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool Status { get; set; }
        public Nullable<int> UserId { get; set; }
    
        public virtual CustomerDetail CustomerDetail { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}