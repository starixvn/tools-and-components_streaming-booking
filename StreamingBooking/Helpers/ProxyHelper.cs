using System;
using System.Configuration;
using System.Net;

namespace StreamingBooking.Helpers
{
    public static class ProxyHelper
    {
        public static WebProxy CreateWebProxy()
        {
            var proxyAddress = ConfigurationManager.AppSettings["Proxy"];
            if (string.IsNullOrEmpty(proxyAddress))
            {
                return null;
            }

            return new WebProxy
            {
                Address = new Uri(proxyAddress),
                UseDefaultCredentials = true
            };
        }
    }
}