using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace VirtualAdvocate.Models
{
    public class KeyCategoryModel
    {
        public Int32 ID { get; set; }

        [Required(ErrorMessage = "Please Enter Category")]
        [Remote("CheckKeyCategory", "KeyCategoryList", ErrorMessage = "Category Name Already Exists. Please Enter Different Category Name")]
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public Boolean IsEnabled { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only numbers")]
        public int Order { get; set; }
        public bool CanAddInsurance { get; set; }
    }

    public class EditKeyCategoryModel
    {

        public Int32 ID { get; set; }

        [Required(ErrorMessage = "Please Enter Category")]
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public Boolean IsEnabled { get; set; }
       
        [RegularExpression("^[0-9]*$", ErrorMessage = "Only numbers")]
        public int Order { get; set; }
        public bool CanAddInsurance { get; set; }
    }
}

