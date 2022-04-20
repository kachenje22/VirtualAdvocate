using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualAdvocate.Models
{
    public class AllAssociateTemplateModel
    {

        public Int32 ID { get; set; }

        public string GroupName { get; set; }
        public string DocumentName { get; set; }
      public DateTime CreatedDate { get; set; }
    }

}