using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace VirtualAdvocate.Models
{
    public class NewInquiry
    {
        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Organization Name is required!")]
        public string Organization { get; set; }
        [Required(ErrorMessage = "Email is required!")]
        [RegularExpression(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$",
        ErrorMessage = "Please Enter Correct Email Address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone Number is required!")]
        [RegularExpression("([0-9]+)", ErrorMessage = "Please Enter valid Phone Number")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "This  field is required!")]
        public string Issue { get; set; }
        public string ID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}