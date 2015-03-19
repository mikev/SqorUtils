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
        
        public static string ToHowLongAgo(this DateTime datetime, string w, string d, string h, string m, string s)
        {
            var now = DateTime.UtcNow;

            var timeSpan = now.Difference(datetime);

            string result = "";

            int years = timeSpan.Years;
            int months = timeSpan.Months;

            if (years > 0)
            {
                string pattern = CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern;
                pattern = pattern.Replace("MMMM", "MMM").Replace ("yyyy", "yy");

                string formatted = datetime.ToString(pattern);

                return formatted;
            }


            if (months > 0)
            {
                string pattern = CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern;
                pattern = pattern.Replace("MMMM", "MMM").Replace ("yyyy", "yy");
                string formatted = datetime.ToString(pattern);

                return formatted;
            }


            /////////
            int weeks = 0;
            int days = timeSpan.Days;

            if(days >= 7)
            {
                weeks = days/7;
                days = days %7;
            }

            if (weeks > 0)
            {
                result += weeks + w;
                return result;
            }
                

            if (days > 0)
            {
                result += days + d;
                return result;
            }

            /////////
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;

            if(hours > 0)
                return hours + h;
                
            if(minutes > 0)
                return minutes + m;

            /////////
            int seconds = timeSpan.Seconds;

            if(seconds > 0)
                return seconds + s;

            return result;
        } 

        public static string ToHowLongAgoUnless(this DateTime datetime, Func<TimeSpan, bool> unless, string w, string d, string h, string m, string s)
        {
            var now = DateTime.Now;

            if (unless(now - datetime))
                return datetime.ToTimeOrDate();
            else
                return datetime.ToHowLongAgo(w, d, h, m, s);
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
            datetime = TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(datetime, DateTimeKind.Utc), timezone);
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

        public static string ToDateTime(this DateTime datetime)
        {
            var culture = CultureInfo.CurrentUICulture;
            var timezone = TimeZoneInfo.Local;
            return datetime.ToDateTime(culture, timezone);
        }

        public static string ToDateTime(this DateTime datetime, CultureInfo culture, TimeZoneInfo timezone)
        {
            datetime = TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(datetime, DateTimeKind.Utc), timezone);
            return datetime.ToString(culture.DateTimeFormat.ShortDatePattern + " " + culture.DateTimeFormat.ShortTimePattern);
        }

        public static string ToTimeOrDate(this DateTime datetime, CultureInfo culture, TimeZoneInfo timezone)
        {
            var now = DateTime.UtcNow;
            now = TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(now, DateTimeKind.Utc), timezone);
            datetime = TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(datetime, DateTimeKind.Utc), timezone);
            if (now.Date == datetime.Date)
                return datetime.ToString(culture.DateTimeFormat.ShortTimePattern);
            else if (now.Date.Year == datetime.Date.Year)
                return datetime.ToString("MMM dd");
            else
                return datetime.ToString(culture.DateTimeFormat.ShortDatePattern);
        }         

        public static DateTime Truncate(this DateTime dateTime)
        {
            return dateTime.Truncate(TimeSpan.FromSeconds(1));
        }

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
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

