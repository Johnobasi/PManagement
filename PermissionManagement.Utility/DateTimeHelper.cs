using System;

namespace PermissionManagement.Utility
{
    public class DateTimeHelper
    {
        public DateTime? ConvertToDateTime(string dateTime, bool eod = false, string format = "")
        {
            DateTime dt;
            if (DateTime.TryParse(dateTime, out dt))
            {
                dt = eod ? dt.AddDays(1).AddMinutes(-1) : dt;
                if (format != string.Empty)
                {
                    string strDate = dt.ToString(format);
                    DateTime.TryParse(strDate, out dt);
                    return dt;
                }
                return dt;
            }
            return null;
        }

        public string FormatDateTime(string dateTime, bool eod = false, string format = "")
        {
            DateTime dt;
            if (DateTime.TryParse(dateTime, out dt))
            {
                dt = eod ? dt.AddDays(1).AddMinutes(-1) : dt;
                if (format != string.Empty)
                {
                    string strDate = dt.ToString(format);
                    return strDate;
                }
                dt.ToString();
            }
            return null;
        }
    }
}

