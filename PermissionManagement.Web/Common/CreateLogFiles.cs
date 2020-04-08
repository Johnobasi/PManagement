using System;
using System.Globalization;
using System.IO;
using System.Web.Mvc;

namespace OtpManagement.Common
{
    public class CreateLogFiles
    {
        private string _sLogFormat;
        private string _sErrorTime;
        public CreateLogFiles()
        {
            _sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
            string sYear = DateTime.Now.Year.ToString();string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();_sErrorTime = sYear + sMonth + sDay;
        }
        public static void ErrorLog(string sErrMsg)
        {
            string path = GetPath();StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine("\n {0} \n", sErrMsg);sw.Flush(); sw.Close();
        }
        public static void DebugLog(string sErrMsg)
        {
            string path = GetPath(); StreamWriter sw = new StreamWriter(path, true);
            sw.WriteLine(" \n DebugLog DateTime: \t {0},{1}:", DateTime.Now.ToString(CultureInfo.InvariantCulture), sErrMsg);sw.Flush(); sw.Close();
        }
        private static string GetPath() { return System.Configuration.ConfigurationManager.AppSettings["ErrorLog"].ToString(); }
    }
    public class ErrorLog : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Exception ex = filterContext.Exception; filterContext.ExceptionHandled = true;
            var model = new HandleErrorInfo(filterContext.Exception, "Controller", "Action");
            // Log Exception ex in database
            CreateLogFiles.ErrorLog("ControllerName Name" + model.ControllerName + "\t Action Name" + model.ActionName + "Error:" + model.Exception.Message + " \t  InnerException:" + model.Exception.InnerException + "Error StackTrace:" + model.Exception.StackTrace);
            // Notify  admin team
            filterContext.Result = new ViewResult() { ViewName = "Error", ViewData = new ViewDataDictionary(model) };
        }
    }
}