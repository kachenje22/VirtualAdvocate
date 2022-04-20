using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationScheduler.Models
{
    class Logger
    {
        private static string LogFile = String.Empty;
        static Logger()
        {
            LogFile = System.Configuration.ConfigurationSettings.AppSettings["RootPath"] + System.Configuration.ConfigurationSettings.AppSettings["LogFile"];
        }

        public static void Log(string msg)
        {
            try
            {
                System.IO.File.AppendAllText(LogFile, DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.fff") + " : " + msg + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
}
