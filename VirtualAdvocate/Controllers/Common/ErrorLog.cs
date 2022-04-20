using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Elmah;

namespace VirtualAdvocate.Common
{
    public sealed class ErrorLog
    {
        //private static ErrorLog _errorLog;

        private ErrorLog()
        {

        }

        public static void LogThisError(Exception exception)
        {

            ErrorSignal.FromCurrentContext().Raise(exception);
        }
    }
}