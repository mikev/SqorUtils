using System;

namespace Sqor.Utils.DateTimes
{
    public struct DateTimeSpan 
    {
        private readonly int years;
        private readonly int months;
        private readonly int days;
        private readonly int hours;
        private readonly int minutes;
        private readonly int seconds;
        private readonly int milliseconds;

        public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            this.years = years;
            this.months = months;
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.milliseconds = milliseconds;
        }

        public int Years
        {
            get { return years; }
        }

        public int Months
        {
            get { return months; }
        }

        public int Days
        {
            get { return days; }
        }

        public int Hours
        {
            get { return hours; }
        }

        public int Minutes
        {
            get { return minutes; }
        }

        public int Seconds
        {
            get { return seconds; }
        }

        public int Milliseconds
        {
            get { return milliseconds; }
        }
    }
}

