using System.Collections.Generic;

namespace StreamingBooking.Models
{
    public class StreamViewModel
    {
        public IList<StreamModel> Streams { get; set; }        

        public IList<string> From { get; set; }

        public IList<string> To { get; set; }

        public IList<string> Leagues { get; set; }

        public IList<string> Licensees { get; set; }
    }
}