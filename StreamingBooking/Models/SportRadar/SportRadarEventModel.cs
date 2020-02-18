using Newtonsoft.Json;
using StreamingBooking.Helpers;
using StreamingBooking.Models.SportRadar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamingBooking.Models
{
    public class SportsRadarEventModel : BaseEventModel
    {
        [JsonProperty("id")]
        public string EventId { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("first_level_category")]
        public ObjectIdentityModel Sport { get; set; }

        [JsonProperty("third_level_category")]
        public ObjectIdentityModel Tournament { get; set; }

        [JsonProperty("competitors")]
        public IList<CompetitorModel> Competitors { get; set; }

        [JsonProperty("contents")]
        public IList<StreamContentModel> Contents { get; set; }

        public override int Id
        {
            get
            {
                var streamId = Contents?.FirstOrDefault(_ => _.IsMain)?
                    .Streams?
                    .FirstOrDefault()?
                    .Id;
                var id = string.IsNullOrEmpty(streamId) ? 0 : int.Parse(streamId.Split(':')[2]);
                return id;
            }
        }

        public override DateTime KickOffDate { get => StartTime.ToClientTime(); }

        public override string SportName { get => Sport.Name; }

        public override string LeagueName { get => Tournament.Name; }

        public override string HomeName { get => Competitors?.ElementAtOrDefault(0).Name; }

        public override string AwayName { get => Competitors?.ElementAtOrDefault(1).Name; }
    }
}