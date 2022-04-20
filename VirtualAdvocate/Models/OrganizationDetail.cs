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
    
    public partial class OrganizationDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OrganizationDetail()
        {
            this.RecursiveNotificationDetails = new HashSet<RecursiveNotificationDetail>();
        }
    
        public int OrganizationId { get; set; }
        public string OrgName { get; set; }
        public string OrgPhoneNumber { get; set; }
        public string OrgEmail { get; set; }
        public string OrgStreetName { get; set; }
        public string OrgBuildingName { get; set; }
        public string OrgPlotNo { get; set; }
        public string OrgBlockNo { get; set; }
        public string OrgRegion { get; set; }
        public string OrgLandMark { get; set; }
        public Nullable<int> UserAccountsType { get; set; }
        public Nullable<int> UserId { get; set; }
        public bool IsEnabled { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> OrganizationTypeId { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecursiveNotificationDetail> RecursiveNotificationDetails { get; set; }
    }
}
