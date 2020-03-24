using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using StreamingBooking.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace StreamingBooking
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            LogManager
                    .SetDefaultLogCategory(ConfigurationManager.AppSettings["Fanex.Logging-LogCategory"])
                    .Use(new List<ILogEngine>
                     {
                         new RabbitMQLogging(ConfigurationManager.AppSettings["RabbitMQ-ConnectionString"])
                     });
            CacheServiceManager.Use(() => new RedisCache(new RedisCacheOptions
            {
                ConnectionString = ConfigurationManager.AppSettings["RedisServer"],
                ApplicationName = ConfigurationManager.AppSettings["RedisAppName"]
            }));
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();
            string message = LogMessageHelper.GenerateMessage(ex);
            LogService.Error(message);
        }
    }
}