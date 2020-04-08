using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using System.Security.Cryptography;
using System.Reflection;

namespace PermissionManagement.Utility
{
        /// <summary>
        /// Describes the cookie used to pass parameters between pages.
        /// </summary>
        /// <remarks>
        /// This class wraps up the cookie handling for the authentication.  Create an instance
        /// to load the cookie, modify values, and .Save the cookie to persist.
        /// </remarks>
        public abstract class Cookie
        {

            #region "fields"

            /// <summary>
            /// Used as a salt during the encryption and decryption process.
            /// </summary>
            /// <remarks>
            /// Can be overridden in a child class to specify a more specific key
            /// </remarks>
            protected virtual byte[] CryptoKey()
            {
                return System.Text.Encoding.UTF8.GetBytes("R3dL10n$19");
            }

            /// <summary>
            /// The amount of time before the cookie expires.
            /// </summary>
            /// <remarks>
            /// This is a default value which ensures that the cookie is not stored.
            /// </remarks>
            protected virtual DateTime Expires()
            {
                return DateTime.MinValue;
            }

            /// <summary>
            /// The domain name to be used when storing the cookie value.
            /// </summary>
            [System.Xml.Serialization.XmlIgnore()]
            public virtual string Domain
            {
                get { return _domain; }
                set { _domain = value; }
            }

            private string _domain;
            /// <summary>
            /// Indicates the default encoding method used when storing cookie values.
            /// </summary>
            /// <remarks>
            /// When overridden in a sub-class allows other cookies to employ a different encoding method.
            /// </remarks>
            /// <returns>The cookie value encoding method.</returns>
            public virtual CookieValueEncoding Encoding()
            {
                return CookieValueEncoding.None;
            }

            public string CookieOwner
            {
                get { return _cookieOwner; }
                set { _cookieOwner = value; }
            }

            private string _cookieOwner;
            #endregion

            #region "Constructor"

            /// <summary>
            /// Create an instance of the cookie and load data from the cookie into the object properties.
            /// </summary>
            public Cookie()
            {
                Load();
            }

            #endregion

            #region "Methods"

            /// <summary>
            /// Remove cookie from the response
            /// </summary>
            public void Delete()
            {
                var cookieName = this.GetType().FullName;

                var current = System.Web.HttpContext.Current;
                if (current != null)
                {
                    current.Response.Cookies[cookieName].Value = "";
                    current.Response.Cookies[cookieName].Expires = Helper.GetLocalDate().AddDays(-1);
                }
            }

            /// <summary>
            /// Persist the details to a cookie.
            /// </summary>
            /// <remarks>
            /// Each time the cookie changes format we should increment the version number to ensure we don't crash with invalid values.
            /// </remarks>
            public void Save()
            {
                this.CookieOwner = System.Web.HttpContext.Current.User.Identity.Name;

                // The name of the cookie comes from the full name of the cookie sub-class.
                var cookieName = this.GetType().FullName;

                // reduce properties on cookie sub-class to an xml string and encrypt using supplied key.
                string fields = Dehydrate();
                fields = Crypto.Encrypt(fields, CryptoKey());

                // create a cookie with the payload.
                var cookie = new HttpCookie(cookieName);
                cookie.HttpOnly = true;

                // if necessary, set the expiry time.
                var expiryDate = Expires();
                if (expiryDate != DateTime.MinValue)
                {
                    cookie.Expires = expiryDate;
                }

                cookie.Path = "/";

                // encode value using specified encoding method.
                switch (Encoding())
                {
                    case CookieValueEncoding.UrlEncoding:
                        cookie.Value = System.Web.HttpContext.Current.Server.UrlEncode(fields);
                        break; // TODO: might not be correct. Was : Exit Select
                    case CookieValueEncoding.Base64Encoding:
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(fields);
                        cookie.Value = Convert.ToBase64String(bytes);
                        break; // TODO: might not be correct. Was : Exit Select
                    default:
                        cookie.Value = fields;
                        break; // TODO: might not be correct. Was : Exit Select
                }

                // if a domain name has been supplied (and is not localhost) assign it to the cookie.
                if (this.Domain != null && this.Domain.ToLower() != ".localhost")
                {
                    cookie.Domain = this.Domain;
                }

                // store the cookie in the response for transmission to the client.
                System.Web.HttpContext.Current.Response.Cookies.Set(cookie);

            }

            /// <summary>
            /// Retrieve details from cookie.
            /// </summary>
            public void Load()
            {
                var cookieName = this.GetType().FullName;

                // Check if there is a cookie stored.
                HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies[cookieName];
                if (cookie != null && cookie.Value.Length != 0)
                {
                    string value = cookie.Value;

                    // decode value using specified decoding method.
                    switch (Encoding())
                    {
                        case CookieValueEncoding.UrlEncoding:
                            value = System.Web.HttpContext.Current.Server.UrlDecode(cookie.Value);
                            break; // TODO: might not be correct. Was : Exit Select
                        case CookieValueEncoding.Base64Encoding:
                            byte[] bytes = Convert.FromBase64String(value);
                            value = System.Text.Encoding.UTF8.GetString(bytes);
                            break; // TODO: might not be correct. Was : Exit Select
                    }

                    // decrypt cookie value using specified crypt key.
                    value = Crypto.Decrypt(value, CryptoKey());

                    // repopulate class properties from xml in cookie value.
                    Rehydrate(value);

                }
            }

            #endregion

            #region "storage"

            /// <summary>
            /// Dehydrate properties of cookie object and store as xml in a string.  We will store the data in the browser cookie.
            /// </summary>
            protected virtual string Dehydrate()
            {
                var sb = new StringBuilder();
                var pis = this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                for (int i = 0; i <= pis.Length - 1; i++)
                {
                    var pi = (System.Reflection.PropertyInfo)pis.GetValue(i);
                    var value = pi.GetValue(this, new object[] { });

                    //if (pi.Attributes["System.Xml.Serialization.XmlIgnore"] == null)
                    if (true)
                    {
                        sb.AppendFormat(" {0}=\"{1}\"", pi.Name, value == null ? "null" : value.ToString().Replace("\"", "~"));
                    }
                }
                string text = string.Format("<{0}{1} />", this.GetType().Name, sb);

                return text;
            }

            /// <summary>
            /// Rehydrate cookie properties, initializing each property from an attribute in the xml data.
            /// </summary>
            protected virtual void Rehydrate(string data)
            {
                var dom = new System.Xml.XmlDocument();
                dom.LoadXml(data);

                foreach (System.Xml.XmlAttribute attribute in dom.DocumentElement.Attributes)
                {
                    // get class property to which we will assign a value.
                    var cookieProperty = this.GetType().GetProperty(attribute.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    // get the value to assign, and convert to the correct type from string.
                    var value = attribute.Value == "null" ? null : Convert.ChangeType(attribute.Value, cookieProperty.PropertyType);

                    // assign the value to the property.
                    cookieProperty.SetValue(this, value, null);
                }
            }

            #endregion

        }

        /// <summary>
        /// The type of method used to encoding the data in the cookie.
        /// </summary>
        public enum CookieValueEncoding
        {
            None,
            UrlEncoding,
            Base64Encoding
        }
}
