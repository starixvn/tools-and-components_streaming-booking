using Newtonsoft.Json;
using StreamingBooking.Common;
using StreamingBooking.Helpers;
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

        protected override async Task<IEnumerable<SportsRadarEventModel>> GetEvents(LicenseeModel licensee)
        {
            var handler = new HttpClientHandler();
            var proxy = ProxyHelper.CreateWebProxy();
            if (proxy != null)
            {
                handler = new HttpClientHandler()
                {
                    Proxy = proxy,
                    UseProxy = true
                };
            }

            using (var httpClient = new HttpClient(handler))
            {
                httpClient.BaseAddress = _apiBaseAddress;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", licensee.APISecret);

                var query = BuildQuery();
                var response = await httpClient.GetAsync($"events{query}").ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<IEnumerable<SportsRadarEventModel>>(json);
                return result;
            }
        }

        private string BuildQuery()
        {
            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(LimitDays - 1).AddHours(23).AddMinutes(59);
            var x = DateTime.Parse(to.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            var query = $"?from={from.ToString("yyyy-MM-ddTHH:mm:ssZ")}&to={to.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
            return query;
        }
    }
}