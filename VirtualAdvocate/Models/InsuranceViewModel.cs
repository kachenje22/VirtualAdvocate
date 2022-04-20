using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class InsuranceViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }

        [Required(ErrorMessage ="Insurer field is required.")]
        public string Insurer { get; set; }

        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; }

        [Required(ErrorMessage = "Please select Asset.")]
        public int Asset { get; set; }

        public string AssetInsured { get; set; }
        public int NoOfDaysExpired { get; set; }

        [Required(ErrorMessage = "Amount insured field is required.")]
        public string AmountInsured { get; set; }

        //[Required(ErrorMessage = "Date of insurance is required.")]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        //public DateTime DateOfInsurance { get; set; }

        //[Required(ErrorMessage = "Date of Expiry is required.")]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        //public DateTime DateOfExpiry { get; set; }

        [Required(ErrorMessage = "Date of insurance is required.")]
        public string DateOfInsurance { get; set; }

        [Required(ErrorMessage = "Date of Expiry is required.")]
        public string DateOfExpiry { get; set; }

        public int DepartmentId { get; set; }
        public int OrganizationId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int ExtendedMonths { get; set; }
        public InsuranceStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required(ErrorMessage = "Currency field is required.")]
        public string Currency { get; set; }
        //public System.DateTime CreatedDate { get; set; }
        //public System.DateTime ModifiedDate { get; set; }
        //public bool Status { get; set; }
    }

    public enum InsuranceStatus
    {
        Valid,
        Expired
    }

    public class Month
    {
        public int Label { get; set; }
    }

    public class Extend
    {
        public int Id { get; set; }
        public int Month { get; set; }  
    }

    public class AssetParam
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
    }
}