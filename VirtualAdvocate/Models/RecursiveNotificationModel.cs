using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace VirtualAdvocate.Models
{
    public class RecursiveNotificationModel
    {
        public int Id { get; set; }

        public int OrgId { get; set; }

        [Display(Name = "Recurrs Before Days")]
        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public Nullable<int> RecurrsBeforeDays { get; set; }

        [Display(Name = "Recurrs After Days")]
        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public Nullable<int> RecurrsAfterDays { get; set; }

        public bool Status { get; set; }
    }
}