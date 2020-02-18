using System;
using System.Configuration;

namespace StreamingBooking.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ToClientTime(this DateTime date, bool isUTC = false)
        {
            var clientTimeZone = Convert.ToInt32(ConfigurationManager.AppSettings["ClientTimeZone"]);
            var result = isUTC ? date : date.ToUniversalTime();
            result = result.AddHours(clientTimeZone);
            return result;
        }
    }
}