using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Xml.Linq;
using StreamingBooking.Models;

namespace StreamingBooking.Services
{
    public static class APIService
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

        public static XDocument GetXmlResponse(string requestUrl)
        {
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;


            XDocument xmlDoc = XDocument.Load(response.GetResponseStream());

            return (xmlDoc);
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
                eventId = int.Parse(el.Element("eventid").Value);
                awayName = el.Element("sportinfo").Element("teamA").Value;
                homeName = el.Element("sportinfo").Element("teamB").Value;
                kickOffDate = DateTime.Parse(el.Element("schedule").Element("date").Value);
                leagueName = el.Element("sportinfo").Element("liga").Value;
                streamId = int.Parse(el.Element("stream").Element("streamid").Value);
                sportName = el.Element("sportinfo").Element("sport").Value;
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
                        Bodog = partnerName == Bodog,
                        Dafabet = partnerName == Dafabet,
                        Fun88 = partnerName == Fun88,
                        IBCBet = partnerName == IBCBet,
                        Macaubet = partnerName == Macaubet,
                        TLC = partnerName == Tlc88
                    });
            }

            return listOfStream;
        }

        public static List<LicenseeModel> Licensees()
        {
            var licensee = new List<LicenseeModel>();

            licensee.Add(new LicenseeModel
            {
                PartnerId = 31,
                PartnerName = APIService.IBCBet,
                APISecret = "ET31DPYd",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.0.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 47,
                PartnerName = APIService.Bodog,
                APISecret = "BYE1QRgW",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.1.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 42,
                PartnerName = APIService.Dafabet,
                APISecret = "k2vSVp6W",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.2.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 45,
                PartnerName = APIService.Fun88,
                APISecret = "ENCmG6t1",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.3.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 38,
                PartnerName = APIService.Macaubet,
                APISecret = "UxcjUrMw",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.4.xml")
            });
            licensee.Add(new LicenseeModel
            {
                PartnerId = 51,
                PartnerName = APIService.Tlc88,
                APISecret = "uG4pxEKM",
                PathTest = HostingEnvironment.MapPath("~/App_Data/api.event.rev.103.php.5.xml")
            });

            return licensee;
        }
    }
}