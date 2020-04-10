using Microsoft.VisualBasic;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml.Serialization;


namespace PermissionManagement.Utility
{
     public class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder sb)
            : base(sb)
        {
        }
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public class Helper
    {

        public static string vbLf = Convert.ToChar(10).ToString();
        public static string vbCr = System.Environment.NewLine;

        public static int GetLogLevel()
        {
            //default to log level 2 if not specified in web config
            var s = ConfigurationManager.AppSettings["AuditLogLevel"];
            return string.IsNullOrEmpty(s) ? 2 : int.Parse(s);
        }

        public static string GetLoggedInUser()
        {
            var c = HttpContext.Current;
            if (c == null) return "anonymous";
            if (c.User.Identity as Identity != null)
                return ((Identity)c.User.Identity).FullName;
            else
                return "Anonymous User";
        }

        public static string GetLoggedInUserID()
        {
            var c = HttpContext.Current;
            if (c == null) return "anonymous";
            if (c.User.Identity as Identity != null)
                return ((Identity)c.User.Identity).Name;
            else
                return "anonymous";
        }

        public static string UrlEncode(string urlToEncode)
        {
            return System.Web.HttpUtility.UrlEncode(urlToEncode);
        }

        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) { return false; }
            System.Globalization.CompareInfo cmpUrl = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            if (cmpUrl.IsPrefix(url, "http://") == false && cmpUrl.IsPrefix(url, "https://") == false)
            {
                url = "http://" + url;
            }

            //Regex urlRegex = new Regex("(^|[^\\w'\"]|\\G)(?<uri>(?:https?|ftp)&#58;&#47;&#47;(?:[^./\\s'\"<)\\]]+\\.)+[^./\\s'\"<)\\]]+(?:&#47;.*?)?)(\\.[\\s<'\"]|[\\s,<'\"]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Regex("^(([^:/?#]+):)?(//([^/?#]*))?([^?#]*)(/?([^#]*))?(#(.*))?");
            //^((https?|ftp)://|(www|ftp)\.)[a-z0-9-]+(\.[a-z0-9-]+)+([/?].*)?$
            Regex urlRegex = new Regex("^(([^:/?#]+):)?(//([^/?#]*))?([^?#]*)(/?([^#]*))?(#(.*))?");
            return urlRegex.IsMatch(url);
        }

        public static bool IsDuplicateEmailPermitted()
        {
            var s = ConfigurationManager.AppSettings["PermitDuplicateEmail"];
            return !string.IsNullOrEmpty(s) && s == "true";
        }

        public static bool IsAcceptNoEmail()
        {
            var acceptNoEmail = ConfigurationManager.AppSettings["IsAcceptNoEmail"];
            return !(string.IsNullOrEmpty(acceptNoEmail) || acceptNoEmail != "true");
        }

        public static bool IsAuthenticated()
        {
            return (HttpContext.Current.User.Identity.IsAuthenticated);
        }

        public static string GetLoginUrl()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return Helper.GetRootURL() + "/Login";
            return Helper.GetRootURL() + "/Logout";
        }
        public static string GetLoginState()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return "in";
            return "out";
        }

        public static string GetHomeURL()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return string.Format("{0}/admin", Helper.GetRootURL());
            return Helper.GetRootURL();
        }
        public static string Replace(string s, string oldValue, string newValue, StringComparison comparisonType)
        {

            if (string.IsNullOrEmpty(s))
                return s;
            if (string.IsNullOrEmpty(oldValue) || newValue == null)
                return s;

            StringBuilder result = new StringBuilder();
            int lenOldValue = oldValue.Length;
            int curPosition = 0;

            int idxNext = s.IndexOf(oldValue, comparisonType);
            while (idxNext >= 0)
            {
                result.Append(s, curPosition, idxNext - curPosition);
                result.Append(newValue);
                curPosition = idxNext + lenOldValue;
                idxNext = s.IndexOf(oldValue, curPosition, comparisonType);
            }

            result.Append(s, curPosition, s.Length - curPosition);
            return result.ToString();
        }

        public static string MakeLink(string url, string text)
        {

            return System.Web.HttpUtility.HtmlEncode(string.Format("<a href='{0}'>{1}</a>", url, text));

        }

        public static string GetIPAddress()
        {
            if (HttpContext.Current.Request == null)
                return "127.0.0.1";

            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static System.DateTime GetLocalDate()
        {
            string offset = ConfigurationManager.AppSettings["TimeZone"];
            if (string.IsNullOrEmpty(offset))
            {
                return System.DateTime.Now;
            }
            else
            {
                var now = DateTime.UtcNow;
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(offset);
                return TimeZoneInfo.ConvertTime(now, timeZoneInfo);
            }
        }

        public static string RemoveCrLf(string s)
        {
            if (s == null || s.Length == 0)
            {
                return string.Empty;
            }
            char c = '\0';
            int i = 0;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            string t = null;

            for (i = 0; i <= len - 1; i++)
            {
                c = s[i];
                //OrElse (c = ">"c) Then
                if ((c == '\\') || (c == '"'))
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
                else if (c == ControlChars.Back)
                {
                    sb.Append(string.Empty);
                    //b
                }
                else if (c == ControlChars.Tab)
                {
                    sb.Append("");
                    //\t
                }
                else if (c == ControlChars.Lf)
                {
                    sb.Append("");
                    //\n
                }
                else if (c == ControlChars.FormFeed)
                {
                    sb.Append("");
                    //\f
                }
                else if (c == ControlChars.Cr)
                {
                    sb.Append("");
                    //\r
                }
                else
                {
                    if (c < ' ')
                    {
                        t = new string(c, 1);
                        t = "000" + int.Parse(t, System.Globalization.NumberStyles.HexNumber);
                        sb.Append("\\u" + t.Substring(t.Length - 4));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }

        public static string ToTitleCase(string input)
        {

            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var curCulture = Thread.CurrentThread.CurrentCulture;
            return curCulture.TextInfo.ToTitleCase(input);

        }

        public static bool IsItemExistInList(string[] list, string item)
        {

            if ((list == null | string.IsNullOrEmpty(item)))
                return false;

            string foundItem = (from l in list where l.ToLower() == item.ToLower() select l).FirstOrDefault();

            return !string.IsNullOrEmpty(foundItem);

        }

        public static bool AreItemsInList(string[] list, string[] items)
        {

            if ((list == null || items == null))
                return false;

            var count = list.Intersect(items).ToList().Count;

            return count == items.Length;

        }

        public static bool IsAnyItemInList(string[] list, string[] items)
        {

            if ((list == null || items == null))
                return false;
            bool hasMatch = list.Select(x => x)
                          .Intersect(items)
                          .Any();

            return hasMatch;
        }

        public static string GetRootPath()
        {

            if (HttpContext.Current == null)
            {
                return null;
            }
            string output = HttpContext.Current.Request.PhysicalApplicationPath;
            return output;

        }

        public static bool IsValidGuid(string value)
        {

            if ((string.IsNullOrEmpty(value)))
                return false;

            Regex guidRegex = new Regex("^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$", RegexOptions.Compiled);

            return guidRegex.IsMatch(value);

        }

        public static string CleanFilename(string filename)
        {

            var r = new Regex("[^\\w\\.-]", RegexOptions.IgnoreCase);
            return r.Replace(filename, "_");

        }
        public static string GetRootURL()
        {
            return GetRootURL(false);
        }

        public static string GetRootURL(bool removeVirtualDirectory)
        {
            var rootUrl = string.Empty;
            if (removeVirtualDirectory)
               rootUrl = HttpContext.Current.Application["RootURL-RMV"] as string;
            if (!removeVirtualDirectory)
                rootUrl = HttpContext.Current.Application["RootURL"] as string;

            if (!string.IsNullOrEmpty(rootUrl)) return rootUrl;

            string output = HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + HttpContext.Current.Request.ApplicationPath;
            if (output.EndsWith("/"))
            {
                // Strip the end slash if it's there
                output = output.Substring(0, output.Length - 1).ToLower();
            }
            output = output.ToLower();
            var url = HttpContext.Current.Request.Url;

            int portNumber = url.Port;
            var virtualDirectory = ConfigurationManager.AppSettings["VirtualDirectory"];
            if (removeVirtualDirectory)
            {
                if (!string.IsNullOrEmpty(virtualDirectory))
                {
                    output = output.Replace(virtualDirectory.ToLower(), string.Empty);
                }
                if (!url.IsDefaultPort)
                {
                    if (output.EndsWith("/"))
                    {
                        output = output.Substring(0, output.Length - 1);
                    }
                    output = string.Format("{0}:{1}", output, portNumber.ToString());
                }
            }
            else
            {
                if (!url.IsDefaultPort)
                {
                    if (!string.IsNullOrEmpty(virtualDirectory))
                    {
                        output = output.Replace("/" + virtualDirectory.ToLower(), ":" + portNumber.ToString() + "/" + virtualDirectory.ToLower());
                    }
                    else
                    {
                        output = string.Format("{0}:{1}", output, portNumber);
                    }
                }
            }
            rootUrl = url.Scheme + "://" + output.ToLower();

            HttpContext.Current.Application.Lock();
            if (removeVirtualDirectory)
                HttpContext.Current.Application["RootURL-RMV"] = rootUrl;
            else
                HttpContext.Current.Application["RootURL"] = rootUrl;

            HttpContext.Current.Application.UnLock();

            return rootUrl;
        }

        public static string ResolveUrl(string url)
        {

            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                return url;
            }
            else
            {
                var virtualDirectory = ConfigurationManager.AppSettings["VirtualDirectory"];
                if (!string.IsNullOrEmpty(virtualDirectory))
                {
                    url = url.ToLower().Replace(virtualDirectory.ToLower(), string.Empty);
                }
                if (url.StartsWith("~"))
                {
                    url = url.Substring(1);
                }

                if (!url.StartsWith("/"))
                {
                    url = "/" + url;
                }
                return string.Format("{0}{1}", Helper.GetRootURL(), url);
            }

        }

        public static string ToStringCSV<T>(T[] list)
        {

            if (list == null || list.Length == 0)
            {
                return string.Empty;
            }
            var value = new StringBuilder();

            foreach (T l in list)
            {
                value.Append(l.ToString());
                value.Append(",");
            }

            return (value.Remove(value.Length - 1, 1)).ToString();

        }

        public static string ToStringCSVWithQuote<T>(T[] list)
        {

            if (list == null || list.Length == 0)
            {
                return string.Empty;
            }
            var value = new StringBuilder();
            value.Append("'");
            foreach (T l in list)
            {
                value.Append(l.ToString());
                value.Append("','");
            }

            return (value.Remove(value.Length - 2, 2)).ToString();

        }

        public static string[] CSVToStringArray(string input)
        {

            if (string.IsNullOrEmpty(input))
                return null;
            string[] inProcess = null;
            inProcess = input.Split(',');
            return inProcess;

        }

        public static int CountWords(string para)
        {

            bool inWord = false;
            int words = 0;
            foreach (char c in para)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (inWord)
                    {
                        words += 1;
                    }
                    inWord = false;
                    continue;
                }
                inWord = true;
            }
            if (inWord)
            {
                words += 1;
            }

            return words;

        }

        public static bool IsTextFile(string fileName)
        {

            bool status = false;
            string extensionTypes = ".txt,.csv";
            string fileExtension = GetFileExtension(fileName);
            if (extensionTypes.ToLower().Contains(fileExtension.ToLower()))
            {
                status = true;
            }

            return status;

        }

        public static string GetFileExtension(string fileName)
        {

            if (fileName == null)
            {
                return string.Empty;
            }
            if (fileName.LastIndexOf(".") == -1)
            {
                return string.Empty;
            }
            return fileName.Substring(fileName.LastIndexOf(".") + 1);

        }

        public static int ConvertBytesToKiloBytes(int sizeInBytes)
        {

            return sizeInBytes / 1024;

        }

        public static bool IsSizeWithinBounds(int maxFileSize, int fileSize)
        {
            return fileSize <= maxFileSize;
        }

        public static string Serialize(object item)
        {

            StringBuilder sb = new StringBuilder();

            using (System.IO.StringWriter s = new Utf8StringWriter(sb))
            {
                XmlSerializer serializer = new XmlSerializer(item.GetType());
                serializer.Serialize(s, item);
            }

            return sb.ToString();

        }

        public static string StripHtml(string input)
        {
            return StripHtml(input, false);
        }

        public static string StripHtml(string input, bool stripAll)
        {

            if (stripAll)
            {
                input = Regex.Replace(input, "(<[^>]+>)", string.Empty, RegexOptions.Multiline);
                //input = Regex.Replace(input, "<img.*?src.*?=.*?[""|'](.*?)[""|'].*?/>", String.Empty, RegexOptions.Multiline And RegexOptions.IgnoreCase)
                //input = Regex.Replace(input, "<input.*?src.*?=.*?[""|'](.*?)[""|'].*?/>", String.Empty, RegexOptions.Multiline And RegexOptions.IgnoreCase)
                return input;
            }

            input = Regex.Replace(input, "&nbsp;", string.Empty, RegexOptions.Multiline & RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "<p></p>", string.Empty, RegexOptions.Multiline & RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "<pre>|</pre>|<code>|</code>|<div>|</div>|<strong>|</strong>|<sup>|</sup>|<sub>|</sub>|<em>|</em>", string.Empty, RegexOptions.Multiline & RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "<br>|<br/>|<br />|</p>|< /p>|<p>", System.Environment.NewLine, RegexOptions.Multiline & RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "(<style(.*)/style>)", string.Empty, RegexOptions.Multiline);
            input = Regex.Replace(input, "(<script(.*)/script>)", string.Empty, RegexOptions.Multiline);
            input = Regex.Replace(input, "<table>|</table>|<td>|</td>|<tr>|</tr>|<tbody>|</tbody>", string.Empty, RegexOptions.Multiline);
            input = Regex.Replace(input, "<table(.*)>", string.Empty, RegexOptions.Multiline);
            input = Regex.Replace(input, "<tr(.*)>", string.Empty, RegexOptions.Multiline);
            input = Regex.Replace(input, "<td(.*)>", string.Empty, RegexOptions.Multiline);
            return input;

        }

        public static bool IsEmailValid(string email)
        {
            //Dim expression As Regex = New Regex("^[a-z0-9]+([-+\.]*[a-z0-9]+)*@[a-z0-9]+([-\.][a-z0-9]+)*$", RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.Multiline)
            //[RegularExpression("^[a-zA-Z0-9_\\+-]+(\\.[a-zA-Z0-9_\\+-]+)*@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9]+)*\\.([a-zA-Z]{2,4})$", ErrorMessage = "Email is invalid")]
            Regex expression = new Regex("^[a-zA-Z][\\w\\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\\w\\.-]*[a-zA-Z0-9]\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z]$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
            return expression.IsMatch(email);
        }
        public static bool IsDigitOnly(string data)
        {
            //Dim expression As Regex = New Regex("^[a-z0-9]+([-+\.]*[a-z0-9]+)*@[a-z0-9]+([-\.][a-z0-9]+)*$", RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.Multiline)
            //[RegularExpression("^[a-zA-Z0-9_\\+-]+(\\.[a-zA-Z0-9_\\+-]+)*@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9]+)*\\.([a-zA-Z]{2,4})$", ErrorMessage = "Email is invalid")]
            //Regex expression = new Regex("^[\\+][0-9]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
            Regex expression = new Regex("^\\+{0,1}[0-9]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
            return expression.IsMatch(data);
        }
        
        public static string ReadFile(string filename)
        {


            if (System.IO.File.Exists(filename))
            {
                System.IO.FileStream fs = null;
                System.IO.StreamReader sr = null;
                try
                {
                    fs = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                    sr = new System.IO.StreamReader(fs);
                    return sr.ReadToEnd();
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }

            }
            return string.Empty;
        }


        public static Guid GetNextGuid()
        {
            byte[] b = Guid.NewGuid().ToByteArray();
            DateTime dateTime = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
            TimeSpan timeOfDay = now.TimeOfDay;
            byte[] bytes1 = BitConverter.GetBytes(timeSpan.Days);
            byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
            Array.Reverse(bytes1);
            Array.Reverse(bytes2);
            Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
            Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
            return new Guid(b);
        }

        public static string GetCompanyName()
        {
            var s = ConfigurationManager.AppSettings["CompanyName"] ?? "First Bank of Nigeria";
            return s;
        }

        public static string GetSortBy(string sortBy, string defaultSortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return defaultSortBy;
            }
            else
            {
                if (Helper.IsItemExistInList(new string[] { Constants.SortField.DateCreated.ToLower(), Constants.SortField.PageTitle.ToLower(), 
                    Constants.SortField.Username.ToLower(), Constants.SortField.Email.ToLower(), Constants.SortField.FirstName.ToLower(), 
                    Constants.SortField.LastName.ToLower(), Constants.SortField.Role.ToLower(), Constants.SortField.Name.ToLower(),
                    Constants.SortField.StartDate.ToLower(), Constants.SortField.Title.ToLower(), Constants.SortField.Name.ToLower()
                },
                    sortBy.ToLower()))
                {
                    return sortBy;
                }
                else
                {
                    return defaultSortBy;
                }
            }
        }

        public static string GetSearchText(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return string.Empty;
            }
            else
            {
                return searchText;
            }
        }

        public static string GetSortDirection(string sortDirection, string defaultDirection)
        {
            if (string.IsNullOrEmpty(sortDirection))
            {
                return defaultDirection;
            }
            else
            {
                if (Helper.IsItemExistInList(new string[] { Constants.SortOrder.Ascending.ToLower(), Constants.SortOrder.Descending.ToLower() }, sortDirection.ToLower()))
                {
                    return sortDirection;
                }
                else
                {
                    return defaultDirection;
                }
            }
        }

        public static int GetPageSize(string pageSize)
        {
            if (string.IsNullOrEmpty(pageSize))
            {
                return 20;
            }
            else
            {
                int outValue = 0;
                if (int.TryParse(pageSize, out outValue))
                {
                    return outValue > 100 ? 100 : outValue;
                }
                else
                {
                    return 20;
                }
            }
        }
        public static int GetPageNumber(string pageNumber)
        {
            if (string.IsNullOrEmpty(pageNumber))
            {
                return 1;
            }
            else
            {
                int outValue = 0;
                if (int.TryParse(pageNumber, out outValue))
                {
                    return outValue;
                }
                else
                {
                    return 1;
                }
            }
        }

        public static string NumberToWordsTotal(double number)
        {
            long n = (long)number;
            long kobo = (long)Math.Round((number - n) * 100, 2);
            string words = NumberToWords(n) + " Naira " + NumberToWords(kobo).Trim() + " Kobo"; ;
            return words;
        }

        private static string NumberToWords(long number)
        {
            if (number == 0)
                return " Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000000000) > 0)
            {
                words += NumberToWords(number / 1000000000000) + " Trillion ";
                number %= 1000000000000;
            }

            if ((number / 1000000000) > 0)
            {
                words += NumberToWords(number / 1000000000) + " Billion ";
                number %= 1000000000;
            }

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        public static string MassageXml(string result, string typeName, string documentType)
        {
            var nodeName = "document";
            var replaceFormat = "<{0} type=\"{1}\"";
            var replaceFormat2 = "<{0}";

            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
            result = result.Replace(string.Format(replaceFormat2, typeName), string.Format(replaceFormat, nodeName, documentType));

            var index = result.IndexOf(">");
            result = result.Insert(index + 1, string.Format("<{0}>", typeName));
            result = string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?>{0}{1}", result, string.Format("</{0}>", nodeName));

            return result;
        }


        //public static bool IsDuplicateUsernamePermitted()
        //{
        //    var s = ConfigurationManager.AppSettings["PermitDuplicateUsername"];
        //    return !string.IsNullOrEmpty(s) && s == "true";
        //}

        public static bool IsTest()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            var s = ConfigurationManager.AppSettings["OverrideIsTest"];
            var IsTest = string.IsNullOrEmpty(s) ? false : s.Equals("true") ? true : false;
            return ((localIP.StartsWith("172.16.248.") || localIP.StartsWith("172.16.249.")) && IsTest);
        }

        public static string GetCacheKey(string modelType, string modelID, string loggedInUser)
        {
            return string.Format("{0}-{1}-{2}", modelType, modelID, loggedInUser);
        }

        public static string GetCurrentURL()
        {
            var rootURL = Helper.GetRootURL(true);
            return string.Format("{0}{1}", rootURL.EndsWith("/") ? 
                rootURL.Remove(rootURL.Length - 1) : rootURL, HttpContext.Current.Request.RawUrl);
        }

        public static string GetLastApprovalStatus(HttpRequestBase httpRequestBase)
        {
            string status = httpRequestBase.Form["LastApprovalStatus"];
            return status;
        }

        public static string GetCancellationUrl(string data)
        {
            var encrypted = Crypto.Encrypt(data);
            return Base64Encode(encrypted);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool IsSendApprovalNotificationMail()
        {
            var s = ConfigurationManager.AppSettings["SendApprovalNotificationMail"];
            return !string.IsNullOrEmpty(s) && s == "true";
        }
        public static bool RedirectToSSL()
        {
            var s = ConfigurationManager.AppSettings["RedirectToSSL"];
            return !string.IsNullOrEmpty(s) && s == "true";
        }
        public static string GetLastApprovalComment()
        {
            if (HttpContext.Current == null) return null;

            var httpRequestBase = HttpContext.Current.Request;
            var comment = httpRequestBase.Form["ApproverComment"];
            return comment;          
        }

        public static string GetLoggedInUserSolID()
        {
            var c = HttpContext.Current;
            if (c == null) return string.Empty;
            if (c.User.Identity as Identity != null)
                return ((Identity)c.User.Identity).BranchCode;
            else
                return string.Empty;
        }
    }
}
