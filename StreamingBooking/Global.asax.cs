using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;
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
            var messageBuilder = new StringBuilder();
            AddExceptionInfo(messageBuilder, ex, 3);

            string message = messageBuilder.ToString() +
                    "\r\nDomain: " + HttpContext.Current.Request.Headers["Host"];
            LogService.Error(message);
        }

        private static void AddExceptionInfo(StringBuilder builder, Exception exception,
            int limitedInnerException)
        {
            builder.AppendLine();
            builder.AppendLine("EXCEPTION INFO");
            builder.AppendLine($"Message: {exception.Message}");
            builder.AppendLine($"Source: {exception.Source}");
            builder.AppendLine($"TargetSite: {exception.TargetSite}");
            builder.AppendLine($"StackTrace: {exception.StackTrace}");
            if (exception.InnerException != null && limitedInnerException > 0)
            {
                limitedInnerException--;
                AddExceptionInfo(builder, exception.InnerException, limitedInnerException);
            }
        }
    }
}