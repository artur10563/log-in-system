using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordSystemLaba2
{
    internal static class Logger
    {
        internal static string LogFile { get; set; } = "defaultLogs.txt";
        internal static void logWrite(string action, User curUser = null)
        {
            using (StreamWriter stream = File.AppendText(LogFile))
            {
                string time = DateTime.Now.ToLocalTime().ToString();
                string userName = curUser?.Login ?? "NOT_LOGGED";
                stream.WriteLine($"{time} USER: {userName}, ACTION: {action}");
            }
        }
    }
}
