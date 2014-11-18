using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using StreamingBooking.Services;
using StreamingBooking.Models;
using System.Configuration;
using System.Xml.Linq;

namespace StreamingBooking.Controllers
{
    public class HomeController : Controller
    {
        private static IDictionary<string, IList<StreamModel>> APIData { get; set; }

        public ActionResult Index()
        {
            var days = Convert.ToInt32(ConfigurationManager.AppSettings["API.Days"]);
            var currentDay = DateTime.Now;
            var nextDay = currentDay.AddDays(days);

            var listOfDates = new List<string>();
            while (currentDay < nextDay)
            {
                listOfDates.Add(currentDay.ToString("MM/dd/yyyy"));
                currentDay = currentDay.AddDays(1);
            }

            var model = new StreamViewModel
            {
                From = listOfDates,
                To = listOfDates,
                Leagues = new List<string>()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Search(string fromDate, string toDate, string league)
        {
            APIData = Data();

            var dataUnion = DataUnion();

            dataUnion = dataUnion
                .Where(x => x.KickOffDate >= DateTime.ParseExact(fromDate, "MM/dd/yyyy", null)
                    && x.KickOffDate <= DateTime.ParseExact(toDate, "MM/dd/yyyy", null)
                    && (!string.IsNullOrWhiteSpace(league) ? x.LeagueName == league : x.LeagueName == x.LeagueName))
                .ToList();

            var model = new StreamViewModel()
            {
                Streams = dataUnion.ToList(),
                LicenseeStream = APIData
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult GetLeagues()
        {
            var dataUnion = DataUnion();

            var leagues = dataUnion.GroupBy(x => x.LeagueName).Select(s => new { name = s.Key, value = s.Key });

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

        private IDictionary<string, IList<StreamModel>> Data()
        {
            Dictionary<string, IList<StreamModel>> streams = new Dictionary<string, IList<StreamModel>>();
            var licensees = APIService.Licensees();
            foreach (var licensee in licensees)
            {
                var apiSetting = string.Format("{0}?{1}&{2}&{3}&{4}",
                ConfigurationManager.AppSettings["API.Path"],
                "pid=" + licensee.PartnerId,
                "auth=" + licensee.Authentication,
                "filter=streams",
                "days=" + ConfigurationManager.AppSettings["API.Days"]);

                XDocument doc = Convert.ToBoolean(ConfigurationManager.AppSettings["IsLocal"]) ? APIService.ReadXmlFile(licensee.PathTest) : APIService.GetXmlResponse(apiSetting);
                var licenseeStreams = APIService.ProcessEntityElements(doc);
                streams.Add(licensee.PartnerName, licenseeStreams);
            }

            return streams;
        }

        private IList<StreamModel> DataUnion()
        {
            var ibcBet = APIData.Where(x => x.Key == APIService.IBCBet).SelectMany(s => s.Value);
            var bogDog = APIData.Where(x => x.Key == APIService.Bodog).SelectMany(s => s.Value);
            var dafaBet = APIData.Where(x => x.Key == APIService.Dafabet).SelectMany(s => s.Value);
            var fun88 = APIData.Where(x => x.Key == APIService.Fun88).SelectMany(s => s.Value);
            var macauBet = APIData.Where(x => x.Key == APIService.Macaubet).SelectMany(s => s.Value);
            var tcl = APIData.Where(x => x.Key == APIService.Tlc88).SelectMany(s => s.Value);

            var unionData = ibcBet.Union(bogDog, new StreamingComparer())
                                    .Union(dafaBet, new StreamingComparer())
                                    .Union(fun88, new StreamingComparer())
                                    .Union(macauBet, new StreamingComparer())
                                    .Union(tcl, new StreamingComparer());

            return unionData.ToList();

        }
    }
}
