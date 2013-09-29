using System;
using System.Globalization;

namespace Sqor.Utils.DateTimes
{
    public static class DateTimeExtensions
    {
        public static bool IsBetween(this DateTime value, DateTime earlier, DateTime later)
        {
            return value >= earlier && value <= later;
        }

        public static bool Within(this DateTime dateTime, DateTime compareTo, int seconds)
        {
            return Within(dateTime, compareTo, new TimeSpan(0, 0, seconds));
        }

        public static bool Within(this DateTime dateTime, DateTime compareTo, TimeSpan tolerance)
        {
            int totalSeconds = Math.Abs((int)(dateTime - compareTo).TotalSeconds);
            int toleranceSeconds = (int)tolerance.TotalSeconds;
            return totalSeconds <= toleranceSeconds;
        }

        public static decimal ToAge(this DateTime dateTime)
        {
            int now = int.Parse(DateTime.Today.ToString("yyyyMMdd"));
            int dob = int.Parse(dateTime.ToString("yyyyMMdd"));
            int diff = int.Parse((now - dob).ToString());
            decimal age = (decimal)diff / 10000;
            return age;
        }
        
        public static string ToHowLongAgo(this DateTime datetime)
        {
            var now = DateTime.UtcNow;

            var timeSpan = now.Difference(datetime);
            var days = timeSpan.Days;
            if (timeSpan.Years > 0 || timeSpan.Months > 0)
            {
                int years = timeSpan.Years;
                int months = timeSpan.Months;
                    
                var yearsText = years > 1 ? "years" : "year";
                var monthsText = months > 1 ? "months" : "month";
                
                var monthsResult = months + " " + monthsText;

                if (years > 0 && months == 0)
                    return years + " " + yearsText + " ago";
                else if (years > 0)
                    return years + " " + yearsText + ", " + monthsResult + " ago";
                else
                    return monthsResult + " ago";
            }
            else if (days >= 2)
                return days + " days ago";
            else if (days >= 1)
                return days + " day ago";
            else
            {
                var hours = timeSpan.Hours;
                if (hours >= 2)
                    return (int)hours + " hours ago";
                else if (hours >= 1)
                    return (int)hours + " hour ago";
                else
                {
                    var minutes = timeSpan.Minutes;
                    if (minutes >= 2)
                        return (int)minutes + " minutes ago";
                    else if (minutes >= 1)
                        return (int)minutes + " minute ago";
                    else
                    {
                        var seconds = timeSpan.Seconds;
                        if (seconds >= 2)
                            return (int)seconds + " seconds ago";
                        else if (minutes >= 1)
                            return (int)seconds + " second ago";
                        else
                            return "Just now";
                    }
                }
            }
        } 

        public static string ToHowLongAgoUnless(this DateTime datetime, Func<TimeSpan, bool> unless)
        {
            var now = DateTime.Now;

            if (unless(now - datetime))
                return datetime.ToTimeOrDate();
            else
                return datetime.ToHowLongAgo();
        }
                
        public static string ToTimeOrDate(this DateTime datetime)
        {
            var culture = CultureInfo.CurrentUICulture;
            var timezone = TimeZoneInfo.Local;
            return datetime.ToTimeOrDate(culture, timezone);
        }        
        
        public static string ToDate(this DateTime datetime)
        {
            var culture = CultureInfo.CurrentUICulture;
            var timezone = TimeZoneInfo.Local;
            return datetime.ToDate(culture, timezone);
        }
        
        public static string ToDate(this DateTime datetime, CultureInfo culture, TimeZoneInfo timezone)
        {
            datetime = TimeZoneInfo.ConvertTime(datetime, TimeZoneInfo.Utc, timezone);
            return datetime.ToString(culture.DateTimeFormat.ShortDatePattern);
        }
        
        public static DateTime FromDate(this string date)
        {
            var culture = CultureInfo.CurrentUICulture;
            var timezone = TimeZoneInfo.Local;
            return date.FromDate(culture, timezone);
        }
        
        public static DateTime FromDate(this string date, CultureInfo culture, TimeZoneInfo timezone)
        {
            return DateTime.ParseExact(date, culture.DateTimeFormat.ShortDatePattern, null);
        }

        public static string ToTimeOrDate(this DateTime datetime, CultureInfo culture, TimeZoneInfo timezone)
        {
            var now = DateTime.UtcNow;
            now = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Utc, timezone);
            datetime = TimeZoneInfo.ConvertTime(datetime, TimeZoneInfo.Utc, timezone);
            if (now.Date == datetime.Date)
                return datetime.ToString(culture.DateTimeFormat.ShortTimePattern);
            else if (now.Date.Year == datetime.Date.Year)
                return datetime.ToString("MMM dd");
            else
                return datetime.ToString(culture.DateTimeFormat.ShortDatePattern);
        }         

        enum Phase { Years, Months, Days, Done }

        public static DateTimeSpan Difference(this DateTime date1, DateTime date2) 
        {
            if (date2 < date1)
            {
                var sub = date1;
                date1 = date2;
                date2 = sub;
            }

            DateTime current = date2;
            int years = 0;
            int months = 0;
            int days = 0;

            Phase phase = Phase.Years;
            DateTimeSpan span = new DateTimeSpan();

            while (phase != Phase.Done) 
            {
                switch (phase) 
                {
                    case Phase.Years:
                        if (current.Year == 1 || current.AddYears(-1) < date1)
                        {
                            phase = Phase.Months;
                        }
                        else 
                        {
                            current = current.AddYears(-1);
                            years++;
                        }
                        break;
                    case Phase.Months:
                        if (current.AddMonths(-1) < date1) 
                        {
                            phase = Phase.Days;
                        }
                        else 
                        {
                            current = current.AddMonths(-1);
                            months++;
                        }
                        break;
                    case Phase.Days:
                        if (current.AddDays(-1) < date1) 
                        {
                            var timespan = current - date1;
                            span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                            phase = Phase.Done;
                        }
                        else 
                        {
                            current = current.AddDays(-1);
                            days++;
                        }
                        break;
                }
            }
            return span;
        }   
    }    
}

