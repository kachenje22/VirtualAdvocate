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
    
    public partial class LogOrganizationDetail
    {
        public int LogId { get; set; }
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public string OrgPhoneNumber { get; set; }
        public string OrgStreetName { get; set; }
        public string OrgBlockNumber { get; set; }
        public string OrgPlotNumber { get; set; }
        public string OrgRegion { get; set; }
        public string OrgLandmark { get; set; }
        public string OrgBuildingName { get; set; }
        public int OrganizationTypeId { get; set; }
        public Nullable<int> UserAccountType { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public int ModifierId { get; set; }
    }
}
