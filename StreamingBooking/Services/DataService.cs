using Fanex.Caching;
using StreamingBooking.Common;
using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace StreamingBooking.Services
{
    public class DataService : IDataService
    {
        private readonly ICacheService cacheService = new CacheService();
        private readonly string key = ConfigurationManager.AppSettings["ActivityCacheKey"];

        public List<StreamModel> Get(StreamMetalModel filter)
        {
            return GetStreamData(filter.Provider, filter.Licensee)
                .Distinct(new StreamingComparer())
                .ToList();
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

                //case (int)ProviderType.SportRadar:
                    //GetStreamEventDataFromSportRadarApi(filterLicensee, streams);
                    break;

                default:
                    GetStreamEventDataFromUNASApi(filterLicensee, streams);
                    GetStreamEventDataFromIMGApi(filterLicensee, streams);
                    //GetStreamEventDataFromSportRadarApi(filterLicensee, streams);
                    break;
            }

            return streams;
        }

        private void GetStreamEventDataFromSportRadarApi(string filterLicensee, List<StreamModel> streams)
        {
            var cacheKey = $"{key}_SportRadarApi_{filterLicensee}";
            cacheService.TryGet(cacheKey, out List<StreamModel> dataFromCache);
            if (dataFromCache == null)
            {
                dataFromCache = new List<StreamModel>();
            }
            if (!dataFromCache.Any())
            {
                var sportRadarService = SportRadarApiService.Instance;
                var licensees = sportRadarService.Licensees;
                if (!string.IsNullOrWhiteSpace(filterLicensee))
                {
                    licensees = licensees.Where(x => x.PartnerName.Equals(filterLicensee, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                dataFromCache.AddRange(sportRadarService.GetStreamEventData(licensees.ToArray()));

                cacheService.Set(cacheKey, dataFromCache, new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }

            streams.AddRange(dataFromCache);
        }

        private void GetStreamEventDataFromIMGApi(string filterLicensee, List<StreamModel> streams)
        {
            var cacheKey = $"{key}_IMGApi_{filterLicensee}";
            cacheService.TryGet(cacheKey, out List<StreamModel> dataFromCache);
            if (dataFromCache == null)
            {
                dataFromCache = new List<StreamModel>();
            }
            if (!dataFromCache.Any())
            {
                var imgApiUrl = ConfigurationManager.AppSettings["IMG.API.Path"];

                var licensees = ImgApiService.Licensees();
                if (!string.IsNullOrWhiteSpace(filterLicensee))
                {
                    licensees = licensees.Where(x => x.PartnerName.Equals(filterLicensee, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                foreach (var licensee in licensees)
                {
                    dataFromCache.AddRange(ImgApiService.GetStreamEventData(licensee, imgApiUrl));
                }

                cacheService.Set(cacheKey, dataFromCache, new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }

            streams.AddRange(dataFromCache);
        }

        private void GetStreamEventDataFromUNASApi(string filterLicensee, List<StreamModel> streams)
        {
            var cacheKey = $"{key}_UNASApi_{filterLicensee}";
            cacheService.TryGet(cacheKey, out List<StreamModel> dataFromCache);
            if (dataFromCache == null)
            {
                dataFromCache = new List<StreamModel>();
            }
            if (!dataFromCache.Any())
            {
                var licensees = UnasApiService.Licensees();

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

                        var doc = Convert.ToBoolean(ConfigurationManager.AppSettings["IsLocal"]) ? UnasApiService.ReadXmlFile(licensee.PathTest) : UnasApiService.GetXmlResponse(apiSetting);
                        var licenseeStreams = UnasApiService.ProcessEntityElements(licensee.PartnerName, doc);

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

        public bool ClearCache(string licensee)
        {
            try
            {
                foreach (var cacheKey in GetCacheKeys(licensee))
                {
                    cacheService.Remove(cacheKey);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IEnumerable<string> GetCacheKeys(string licensee)
        {
            yield return $"{key}_SportRadarApi_{licensee}";
            yield return $"{key}_UNASApi_{licensee}";
            yield return $"{key}_IMGApi_{licensee}";
        }
    }
}