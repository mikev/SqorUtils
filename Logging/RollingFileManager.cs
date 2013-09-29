using System;
using System.IO;
using System.Linq;

namespace Sqor.Utils.Logging
{
    public class RollingFileManager : IFileManager
    {
        private string baseFileName;
        private DateTime nextRotation;
        private TimeSpan period;
        private string fileNameDateFormat;
        private TextWriter output;

        public RollingFileManager(string fileName, TimeSpan period, TimeSpan? timeOfDay = null, string fileNameDateFormat = null) 
        {
            baseFileName = fileName;
            this.period = period;
            this.fileNameDateFormat = fileNameDateFormat;
            
            nextRotation = DateTime.Now + period;
            if (timeOfDay != null)
            {
                // Adjust the date so that it happens at the correct tiem of day.
                nextRotation = nextRotation.Date.Add(timeOfDay.Value);
            }
            OpenFile(DateTime.Now);
        }

        private void OpenFile(DateTime dateTime)
        {
            if (output != null)
                output.Dispose();

            string fileName = baseFileName + dateTime.ToString(fileNameDateFormat) + ".log";
            output = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read));                            

            // Delete all old files (old == > 7 days)
            foreach (var file in new FileInfo(fileName).Directory.GetFiles("*.log").Where(x => x.LastWriteTime < DateTime.Now.AddDays(-7)))
            {
                try
                {
                    file.Delete();
                    this.LogInfo("Deleted old log file " + file.FullName);
                }
                catch (Exception e)
                {
                    this.LogError("Error deleting old log file " + file.FullName, e);
                }
            }
        }

        private void CheckForFileRotation()
        {
            if (DateTime.Now >= nextRotation)
            {
                OpenFile(nextRotation);
                nextRotation = nextRotation + period;
            }
        }

        public TextWriter Output
        {
            get
            {
                CheckForFileRotation();
                return output;
            }
        }
    }
}