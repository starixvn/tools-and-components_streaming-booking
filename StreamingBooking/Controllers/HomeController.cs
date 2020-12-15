using StreamingBooking.Attribute;
using StreamingBooking.Common;
using StreamingBooking.Helpers;
using StreamingBooking.Models;
using StreamingBooking.Services;
using StreamingBooking.Services.Excel;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StreamingBooking.Controllers
{
    [HandleException]
    public class HomeController : Controller
    {
        private readonly IDataService dataService = new DataService();
        private readonly IActivityService activityService = new ActivityService();
        private readonly IHomeStreamingBookingExcel _homeStreamingBookingExcel = new HomeStreamingBookingExcel();

        public string RemoveCache(string licensee)
        {
            return dataService.ClearCache(licensee).ToString();
        }

        public ActionResult Index()
        {
            var days = Convert.ToInt32(ConfigurationManager.AppSettings["API.Days"]);
            var currentDay = DateTime.Now.ToClientTime();
            var nextDay = currentDay.AddDays(days);

            var filterDates = new List<string>();
            while (currentDay < nextDay)
            {
                filterDates.Add(currentDay.ToString("MM/dd/yyyy"));
                currentDay = currentDay.AddDays(1);
            }

            var licensees = GetLicensees();
            var providers = GetProviders();
            var sports = GetSportNames();

            var model = new StreamViewModel
            {
                From = filterDates,
                To = filterDates,
                Leagues = new List<string>(),
                Licensees = licensees,
                Providers = providers,
                Sports = sports
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Search(
            string fromDate,
            string toDate,
            string league,
            string licensee,
            string sports,
            int provider)
        {
            var model = GetData(fromDate, toDate, league, licensee, sports, provider);

            return View(model);
        }

        private StreamViewModel GetData(
            string fromDate,
            string toDate,
            string league,
            string licensee,
            string sports,
            int provider)
        {
            var allStreams = dataService.Get(new StreamMetalModel
            {
                Licensee = licensee,
                Provider = provider,
                From = fromDate,
                To = toDate
            });

            var model = new StreamViewModel
            {
                AllStreams = allStreams,
                DistinctStreams = new List<StreamModel>()
            };

            if (allStreams.Count > 0)
            {
                Predicate<StreamModel> kickOffDateFilter = _ => _.KickOffDate.Date >= DateTime.ParseExact(fromDate, "MM/dd/yyyy", null)
                && _.KickOffDate.Date <= DateTime.ParseExact(toDate, "MM/dd/yyyy", null);
                Predicate<StreamModel> leagueFilter = _ => string.IsNullOrWhiteSpace(league) || _.LeagueName == league;
                Predicate<StreamModel> sportFilter = _ => string.IsNullOrWhiteSpace(sports) || sports.Split(',').Any(s => s == _.SportName);

                model.DistinctStreams = allStreams
                                            .Where(_ => kickOffDateFilter(_) && leagueFilter(_) && sportFilter(_))
                                            .ToList();
            }

            var activeStreams = activityService.GetActiveStream();
            if (activeStreams != null && activeStreams.Any())
            {
                model.DistinctStreams = (from s in model.DistinctStreams
                                         join a in activeStreams
                                    on s.StreamId equals a.StreamId
                                    into r
                                         from ax in r.DefaultIfEmpty(new StreamModel())
                                         select new StreamModel
                                         {
                                             ALog = s.ALog,
                                             AwayName = s.AwayName,
                                             Dafabet = s.Dafabet,
                                             EventId = s.EventId,
                                             StreamId = s.StreamId,
                                             SportName = s.SportName,
                                             Fun88 = s.Fun88,
                                             HomeName = s.HomeName,
                                             IBCBet = s.IBCBet,
                                             IsActive = ax.IsActive,
                                             KickOffDate = s.KickOffDate,
                                             LeagueName = s.LeagueName,
                                             M88 = s.M88,
                                             Mayfair = s.Mayfair,
                                             Remark = ax.Remark,
                                             ProviderName = s.ProviderName
                                         }).ToList();
            }

            return model;
        }



        [HttpGet]
        public JsonResult GetLeagues(int provider, string licensee, string fromDate, string toDate)
        {
            var matchedStreamData = dataService.Get(new StreamMetalModel { Provider = provider, Licensee = licensee });

            if (!string.IsNullOrWhiteSpace(fromDate) && !string.IsNullOrWhiteSpace(toDate))
            {
                var filterFromDate = Convert.ToDateTime(fromDate);
                var filterToDate = Convert.ToDateTime(toDate);

                matchedStreamData = matchedStreamData
                                           .Where(x => x.KickOffDate >= filterFromDate && x.KickOffDate <= filterToDate)
                                           .ToList();
            }

            var leagues = matchedStreamData.GroupBy(x => x.LeagueName).Select(s => s.Key).ToList();

            return Json(leagues, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetLicenseesByProvider(int provider)
        {
            var licensees = GetLicensees(provider);

            return Json(licensees, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void UpdateActiveRow(int streamId)
        {
            if (streamId > 0)
            {
                activityService.ActiveRow(streamId);
            }
        }

        [HttpPost]
        public void UpdateRemark(int streamId, string remark)
        {
            if (streamId > 0)
            {
                activityService.UpdateRemark(streamId, HttpUtility.HtmlEncode(remark));
            }
        }

        [HttpGet]
        public FileContentResult Export(
            string fromDate,
            string toDate,
            string league,
            string licensee,
            string sports,
            int provider)
        {
            using (var memoryStream = new MemoryStream())
            {
                var model = GetData(fromDate, toDate, league, licensee, sports, provider);
                // Generate Excel
                var workbook = _homeStreamingBookingExcel.Export(model);
                workbook.Write(memoryStream);

                return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SB-" + DateTime.UtcNow.ToClientTime(true).ToString("dd-MM-yyyy") + ".xlsx");
            }
        }

        private static List<SelectListItem> GetProviders()
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

        private static List<string> GetLicensees(int provider = 0)
        {
            var allLicensees = new List<string>();
            var unasLicensees = new List<string> {
                    //APIService.Bodog,
                    UnasApiService.Dafabet,
                    //APIService.Fun88,
                    UnasApiService.IBCBet,
                    UnasApiService.M88
                    //APIService.Macaubet,
                    //APIService.Tlc88
            };
            var imgLicensees = new List<string> {
                    ImgApiService.Fun88,
                    ImgApiService.ALog,
                    ImgApiService.Mayfair
            };

            switch (provider)
            {
                case (int)ProviderType.IMG:
                    allLicensees = imgLicensees;
                    break;

                //case (int)ProviderType.Unas:
                //    allLicensees = unasLicensees;
                //    break;

                default:
                    allLicensees = unasLicensees.Union(imgLicensees).ToList();
                    break;
            }

            allLicensees = allLicensees.Distinct().OrderBy(x => x).ToList();

            return allLicensees;
        }

        private List<string> GetSportNames()
        {
            var allStreams = dataService.Get(new StreamMetalModel());

            return allStreams?
                .Select(_ => _.SportName)
                .Distinct()
                .OrderBy(_ => _).ToList() ?? new List<string>();
        }
    }
}