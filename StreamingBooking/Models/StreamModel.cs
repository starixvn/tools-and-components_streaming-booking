using System;

namespace StreamingBooking.Models
{
    public class StreamModel
    {
        public int StreamId { get; set; }

        public int EventId { get; set; }

        public string SportName { get; set; }

        public string LeagueName { get; set; }

        public string HomeName { get; set; }

        public string AwayName { get; set; }

        public DateTime KickOffDate { get; set; }

        public bool IBCBet { get; set; }                

        public bool Dafabet { get; set; }

        public bool Fun88 { get; set; }

        public bool M88 { get; set; }        

        public bool ALog { get; set; }

        public bool Mayfair { get; set; }
    }
}