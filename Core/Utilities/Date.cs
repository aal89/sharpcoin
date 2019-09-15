using System;

namespace Core.Utilities
{
    public static class Date
    {
        public static DateTime Now()
        {
            return DateTime.ParseExact(DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), "MM/dd/yyyy HH:mm:ss", null);
        }

        public static string FormattedString(this DateTime self)
        {
            // this format string operation is here to force the same string representation on different platforms
            return String.Format("{0:MM/dd/yyyy HH:mm:ss}", self).Replace("-", "/");
        }
    }
}
