using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Models
{
    public class BulkInsuranceJsonResponse
    {
        public BulkInsuranceJsonResponse()
        {
            this.Errors = new List<Error>();
        }
        public int TotalRecords { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class Error
    {
        public int RecordNumber { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Document { get; set; }
        public string AssetInsured { get; set; }
        public string DateOfJoining { get; set; }
        public string ProbationPeriod { get; set; }
    }
}