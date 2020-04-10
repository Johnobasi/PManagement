using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using System.Collections.Concurrent;

namespace IMTO.Common
{
    public enum ApplicationType
    {
        WebApplication,
        OtherApplication
    }

    public class Logger
    {

        private string ApplicationName
        {
            get;
            set;
        }

        private string MethodName
        {
            get;
            set;
        }

        private string szAddress
        {
            get;
            set;
        }

        private string logLocation
        {
            get;
            set;
        }

        public Logger(string applicationName)
        {
            this.ApplicationName = applicationName;
            logLocation = ConfigurationManager.AppSettings["LogLocation"].ToString();
        }

        public Logger(string applicationName, string methodName, ApplicationType appType)
        {
            this.ApplicationName = applicationName;
            this.MethodName = methodName;
            logLocation = ConfigurationManager.AppSettings["LogLocation"].ToString();
        }

        public Logger(string applicationName, string methodName, string callerIP)
        {
            this.ApplicationName = applicationName;
            this.MethodName = methodName;
            string text = szAddress = callerIP;
            logLocation = ConfigurationManager.AppSettings["LogLocation"].ToString();
        }

        public string GetApplicationName()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            logLocation = ConfigurationManager.AppSettings["LogLocation"].ToString();
            return executingAssembly.FullName.Split(',')[0];
        }

        public string GetCurrentMethodName()
        {
            return MethodBase.GetCurrentMethod().Name;
        }

        public string LogRequest(string[] logitems)
        {
            return Log("Request", logitems);
        }

        public string LogResponse(string[] logitems)
        {
            return Log("Response", logitems);
        }

        public string Log(string LogType, string[] logitems)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string str = ApplicationName + "Log.txt";
                string text = GetFilename() + str;
                DateTime.Compare(DateTime.Now, DateTime.UtcNow);
                string str2 = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
                try
                {
                    string value = string.Format("{0}.....{1}-Details-{2}", LogType, MethodName, string.Join(":", (from s in logitems
                                                                                                                   select "{" + s + "}").ToArray()));
                    string.Join(":", (from s in logitems
                                      select "{" + s + "}").ToArray());
                    if (Directory.Exists(logLocation))
                    {
                        if (Directory.Exists(logLocation + ApplicationName + "/"))
                        {
                            string path = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
                            {
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(value);
                                streamWriter.WriteLine(stringBuilder.ToString());
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(logLocation + ApplicationName + "/");
                            string path2 = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter2 = new StreamWriter(path2, append: true))
                            {
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter2.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(value);
                                streamWriter2.WriteLine(stringBuilder.ToString());
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter2.Flush();
                                streamWriter2.Close();
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(logLocation);
                        Directory.CreateDirectory(logLocation + ApplicationName + "/");
                        string path3 = logLocation + ApplicationName + text;
                        using (StreamWriter streamWriter3 = new StreamWriter(path3, append: true))
                        {
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.WriteLine("## " + str2 + " ## ");
                            stringBuilder.AppendLine(value);
                            streamWriter3.WriteLine(stringBuilder.ToString());
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.Flush();
                            streamWriter3.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(ex.Message);
                }
            }
            catch (Exception ex2)
            {
                stringBuilder.AppendLine(ex2.Message);
            }
            return stringBuilder.ToString();
        }

        public string LogError(string[] logitems)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string str = ApplicationName + "ErrorLog.txt";
                string text = GetFilename() + str;
                DateTime.Compare(DateTime.Now, DateTime.UtcNow);
                string str2 = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
                try
                {
                    string value = string.Format("Error.....{0}-Details-{1}", MethodName, string.Join("\n", (from s in logitems
                                                                                                             select "{" + s + "}").ToArray()));
                    if (Directory.Exists(logLocation))
                    {
                        if (Directory.Exists(logLocation + ApplicationName + "/"))
                        {
                            string path = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
                            {
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(value);
                                streamWriter.WriteLine(stringBuilder.ToString());
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(logLocation + ApplicationName + "/");
                            string path2 = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter2 = new StreamWriter(path2, append: true))
                            {
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter2.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(value);
                                streamWriter2.WriteLine(stringBuilder.ToString());
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter2.Flush();
                                streamWriter2.Close();
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(logLocation);
                        Directory.CreateDirectory(logLocation + ApplicationName + "/");
                        string path3 = logLocation + ApplicationName + text;
                        using (StreamWriter streamWriter3 = new StreamWriter(path3, append: true))
                        {
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.WriteLine("## " + str2 + " ## ");
                            stringBuilder.AppendLine(value);
                            streamWriter3.WriteLine(stringBuilder.ToString());
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.Flush();
                            streamWriter3.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(ex.Message);
                }
            }
            catch (Exception ex2)
            {
                stringBuilder.AppendLine(ex2.Message);
            }
            return stringBuilder.ToString();
        }

        public string LogError(Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string str = ApplicationName + "ErrorLog.txt";
                string text = GetFilename() + str;
                List<string> list = new List<string>();
                list.Add(exception.Message);
                list.Add(exception.StackTrace);
                string[] source = list.ToArray();
                DateTime.Compare(DateTime.Now, DateTime.UtcNow);
                string str2 = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
                try
                {
                    string value = string.Format("Error.....{0}-Details-{1}", MethodName, string.Join("\n", (from s in source
                                                                                                             select "{" + s + "}").ToArray()));
                    if (Directory.Exists(logLocation))
                    {
                        if (Directory.Exists(logLocation + ApplicationName + "/"))
                        {
                            string path = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
                            {
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(value);
                                streamWriter.WriteLine(stringBuilder.ToString());
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(logLocation + ApplicationName + "/");
                            string path2 = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter2 = new StreamWriter(path2, append: true))
                            {
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter2.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(value);
                                streamWriter2.WriteLine(stringBuilder.ToString());
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter2.Flush();
                                streamWriter2.Close();
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(logLocation);
                        Directory.CreateDirectory(logLocation + ApplicationName + "/");
                        string path3 = logLocation + ApplicationName + text;
                        using (StreamWriter streamWriter3 = new StreamWriter(path3, append: true))
                        {
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.WriteLine("## " + str2 + " ## ");
                            stringBuilder.AppendLine(value);
                            streamWriter3.WriteLine(stringBuilder.ToString());
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.Flush();
                            streamWriter3.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(ex.Message);
                }
            }
            catch (Exception ex2)
            {
                stringBuilder.AppendLine(ex2.Message);
            }
            return stringBuilder.ToString();
        }

        public string Log(string msg)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string str = ApplicationName + "Log.txt";
                string text = GetFilename() + str;
                DateTime.Compare(DateTime.Now, DateTime.UtcNow);
                string str2 = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
                try
                {
                    if (Directory.Exists(logLocation))
                    {
                        if (Directory.Exists(logLocation + ApplicationName + "/"))
                        {
                            string path = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
                            {
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(msg);
                                streamWriter.WriteLine(stringBuilder.ToString());
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(logLocation + ApplicationName + "/");
                            string path2 = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter2 = new StreamWriter(path2, append: true))
                            {
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter2.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(msg);
                                streamWriter2.WriteLine(stringBuilder.ToString());
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter2.Flush();
                                streamWriter2.Close();
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(logLocation);
                        Directory.CreateDirectory(logLocation + ApplicationName + "/");
                        string path3 = logLocation + ApplicationName + text;
                        using (StreamWriter streamWriter3 = new StreamWriter(path3, append: true))
                        {
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.WriteLine("## " + str2 + " ## ");
                            stringBuilder.AppendLine(msg);
                            streamWriter3.WriteLine(stringBuilder.ToString());
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.Flush();
                            streamWriter3.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(ex.Message);
                    msg = "";
                }
            }
            catch (Exception ex2)
            {
                stringBuilder.AppendLine(ex2.Message);
                msg = "";
            }
            return stringBuilder.ToString();
        }

        private string GetFilename()
        {

            string logCycle = ConfigurationManager.AppSettings["LogCycle"] ?? "Daily";  //FourHourly, //TwoHourly, Hourly
            if (logCycle == "Daily") return DateTime.Now.ToString("dd-MM-yyyy");

            int divider = 1; //hourly
            if (logCycle == "FourHourly") divider = 6;
            if (logCycle == "TwoHourly") divider = 2;

            int hrs = (int)(DateTime.Now.Hour / divider);
            string text = string.Format("{0}-{1}", DateTime.Now.ToString("dd-MM-yyyy"), hrs.ToString());
            return text;
        }

        public string LogError(string msg)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string str = ApplicationName + "ErrorLog.txt";
                string text = GetFilename() + str;
                DateTime.Compare(DateTime.Now, DateTime.UtcNow);
                string str2 = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
                try
                {
                    if (Directory.Exists(logLocation))
                    {
                        if (Directory.Exists(logLocation + ApplicationName + "/"))
                        {
                            string path = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter = new StreamWriter(path, append: true))
                            {
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(msg);
                                streamWriter.WriteLine(stringBuilder.ToString());
                                streamWriter.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(logLocation + ApplicationName + "/");
                            string path2 = logLocation + ApplicationName + "/" + text;
                            using (StreamWriter streamWriter2 = new StreamWriter(path2, append: true))
                            {
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "----------------------------------------");
                                streamWriter2.WriteLine("## " + str2 + " ## ");
                                stringBuilder.AppendLine(msg);
                                streamWriter2.WriteLine(stringBuilder.ToString());
                                streamWriter2.WriteLine("---------------------REQUEST FROM " + szAddress + "------------------------------------------");
                                streamWriter2.Flush();
                                streamWriter2.Close();
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(logLocation);
                        Directory.CreateDirectory(logLocation + ApplicationName + "/");
                        string path3 = logLocation + ApplicationName + text;
                        using (StreamWriter streamWriter3 = new StreamWriter(path3, append: true))
                        {
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.WriteLine("## " + str2 + " ## ");
                            stringBuilder.AppendLine(msg);
                            streamWriter3.WriteLine(stringBuilder.ToString());
                            streamWriter3.WriteLine("----------------------------REQUEST FROM " + szAddress + "-----------------------------------");
                            streamWriter3.Flush();
                            streamWriter3.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    stringBuilder.AppendLine(ex.Message);
                    msg = "";
                }
            }
            catch (Exception ex2)                                                                                                                                                        
            {
                stringBuilder.AppendLine(ex2.Message);
                msg = "";
            }
            return stringBuilder.ToString();
        }
    }
}
