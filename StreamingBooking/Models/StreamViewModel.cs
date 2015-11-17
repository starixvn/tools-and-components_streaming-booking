using System.Collections.Generic;
using System.Web.Mvc;

namespace StreamingBooking.Models
{
    public class StreamViewModel
    {
        public IList<StreamModel> DistinctStreams { get; set; }

        public IList<StreamModel> AllStreams { get; set; }

        public IList<string> From { get; set; }

        public IList<string> To { get; set; }

        public IList<string> Leagues { get; set; }

        public IList<string> Licensees { get; set; }

        public IList<SelectListItem> Providers { get; set; }
    }
}