using System.IO;

namespace Logger
{
    public class LogService
    {
        public LogService()
        {
#if DEBUG
            this.LogsLocation = "../../../logs/";
#else
            this.LogsLocation = "./logs/";
#endif
            this.LogFileName = "runtime-";
            this.LogsExtension = ".log";
        }

        public string LogsLocation { get; set; }

        public string LogFileName { get; set; }

        public string LogsExtension { get; set; }

        public string LogFullLocationName { get; set; }

        public StreamWriter GetStream()
        {
            int logVersion = 1;

            Directory.CreateDirectory(this.LogsLocation);

            while (File.Exists(this.LogsLocation + this.LogFileName + logVersion + this.LogsExtension))
            {
                logVersion++;
            }

            this.LogFullLocationName = this.LogsLocation + this.LogFileName + logVersion + this.LogsExtension;

            return new StreamWriter(LogFullLocationName);
        }
    }
}