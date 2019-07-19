using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Fanex.Caching;
using StreamingBooking.Common;
using StreamingBooking.Models;

namespace StreamingBooking.Services
{
    public class DataService : IDataService
    {
        private readonly ICacheService cacheService = new CacheService();
        private readonly string key = ConfigurationManager.AppSettings["ActivityCacheKey"];

        public List<StreamModel> Get(StreamMetalModel filter)
        {
            return GetStreamData(filter.Provider, filter.Licensee).Distinct(new StreamingComparer()).ToList(); ;
        }
        private List<StreamModel> GetStreamData(int provider, string filterLicensee)
        {
            var streams = new List<StreamModel>();

            switch (provider)
            {
                case (int)ProviderType.IMG:
                    GetStreamEventDataFromIMGApi(filterLicensee, streams);
                    break;
                case (int)ProviderType.Unas:
                    GetStreamEventDataFromUNASApi(filterLicensee, streams);
                    break;
                default:
                    GetStreamEventDataFromUNASApi(filterLicensee, streams);

                    GetStreamEventDataFromIMGApi(filterLicensee, streams);
                    break;
            }

            return streams;
        }

        private void GetStreamEventDataFromIMGApi(string filterLicensee, List<StreamModel> streams)
        {
            var cacheKey = $"{key}_IMGApi_{filterLicensee}";

            var dataFromCache = cacheService.Get<List<StreamModel>>(cacheKey) ?? new List<StreamModel>();
            if (!dataFromCache.Any())
            {
                var imgApiUrl = ConfigurationManager.AppSettings["IMG.API.Path"];

                var licensees = IMGAPIService.Licensees();
                if (!string.IsNullOrWhiteSpace(filterLicensee))
                {
                    licensees = licensees.Where(x => x.PartnerName.Equals(filterLicensee, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                foreach (var licensee in licensees)
                {
                    dataFromCache.AddRange(IMGAPIService.GetStreamEventData(licensee, imgApiUrl));
                }

                cacheService.Set(cacheKey, dataFromCache, new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }

            streams.AddRange(dataFromCache);
        }

        private void GetStreamEventDataFromUNASApi(string filterLicensee, List<StreamModel> streams)
        {
            var cacheKey = $"{key}_UNASApi_{filterLicensee}";

            var dataFromCache = cacheService.Get<List<StreamModel>>(cacheKey) ?? new List<StreamModel>();

            if (!dataFromCache.Any())
            {
                var licensees = APIService.Licensees();

                if (!string.IsNullOrWhiteSpace(filterLicensee))
                {
                    licensees = licensees.Where(x => x.PartnerName.Equals(filterLicensee, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                foreach (var licensee in licensees)
                {
                    try
                    {
                        var apiSetting = string.Format("{0}?{1}&{2}&{3}&{4}",
                               ConfigurationManager.AppSettings["API.Path"],
                               "pid=" + licensee.PartnerId,
                               "auth=" + licensee.APISecret,
                               "filter=streams",
                               "days=" + ConfigurationManager.AppSettings["API.Days"]);

                        var doc = Convert.ToBoolean(ConfigurationManager.AppSettings["IsLocal"]) ? APIService.ReadXmlFile(licensee.PathTest) : APIService.GetXmlResponse(apiSetting);
                        var licenseeStreams = APIService.ProcessEntityElements(licensee.PartnerName, doc);

                        dataFromCache.AddRange(licenseeStreams);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                cacheService.Set(cacheKey, dataFromCache, new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));

            }
            streams.AddRange(dataFromCache);
        }
    }
}
