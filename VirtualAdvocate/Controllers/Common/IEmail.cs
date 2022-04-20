using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualAdvocate.Common
{
    public interface IEmail
    {
        string[] ToAddress { get; set; }
        string Body { get; set; }
        string Subject { get; set; }
        string[] CCAddress { get; set; }

    }
}