using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using Fanex.Logging;
using StreamingBooking.Helpers;
using StreamingBooking.Models;

namespace StreamingBooking.Services
{
    public static class UnasApiService
    {
        public static string IBCBet
        {
            get
            {
                return "IBCBet";
            }
        }

        public static string Bodog
        {
            get
            {
                return "Bodog";
            }
        }

        public static string Dafabet
        {
            get
            {
                return "Dafabet";
            }
        }

        public static string Fun88
        {
            get
            {
                return "Fun88";
            }
        }

        public static string Macaubet
        {
            get
            {
                return "Macaubet";
            }
        }

        public static string Tlc88
        {
            get
            {
                return "Tlc88";
            }
        }

        public static string M88
        {
            get
            {
                return "M88";
            }
        }

        public static XDocument GetXmlResponse(string requestUrl)
        {
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            XDocument xmlDoc = null;

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (var stream = response.GetResponseStream())
                {
                    StreamReader resStream = new StreamReader(stream, Encoding.UTF8);
                    var xmlText = resStream.ReadToEnd();
                    xmlDoc = XDocument.Parse(xmlText);
                }
            }

            return xmlDoc;
        }

        public static XDocument ReadXmlFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
            return doc;
        }

        public static List<StreamModel> ProcessEntityElements(string partnerName, XDocument dataResponsed)
        {
            var listOfStream = new List<StreamModel>();

            IEnumerable<XElement> events = from el in dataResponsed.Descendants("event")
                                           select el;

            var eventId = 0;
            var awayName = string.Empty;
            var homeName = string.Empty;
            var kickOffDate = DateTime.Now;
            var leagueName = string.Empty;
            var streamId = 0;
            var sportName = string.Empty;

            foreach (XElement el in events)
            {
                try
                {                   
                    eventId = int.Parse(el.Element("eventid").Value);
                    awayName = el.Element("sportinfo").Element("teamA").Value;
                    homeName = el.Element("sportinfo").Element("teamB").Value;
                    leagueName = el.Element("sportinfo").Element("liga").Value;
                    streamId = el.Element("stream") != null ? int.Parse(el.Element("stream").Element("streamid").Value) : 0;
                    sportName = el.Element("sportinfo").Element("sport").Value;
                    kickOffDate = DateTime.Parse(el.Element("schedule").Element("date").Value);
                    var time = el.Element("schedule").Element("start").Element("time").Value.Split(':');
                    kickOffDate = kickOffDate.Add(new TimeSpan(ushort.Parse(time[0]), ushort.Parse(time[1]), default(ushort))).ToClientTime(true);

                    listOfStream.Add(
                        new StreamModel
                        {
                            EventId = eventId,
                            AwayName = awayName,
                            HomeName = homeName,
                            KickOffDate = kickOffDate,
                            LeagueName = leagueName,
                            StreamId = streamId,
                            SportName = sportName,                           
                            Dafabet = partnerName == Dafabet,
                            Fun88 = partnerName == Fun88,
                            IBCBet = partnerName == IBCBet,
                            M88 = partnerName == M88                            
                        });
                }
                catch
                {
                    throw new NotSupportedException(el.ToString());
                }
            }

            return listOfStream;
        }

        public static List<LicenseeModel> Licensees()
        {
            var licensee = new List<LicenseeModel>();

            licensee.Add(new LicenseeModel
            {
                PartnerId = 31,
                PartnerName = UnasApiService.IBCBet,
                APISecret = "ET31DPYd",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.0.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 47,
                PartnerName = UnasApiService.Bodog,
                APISecret = "BYE1QRgW",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.1.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 42,
                PartnerName = UnasApiService.Dafabet,
                APISecret = "k2vSVp6W",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.2.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 45,
                PartnerName = UnasApiService.Fun88,
                APISecret = "ENCmG6t1",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.3.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 38,
                PartnerName = UnasApiService.Macaubet,
                APISecret = "UxcjUrMw",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.4.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 51,
                PartnerName = UnasApiService.Tlc88,
                APISecret = "uG4pxEKM",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.5.xml")
            });

            licensee.Add(new LicenseeModel
            {
                PartnerId = 39,
                PartnerName = UnasApiService.M88,
                APISecret = "uCQO84ifdI",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.6.xml")
            });

            return licensee;
        }
    }
}