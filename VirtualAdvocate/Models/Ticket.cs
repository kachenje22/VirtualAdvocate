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
    
    public partial class Ticket
    {
        public long ID { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BusinessImpact { get; set; }
        public string Organization { get; set; }
        public string Issue { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedOn { get; set; }
    }
}
