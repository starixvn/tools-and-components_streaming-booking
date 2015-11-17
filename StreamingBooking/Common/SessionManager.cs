using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using StreamingBooking.Models;
namespace StreamingBooking.Common
{
    public static class SessionManager
    {
        private const string StreamDataKey = "StreamData";
        private const string FilterDateKey = "FilterDate";

        public static HttpSessionState CurrentSession
        {
            get
            {
                return HttpContext.Current.Session;
            }
        }

        public static List<StreamModel> Streams
        {
            get
            {
                var streams = CurrentSession[StreamDataKey] as List<StreamModel>;

                return streams;
            }
            set
            {
                CurrentSession[StreamDataKey] = value;
            }
        }

        public static List<string> FilterDates
        {
            get
            {
                var filterDates = CurrentSession[FilterDateKey] as List<string>;

                return filterDates;
            }
            set
            {
                CurrentSession[FilterDateKey] = value;
            }
        }
    }
}