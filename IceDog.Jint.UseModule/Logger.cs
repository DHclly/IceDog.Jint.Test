using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IceDog.Jint.Test
{
    public class Logger
    {
        public static Logger Default = new Logger();

        private static readonly string _dateFormat = "HH:mm:ss";

        private string getDateNowText() => DateTime.Now.ToString(_dateFormat);

        public void Log(string logType, string message)
        {
            var text = $"[{getDateNowText()}][{logType}] {message}";
            Console.WriteLine(text);
        }

        public void Debug(string message)
        {
            Log("debug", message);
        }

        public void Info(string message)
        {
            Log("info", message);
        }

        public void Warn(string message)
        {
            Log("warn", message);
        }

        public void Error(string message)
        {
            Log("error", message);
        }
    }
}
