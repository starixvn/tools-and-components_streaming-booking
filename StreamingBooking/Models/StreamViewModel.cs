using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamingBooking.Models
{
    public class StreamViewModel
    {
        public IList<StreamModel> Streams { get; set; }

        public IDictionary<string, IList<StreamModel>> LicenseeStream { get; set; }

        public IList<string> From { get; set; }

        public IList<string> To { get; set; }

        public IList<string> Leagues { get; set; }
    }
}