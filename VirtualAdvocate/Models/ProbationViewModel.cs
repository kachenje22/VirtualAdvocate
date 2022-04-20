using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class ProbationViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ExtendExpiry { get; set; }

        //[Required(ErrorMessage = "Date of joining field is required.")]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public string DateOfJoining { get; set; }
        

        [Required(ErrorMessage = "Probation Month field is required.")]
        public int ProbationPeriod { get; set; }

        public DateTime ProbationPeriodExpiredOn { get; set; }

        public System.DateTime DateOfExpiry { get; set; }

        public int NoOfDaysExpired { get; set; }

        public InsuranceStatus Status { get; set; }

        public int OrgId { get; set; }

        [Required(ErrorMessage = "Customer Name is required.")]
        public Nullable<int> CustomerId { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}