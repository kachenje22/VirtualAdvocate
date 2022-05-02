#region NameSpaces
using Elmah;
using System;
#endregion
#region VirtualAdvocate.Common
namespace VirtualAdvocate.Common
{
    #region ErrorLog
    public sealed class ErrorLog
    {
        //private static ErrorLog _errorLog;

        #region ErrorLog
        private ErrorLog()
        {

        }
        #endregion

        #region LogThisError
        public static void LogThisError(Exception exception)
        {
            ErrorSignal.FromCurrentContext().Raise(exception);
        } 
        #endregion
    } 
    #endregion
} 
#endregion