using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using StreamingBooking.Common;
using StreamingBooking.Models;
using StreamingBooking.Services;

namespace StreamingBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var days = Convert.ToInt32(ConfigurationManager.AppSettings["API.Days"]);
            var currentDay = DateTime.Now;
            var nextDay = currentDay.AddDays(days);

            SessionManager.FilterDates = new List<string>();
            while (currentDay < nextDay)
            {
                SessionManager.FilterDates.Add(currentDay.ToString("MM/dd/yyyy"));
                currentDay = currentDay.AddDays(1);
            }

            var licensees = GetLicensees();
            var providers = GetProviders();

            var model = new StreamViewModel
            {
                From = SessionManager.FilterDates,
                To = SessionManager.FilterDates,
                Leagues = new List<string>(),
                Licensees = licensees,
                Providers = providers
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Search(
            string fromDate,
            string toDate,
            string league,
            string licensee,
            int provider)
        {
            List<StreamModel> allStreams = GetStreamData(provider, licensee);
            SessionManager.Streams = allStreams.Distinct(new StreamingComparer()).ToList();

            var matchedStreamData = SessionManager.Streams
                                        .Where(x => x.KickOffDate >= DateTime.ParseExact(fromDate, "MM/dd/yyyy", null)
                                            && x.KickOffDate <= DateTime.ParseExact(toDate, "MM/dd/yyyy", null)
                                            && (!string.IsNullOrWhiteSpace(league) ? x.LeagueName == league : x.LeagueName == x.LeagueName))
                                        .ToList();

            var model = new StreamViewModel()
            {
                DistinctStreams = matchedStreamData,
                AllStreams = allStreams
            };

            return View(model);
        }

        [HttpGet]
        public JsonResult GetLeagues(int provider, string licensee)
        {
            if (SessionManager.Streams == null)
            {
                SessionManager.Streams = GetStreamData(provider, licensee);
            }

            //string fromDate = SessionManager.FilterDates.First();
            //string toDate = SessionManager.FilterDates.Last();

            //var matchedStreamData = SessionManager.Streams
            //                            .Where(x => x.KickOffDate >= DateTime.ParseExact(fromDate, "MM/dd/yyyy", null)
            //                                    && x.KickOffDate <= DateTime.ParseExact(toDate, "MM/dd/yyyy", null))
            //                            .ToList();

            var leagues = SessionManager.Streams.GroupBy(x => x.LeagueName).Select(s => s.Key).ToList();

            return Json(leagues, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetLicenseesByProvider(int provider)
        {
            List<string> licensees = GetLicensees(provider);

            return Json(licensees, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void UpdateActiveRow(int streamId)
        {
            var filePath = ConfigurationManager.AppSettings["StreamingPath"];

            Helper.WriteFile(filePath, streamId);
        }

        [HttpPost]
        public void UpdateRemark(int streamId, string remark)
        {
            var filePath = ConfigurationManager.AppSettings["StreamingRemarkPath"];

            Helper.WriteRemarkFile(filePath, streamId, remark);
        }

        private List<SelectListItem> GetProviders()
        {
            var providers = new List<SelectListItem>();

            foreach (ProviderType providerType in Enum.GetValues(typeof(ProviderType)))
            {
                providers.Add(new SelectListItem
                {
                    Text = providerType.ToString(),
                    Value = string.Format("{0}", (int)providerType)
                });
            }

            return providers;
        }

        private List<string> GetLicensees(int provider = 0)
        {
            var allLicensees = new List<string>();
            var unasLicensees = new List<string> {                  
                    APIService.Bodog,
                    APIService.Dafabet,
                    APIService.Fun88,
                    APIService.IBCBet,
                    APIService.Macaubet,
                    APIService.Tlc88                    
            };
            var imgLicensees = new List<string> {                  
                    IMGAPIService.Fun88,
                    IMGAPIService.ALog,
                    IMGAPIService.Mayfair
            };

            switch (provider)
            {
                case (int)ProviderType.IMG:
                    allLicensees = imgLicensees;
                    break;
                case (int)ProviderType.Unas:
                    allLicensees = unasLicensees;
                    break;
                default:
                    allLicensees = unasLicensees.Union(imgLicensees).ToList();
                    break;
            }

            allLicensees = allLicensees.Distinct().OrderBy(x => x).ToList();

            return allLicensees;
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
            string imgApiUrl = ConfigurationManager.AppSettings["IMG.API.Path"];
            var licensees = IMGAPIService.Licensees();

            if (!string.IsNullOrWhiteSpace(filterLicensee))
            {
                licensees = licensees.Where(x => x.PartnerName.Equals(filterLicensee, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var licensee in licensees)
            {
                IList<StreamModel> events = IMGAPIService.GetStreamEventData(licensee, imgApiUrl);

                streams.AddRange(events);
            }
        }

        private static void GetStreamEventDataFromUNASApi(string filterLicensee, List<StreamModel> streams)
        {
            var licensees = APIService.Licensees();

            if (!string.IsNullOrWhiteSpace(filterLicensee))
            {
                licensees = licensees.Where(x => x.PartnerName.Equals(filterLicensee, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var licensee in licensees)
            {
                var apiSetting = string.Format("{0}?{1}&{2}&{3}&{4}",
                ConfigurationManager.AppSettings["API.Path"],
                "pid=" + licensee.PartnerId,
                "auth=" + licensee.APISecret,
                "filter=streams",
                "days=" + ConfigurationManager.AppSettings["API.Days"]);

                XDocument doc = Convert.ToBoolean(ConfigurationManager.AppSettings["IsLocal"]) ? APIService.ReadXmlFile(licensee.PathTest) : APIService.GetXmlResponse(apiSetting);
                var licenseeStreams = APIService.ProcessEntityElements(licensee.PartnerName, doc);

                streams.AddRange(licenseeStreams);
            }
        }
    }
}
