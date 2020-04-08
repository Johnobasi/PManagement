using System;
using System.Globalization;
using System.IO;

namespace PermissionManagement.Repository.Implementation
{
    public class CreateLogFiles
    {
        private string _sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
        private string _sErrorTime;
        public CreateLogFiles()
        {
            var sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString(); _sErrorTime = sYear + sMonth + sDay;
        }
        public static void ErrorLog(string sErrMsg)
        {
            string path = GetPath(); StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine("\n {0} \n", sErrMsg); sw.Flush(); sw.Close();
        }
        public static void DebugLog(string sErrMsg)
        {
            string path = GetPath(); StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(" \n DebugLog DateTime: \t {0},{1}:", DateTime.Now.ToString(CultureInfo.InvariantCulture), sErrMsg); sw.Flush(); sw.Close();
        }
        private static string GetPath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ErrorLog"];
        }

        public static bool CheckIsDebug
        {
            get { return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["isDebugInfo"]); }
        }
    }
}
