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
    
    public partial class TemplateKeyword
    {
        public int TemplateKeyId { get; set; }
        public Nullable<int> TemplateKeyCategory { get; set; }
        public string TemplateKeyValue { get; set; }
        public string TemplateKeyLabels { get; set; }
        public string TemplateKeyDescription { get; set; }
        public bool IsEnabled { get; set; }
        public bool MultipleKeys { get; set; }
        public bool AddedByClient { get; set; }
        public string KeyCategoryName { get; set; }
        public Nullable<bool> SecurityAlert { get; set; }
        public bool TextArea { get; set; }
        public bool BigTextArea { get; set; }
        public bool Cloned { get; set; }
        public Nullable<int> ClonedFrom { get; set; }
        public Nullable<bool> IsAssetName { get; set; }
    }
}
