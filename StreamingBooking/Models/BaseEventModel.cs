using Newtonsoft.Json;
using System;

namespace StreamingBooking.Models
{
    public abstract class BaseEventModel
    {
        [JsonIgnore]
        public abstract int Id { get; }

        [JsonIgnore]
        public abstract DateTime KickOffDate { get; }

        [JsonIgnore]
        public abstract string SportName { get; }

        [JsonIgnore]
        public abstract string LeagueName { get; }

        [JsonIgnore]
        public abstract string HomeName { get; }

        [JsonIgnore]
        public abstract string AwayName { get; }
    }
}