using Newtonsoft.Json;
using System.Collections.Generic;

namespace StreamingBooking.Models.SportRadar
{
    public class StreamContentModel
    {
        [JsonProperty("is_main")]
        public bool IsMain { get; set; }

        [JsonProperty("streams")]
        public IList<ObjectIdentityModel> Streams { get; set; }
    }
}