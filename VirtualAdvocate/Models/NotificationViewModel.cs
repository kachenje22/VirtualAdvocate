using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class NotificationViewModel
    {
        public TemplateType Title { get; set; }
        public int Prior { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int PriorToExpiry { get; set; }
        public int OnExpiry { get; set; }
        public int AfterExpiry { get; set; }
    }
}