using System;
using System.Text;
using System.Web;
using System.Configuration;
using System.Security.Cryptography;
using System.Reflection;

namespace PermissionManagement.Utility
{
    public class AuthCookie : Cookie
    {

        #region "properties"

        /// <summary>
        /// UUID of the session id assigned to this 
        /// </summary>
        public string SessionUid
        {
            get { return m_SessionUid; }
            set { m_SessionUid = value; }
        }

        private string m_SessionUid;
        /// <summary>
        /// Username who owns the cookie
        /// </summary>
        public string Username
        {
            get { return m_Username; }
            set { m_Username = value; }
        }

        private string m_Username;
        /// <summary>
        /// User Roles, set only if cookieonly check is true
        /// </summary>
        public string UserRoles
        {
            get { return m_UserRoles; }
            set { m_UserRoles = value; }
        }

        private string m_UserRoles;

        /// <summary>
        /// User BranchCode, set only if cookieonly check is true
        /// </summary>
        public string BranchCode
        {
            get { return m_BranchCode; }
            set { m_BranchCode = value; }
        }

        private string m_BranchCode;

        /// <summary>
        /// Supply the domain namne for the cookie, and get this from the config file.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public override string Domain
        {
            get
            {
                var settings = SecurityConfig.GetCurrent();   //(SecurityConfig)System.Configuration.ConfigurationManager.GetSection(SecurityConfig.ConfigSectionName);
                return settings.Cookie.DomainName;
            }
            set { base.Domain = value; }
        }

        private System.DateTime _authExpiry;
        /// <summary>
        /// The amount of time before the cookie expires.
        /// </summary>
        /// <remarks>
        /// This will be set according to the timeout in the config file if
        /// cookieonlycheck is enabled
        /// </remarks>
        public System.DateTime AuthExpiry
        {
            get { return _authExpiry; }
            set { _authExpiry = value; }
        }

        #endregion

        #region "overrides"

        /// <summary>
        /// Salt String used in the encryption.
        /// </summary>
        /// <returns></returns>
        protected override byte[] CryptoKey()
        {
            return System.Text.Encoding.UTF8.GetBytes("00TnHLx+pU!efZ*P*xlF]f&{Fz09hkkJ");
        }

        #endregion

        #region "GetCurrent"

        /// <summary>
        /// Create an instance of the Authentication Cookie and return to invoking method.
        /// </summary>
        /// <returns></returns>
        public static AuthCookie GetCurrent()
        {
            var settings = SecurityConfig.GetCurrent();
            var cookie = new AuthCookie();
            cookie.Domain = settings.Cookie.DomainName;
            return cookie;
        }

        #endregion

    }
}
