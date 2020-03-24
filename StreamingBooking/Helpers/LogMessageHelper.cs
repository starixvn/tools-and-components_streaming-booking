using System;
using System.Text;
using System.Web;

namespace StreamingBooking.Helpers
{
    public static class LogMessageHelper
    {
        public static string GenerateMessage(Exception exception, int limitedInnerException = 3)
        {
            var messageBuilder = new StringBuilder();
            AddExceptionInfo(messageBuilder, exception, limitedInnerException);
            messageBuilder.Append("\r\nDomain: " + HttpContext.Current.Request.Headers["Host"]);

            return messageBuilder.ToString();
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