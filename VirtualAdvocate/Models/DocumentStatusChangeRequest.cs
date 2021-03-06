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
    
    public partial class DocumentStatusChangeRequest
    {
        public int Id { get; set; }
        public int DocumentDetailId { get; set; }
        public int ChangeFrom { get; set; }
        public int ChangeTo { get; set; }
        public bool Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> UserId { get; set; }
    
        public virtual DocumentDetailsStatu DocumentDetailsStatu { get; set; }
        public virtual DocumentDetailsStatu DocumentDetailsStatu1 { get; set; }
        public virtual UserProfile UserProfile { get; set; }
        public virtual DocumentDetail DocumentDetail { get; set; }
    }
}
