namespace StreamingBooking.Common
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.SessionState;
    using StreamingBooking.Models;

    public static class SessionManager
    {
        private const string StreamDataKey = "StreamData";

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
    }
}