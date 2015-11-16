using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using StreamingBooking.Models;
using StreamingBooking.Services;

namespace StreamingBooking.Controllers
{
    public class HomeController : Controller
    {
        private static List<StreamModel> _streams;
        private static List<string> _listOfDates;

        public ActionResult Index()
        {
            var days = Convert.ToInt32(ConfigurationManager.AppSettings["API.Days"]);
            var currentDay = DateTime.Now;
            var nextDay = currentDay.AddDays(days);

            _listOfDates = new List<string>();
            while (currentDay < nextDay)
            {
                _listOfDates.Add(currentDay.ToString("MM/dd/yyyy"));
                currentDay = currentDay.AddDays(1);
            }

            var model = new StreamViewModel
            {
                From = _listOfDates,
                To = _listOfDates,
                Leagues = new List<string>()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Search(string fromDate, string toDate, string league)
        {
            _streams = GetStreamData();

            var matchedStreamData = _streams
                                .Where(x => x.KickOffDate >= DateTime.ParseExact(fromDate, "MM/dd/yyyy", null)
                                    && x.KickOffDate <= DateTime.ParseExact(toDate, "MM/dd/yyyy", null)
                                    && (!string.IsNullOrWhiteSpace(league) ? x.LeagueName == league : x.LeagueName == x.LeagueName))
                                .ToList();

            var model = new StreamViewModel()
            {
                Streams = matchedStreamData
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult GetLeagues()
        {
            if (_streams == null)
            {
                _streams = GetStreamData();
            }

            string fromDate = _listOfDates.First();
            string toDate = _listOfDates.Last();

            var matchedStreamData = _streams
                                        .Where(x => x.KickOffDate >= DateTime.ParseExact(fromDate, "MM/dd/yyyy", null)
                                                && x.KickOffDate <= DateTime.ParseExact(toDate, "MM/dd/yyyy", null))
                                        .ToList();

            var leagues = matchedStreamData.GroupBy(x => x.LeagueName).Select(s => new { name = s.Key, value = s.Key });

            return Json(leagues, JsonRequestBehavior.AllowGet);
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

        private List<StreamModel> GetStreamData()
        {
            var streams = new List<StreamModel>();

            GetStreamEventDataFromUNASApi(streams);

            GetStreamEventDataFromIMGApi(streams);

            streams = streams.Distinct(new StreamingComparer()).ToList();

            return streams;
        }

        private void GetStreamEventDataFromIMGApi(List<StreamModel> streams)
        {
            var licensees = IMGAPIService.Licensees();
            string imgApiUrl = ConfigurationManager.AppSettings["IMG.API.Path"];

            foreach (var licensee in licensees)
            {
                IList<StreamModel> events = IMGAPIService.GetStreamEventData(licensee, imgApiUrl);

                streams.AddRange(events);
            }
        }

        private static void GetStreamEventDataFromUNASApi(List<StreamModel> streams)
        {
            var licensees = APIService.Licensees();

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
