using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualAdvocate.Models
{
    public class ServiceModel
    {

        public Int32 ID { get; set; }

        [Required(ErrorMessage = "Please Enter Client")]
        [Remote("CheckService", "DocumentManagement", ErrorMessage = "Client Already Exists. Please Enter Different Client")]
        public string Service { get; set; }
        public string ServiceDescription { get; set; }
        public Boolean IsEnabled { get; set; }
        public IEnumerable<ClientWiseCustomerTemplate> extraFields { get; set; }
    }

    public class EditServiceModel
    {

        public Int32 ID { get; set; }

        [Required(ErrorMessage = "Please Enter Client")]
        public string Service { get; set; }
        public string ServiceDescription { get; set; }
        public Boolean IsEnabled { get; set; }
        public IEnumerable<ClientWiseCustomerTemplate> extraFields { get; set; }
    }
}