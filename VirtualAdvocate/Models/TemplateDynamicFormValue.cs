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
    
    public partial class TemplateDynamicFormValue
    {
        public int RowId { get; set; }
        public int UserId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateKey { get; set; }
        public string UserInputs { get; set; }
        public bool IsEnabled { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public int CustomerId { get; set; }
        public string ParentkeyId { get; set; }
    }
}
