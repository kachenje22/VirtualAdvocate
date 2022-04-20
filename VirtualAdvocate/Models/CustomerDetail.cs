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
    
    public partial class CustomerDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CustomerDetail()
        {
            this.ProbationDetails = new HashSet<ProbationDetail>();
        }
    
        public int CustomerId { get; set; }
        public string EmailAddress { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string Address { get; set; }
        public int OrganizationId { get; set; }
        public string BankName { get; set; }
        public Nullable<bool> IsEnabled { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> Department { get; set; }
        public virtual CustomerDetail CustomerDetails1 { get; set; }
        public virtual CustomerDetail CustomerDetail1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProbationDetail> ProbationDetails { get; set; }
    }
}