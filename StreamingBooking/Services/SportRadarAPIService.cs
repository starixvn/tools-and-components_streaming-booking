using Newtonsoft.Json;
using StreamingBooking.Common;
using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StreamingBooking.Services
{
    public class SportRadarApiService : ProviderApiService<SportsRadarEventModel>
    {
        private static readonly Lazy<IEventProviderService> _instance = new Lazy<IEventProviderService>(() => new SportRadarApiService());

        public static IEventProviderService Instance { get => _instance.Value; }

        private const string ApiPathConfigKey = "SportsRadar.API.Path";

        private readonly Uri _apiBaseAddress;

        public override IEnumerable<LicenseeModel> Licensees
        {
            get
            {
                yield return new LicenseeModel
                {
                    APISecret = "eF3yu5j8HxyIPdIWEy",
                    PartnerName = LicenseeName.ALog
                };
            }
        }

        private SportRadarApiService()
        {
            _apiBaseAddress = new Uri(ConfigurationManager.AppSettings[ApiPathConfigKey]);
        }

        protected override async Task<IEnumerable<SportsRadarEventModel>> GetEvents(LicenseeModel licensee, DateTime? from, DateTime? to)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = _apiBaseAddress;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", licensee.APISecret);

                var query = string.Empty;
                if (from.HasValue && to.HasValue)
                {
                    to = to.Value.Date.AddHours(23).AddMinutes(59);
                    query = $"?from={from.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")}&to={to.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
                }

                var response = await httpClient.GetAsync($"events{query}").ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<IEnumerable<SportsRadarEventModel>>(json);
                return result;
            }
        }
    }
}