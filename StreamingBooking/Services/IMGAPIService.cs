namespace StreamingBooking.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using StreamingBooking.Models;
    using StreamingBooking.Models.IMG;

    public static class IMGAPIService
    {
        private const string ALG = "HmacMD5";

        private const string ALog = "ALog";
        private const string Fun88 = "Fun88";
        private const string Mayfair = "Mayfair";

        public static IList<LicenseeModel> Licensees()
        {
            var licensees = new List<LicenseeModel>();

            licensees.Add(new LicenseeModel
            {
                APISecret = "epPgs22TWH",
                PartnerId = 58,
                PartnerName = ALog
            });

            licensees.Add(new LicenseeModel
            {
                APISecret = "0QBYN1NOBU",
                PartnerId = 59,
                PartnerName = Fun88
            });

            licensees.Add(new LicenseeModel
            {
                APISecret = "Scuzb1F0VB",
                PartnerId = 54,
                PartnerName = Mayfair
            });

            return licensees;
        }

        public static IList<StreamModel> GetStreamEventData(LicenseeModel licensee, string apiUrl)
        {
            TimeSpan timeSpan = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));
            double timestamp = Math.Round(timeSpan.TotalMilliseconds);

            string apiToken = GenerateAPIToken(licensee.APISecret, timestamp);
            apiUrl = string.Format("{0}?operatorId={1}&auth={2}&timestamp={3}", apiUrl, licensee.PartnerId, apiToken, timestamp);

            using (HttpClient client = new HttpClient())
            {
                var result = client.GetAsync(apiUrl).Result.Content.ReadAsStringAsync().Result;

                List<EventModel> events = JsonConvert.DeserializeObject<List<EventModel>>(result);

                IList<StreamModel> streams = events.Select(x =>
                {
                    var match = GetMatchInfo(x.Title);

                    var stream = new StreamModel
                    {
                        ALog = licensee.PartnerName == ALog,
                        Fun88 = licensee.PartnerName == Fun88,
                        Mayfair = licensee.PartnerName == Mayfair,
                        AwayName = match.AwayTeam,
                        EventId = x.Id,
                        HomeName = match.HomeTeam,
                        KickOffDate = x.StartDate,
                        LeagueName = x.Tournament.Name,
                        SportName = x.Tournament.Property.Sport.Name,
                        StreamId = x.Id
                    };

                    return stream;
                }).ToList();

                return streams;
            }
        }

        private static MatchModel GetMatchInfo(string eventTitle)
        {
            var match = new MatchModel();
            string separateString = " v ";

            if (eventTitle.Contains(separateString))
            {
                var teams = eventTitle.Split(new[] { separateString }, StringSplitOptions.RemoveEmptyEntries);

                if (teams.Length >= 2)
                {
                    match.HomeTeam = teams[0];
                    match.AwayTeam = teams[1];
                }
            }

            return match;
        }

        private static string GenerateAPIToken(string secret, double timestamp)
        {
            string keyString = string.Format("{0}:{1}", secret, timestamp);

            HMAC hmac = HMACMD5.Create(ALG);
            hmac.Key = Encoding.UTF8.GetBytes(secret);
            hmac.ComputeHash(Encoding.UTF8.GetBytes(keyString));
            string token = BitConverter.ToString(hmac.Hash).Replace("-", "");

            return token.ToLower();
        }
    }
}