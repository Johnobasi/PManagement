using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Configuration;
using System.Xml;
using System.Text;
using System.Xml.XPath;
namespace PermissionManagement.Utility
{

    #region "SecurityConfigHandler"

    /// <summary>
    /// This class implements IConfigurationSectionHandler and allows use of user-defined XML 
    /// nodes inside the Web.Config file.  Specfically, it allows the appSettings section
    /// to be read.
    /// </summary>
    /// <history>
    ///		<change author="Gboyega Suleman" date="2010-10-04">Original Version</change>
    /// </history>
    public class SecurityConfigHandler : IConfigurationSectionHandler
    {

        #region "IConfigurationSectionHandler Members"

        /// <summary>
        /// Create a new BaseSettings object populates with data taken from the "securitySettings" node 
        /// in the config file.
        ///	</summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section">The XML section we will iterate against</param>
        /// <returns></returns>
        /// 

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            SecurityConfig r = new SecurityConfig(section);
            return r;
        }

        #endregion

    }
}
namespace PermissionManagement.Utility
{

    #endregion

    /// <summary>
    /// A class that encapsulates the data from the securitySettings section in the config file.
    /// </summary>
    /// <history>
    ///		<change author="Gboyega Suleman" date="2010-10-04">Original Version</change>
    /// </history>
    /// 

    public sealed class SecurityConfig : IDisposable
    {
        // Name of the config file custom section from which this class is hydrated.

        public const string ConfigSectionName = "securitySettings";
        #region "getcurrent"

        /// <summary>
        /// A factory method used to create an instance of the settings.
        /// </summary>
        /// <returns></returns>
        /// 

        //public static SecurityConfig GetCurrent()
        //{
        //    var settings = (SecurityConfig)ConfigurationManager.GetSection(SecurityConfig.ConfigSectionName);
        //    if (settings == null)
        //    {
        //        throw new Exception("Config file does not appear to have an entry for the securitySettings node");
        //    }
        //    return settings;
        //}

        public static SecurityConfig GetCurrent()
        {
            var settings = HttpContext.Current.Cache["securitySettings"] as SecurityConfig;
            if (settings != null) return settings;
            settings = (SecurityConfig)ConfigurationManager.GetSection(SecurityConfig.ConfigSectionName);
            if (settings == null)
            {
                throw new Exception("Config file does not appear to have an entry for the securitySettings node");
            }
            HttpContext.Current.Cache.Insert("securitySettings", settings, null, DateTime.UtcNow.AddSeconds(20 * 60), System.Web.Caching.Cache.NoSlidingExpiration);
            return settings;
        }


        #endregion

        #region "properties"

        /// <summary>
        /// Details of the login page used to login to the application.
        /// </summary>
        public LoginSettings Login
        {
            get { return m_Login; }
            private set { m_Login = value; }
        }

        private LoginSettings m_Login;
        /// <summary>
        /// Details of the cookie used to transport the session data.
        /// </summary>
        public CookieSettings Cookie
        {
            get { return m_Cookie; }
            private set { m_Cookie = value; }
        }

        private CookieSettings m_Cookie;
        #endregion

        #region "constructor"

        /// <summary>
        /// Simple constructor
        /// </summary>
        public SecurityConfig()
        {
        }

        /// <summary>
        /// Overloaded constructor that is given the Settings node from the Config file and 
        /// populates the settings object.
        /// </summary>
        /// <param name="node"></param>
        public SecurityConfig(XmlNode node)
        {
            // try to find a node with the name of the server.
            var serverName = Environment.MachineName.ToLower();
            XmlNode serverNode = node.SelectSingleNode(serverName);

            // if no server node exists, look for a "default" node.
            if (serverNode == null)
            {
                serverNode = node.SelectSingleNode("default");
            }

            //#Region "cookie"

            XmlNode cookieNode = serverNode.SelectSingleNode("./cookie");

            if (cookieNode != null)
            {
                // if a cookie domain name exists (and is NOT localhost) use it.
                var domainName = cookieNode.Attributes["domain"] == null ? null : cookieNode.Attributes["domain"].Value.ToLower().Trim() == ".localhost" ? null : cookieNode.Attributes["domain"].Value;

                // if a cookie domain name exists (and is NOT localhost) use it.
                var timeout = cookieNode.Attributes["timeout"] == null ? 30 : int.Parse(cookieNode.Attributes["timeout"].Value);

                var slidingExpiration = cookieNode.Attributes["slidingexpiration"] == null ? false : cookieNode.Attributes["slidingexpiration"].Value == "true" ? true : false;

                var passwordHashed = cookieNode.Attributes["passwordhashed"] == null ? false : cookieNode.Attributes["passwordhashed"].Value == "true" ? true : false;

                var cookieOnlyCheck = cookieNode.Attributes["cookieonlycheck"] == null ? false : cookieNode.Attributes["cookieonlycheck"].Value == "true" ? true : false;
                var maximumPasswordRetries = cookieNode.Attributes["maximumpasswordretries"] == null ? 5 : int.Parse(cookieNode.Attributes["maximumpasswordretries"].Value);
                var enable2FA = cookieNode.Attributes["enable2fa"] == null ? true : cookieNode.Attributes["enable2fa"].Value == "true" ? true : false;
                var exemptlocaluserfrom2fa = cookieNode.Attributes["exemptlocaluserfrom2fa"] == null ? true : cookieNode.Attributes["exemptlocaluserfrom2fa"].Value == "true" ? true : false;

                this.Cookie = new CookieSettings(domainName, timeout.ToString(), slidingExpiration, passwordHashed, cookieOnlyCheck, maximumPasswordRetries, enable2FA, exemptlocaluserfrom2fa);
            }

            //#End Region

            //#Region "login"

            XmlNode loginNode = serverNode.SelectSingleNode("./login");

            if (loginNode != null)
            {
                var url = loginNode.Attributes["url"] == null ? null : loginNode.Attributes["url"].Value;

                var page = loginNode.Attributes["page"] == null ? null : loginNode.Attributes["page"].Value;

                this.Login = new LoginSettings(url, page);

                //#End Region

            }
        }

        #endregion

        #region "IDisposable"

        /// <summary>
        /// Provide a dispose method to fulfill the IDisposable interface.
        /// </summary>
        /// <remarks>
        /// The IDisposable interface is implemented to allow the use of the object
        /// in a using statement.
        /// </remarks>
        /// 

        public void Dispose()
        {
        }

        #endregion

        #region "CookieSettings"

        /// <summary>
        /// Describes the Cookie settings for the cookie used to transport the encrypted session uuid.
        /// </summary>
        public class CookieSettings
        {
            /// <summary>
            /// the default amount of time before the cookie expires.
            /// </summary>

            private int default_timeout = 30;
            #region "constructor"

            /// <summary>
            /// Constructs an instance with all fields supplied.
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="timeout"></param>
            public CookieSettings(string domain, string timeout, bool slidingExpiration, bool passwordHashed, bool cookieOnlyCheck, int maximumPasswordRetries, bool enable2FA, bool exemptlocalaccountfrom2fa)
            {
                this.DomainName = domain;
                this.Timeout = timeout == null ? default_timeout : int.Parse(timeout);
                this.SlidingExpiration = slidingExpiration;
                this.PasswordHashed = passwordHashed;
                this.CookieOnlyCheck = cookieOnlyCheck;
                this.MaximumPasswordRetries = maximumPasswordRetries;
                this.Enable2FA = enable2FA;
                this.ExemptLocalAccountFrom2FA = exemptlocalaccountfrom2fa;
            }

            #endregion

            #region "properties"

            private int m_maximumPasswordRetries;
            public int MaximumPasswordRetries
            {
                get { return m_maximumPasswordRetries; }
                set { m_maximumPasswordRetries = value; }
            }

            /// <summary>
            /// The name of the domain name to be used to transport the session cookie.
            /// </summary>
            public string DomainName
            {
                get { return m_DomainName; }
                private set { m_DomainName = value; }
            }

            private string m_DomainName;
            /// <summary>
            /// The number of minutes after which the session cookie expires.
            /// </summary>
            public int Timeout
            {
                get { return m_Timeout; }
                private set { m_Timeout = value; }
            }

            private int m_Timeout;
            /// <summary>
            /// Whether log in time out at exactly the timeout specified 
            /// or timeout only on inactivity for the timeout specified.
            /// </summary>
            public bool SlidingExpiration
            {
                get { return m_SlidingExpiration; }
                private set { m_SlidingExpiration = value; }
            }

            private bool m_SlidingExpiration;
            /// <summary>
            /// Whether the user password will hasshed or saved in plain text
            /// </summary>
            public bool PasswordHashed
            {
                get { return m_PasswordHashed; }
                private set { m_PasswordHashed = value; }
            }

            private bool m_PasswordHashed;
            /// <summary>
            /// Whether authentication is checked by using the cookie content
            /// only or checked against the data store for each request.
            /// </summary>
            public bool CookieOnlyCheck
            {
                get { return m_CookieOnlyCheck; }
                private set { m_CookieOnlyCheck = value; }
            }

            private bool m_CookieOnlyCheck;

            /// <summary>
            /// Whether authentication two factor authentication is enabled or not
            /// </summary>
            public bool Enable2FA
            {
                get { return m_enable2FA; }
                private set { m_enable2FA = value; }
            }

            private bool m_enable2FA;

            /// <summary>
            /// Whether Local(Application) role is used for authorization or not
            /// </summary>
            public bool ExemptLocalAccountFrom2FA
            {
                get { return m_ExemptLocalAccountFrom2FA; }
                private set { m_ExemptLocalAccountFrom2FA = value; }
            }

            private bool m_ExemptLocalAccountFrom2FA;

            #endregion
        }
        
        #endregion

        #region "LoginSettings"

        /// <summary>
        /// Describes which page is used to sign in into the application.
        /// </summary>
        public class LoginSettings
        {
            #region "constructor"

            /// <summary>
            /// Constructs an instance with all fields supplied.
            /// </summary>
            /// <param name="url"></param>
            /// <param name="page"></param>
            public LoginSettings(string url, string page)
            {
                this.Url = url;
                this.Page = page;
            }

            #endregion

            #region "properties"

            /// <summary>
            /// Url location of the page used as the login page.
            /// </summary>
            public string Url
            {
                get { return m_Url; }
                private set { m_Url = value; }
            }

            private string m_Url;
            /// <summary>
            /// Name of the page used to login into the application.
            /// </summary>
            public string Page
            {
                get { return m_Page; }
                private set { m_Page = value; }
            }

            private string m_Page;
            #endregion
        }

        #endregion

    }
}
