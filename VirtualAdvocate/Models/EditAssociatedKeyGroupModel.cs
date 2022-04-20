using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace VirtualAdvocate.Models
{
    public class EditAssociatedKeygroupModel
    {
        public int ID { get; set; }
        public int TemplateID { get; set; }
        public string DocumentTitle { get; set; }
        public int KeyID { get; set; }
      
        public string GroupName { get; set; }
        public IEnumerable<TemplateKeywordModel> templateKeyword { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }

        public string DesignType { get; set; }

        public string AutoNumberStartsFrom { get; set; }

        public string FirstColumn { get; set; }

    }

   

}

