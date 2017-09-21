using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFileMergeApplication
{
    class LogEntry
    {
        public DateTime dateTime;
        // logData[0] = logType -> INFO, DEBUG, ERROR, or FATAL
        // logData[1] = Device name
        // logData[2] = Log message
        private string[] logData;

        // Default constructor
        public LogEntry()
            : this(new DateTime(),"","","") { }

        public LogEntry(DateTime dateTime, string logType, string deviceName, string msg)
        {
            this.dateTime = dateTime;
            logData = new string[] { logType, deviceName, msg };
        }

        public string[] getLogData() { return logData; }

        // Converts log entries into proper format for new log file
        // TODO: check if the format for dateTime.ToString() is appropriate
        public override string ToString()
        {
            return this.dateTime.ToString("yyyy-MM-dd [HH:mm:ss:ffff]") + "," + String.Join(",",logData);
        }


    }
}
