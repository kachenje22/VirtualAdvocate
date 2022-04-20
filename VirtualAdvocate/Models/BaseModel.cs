using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace VirtualAdvocate.Models
{
    public class BaseModel
    {
        [NotMapped, XmlIgnore]
        public string EntityName { get; set; }
    }
}