using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace PermissionManagement.Utility
{


    #region "MailSettingsSectionHandler"

    /// <summary>
    /// This class implements IConfigurationSectionHandler and allows use of user-defined XML 
    /// nodes inside the Web.Config file.  Specfically, it allows the appSettings section
    /// to be read.
    
    public class MailSettingsSectionHandler : IConfigurationSectionHandler
    {

        #region "IConfigurationSectionHandler Members"

        /// <summary>
        /// Create a new BaseSettings object populates with data taken from the "mailSettings" node 
        /// in the config file.
        ///	</summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section">The XML section we will iterate against</param>
        /// <returns></returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            MailSettings r = new MailSettings(section);
            return r;
        }

        #endregion

    }
}
namespace PermissionManagement.Utility
{

    #endregion

    #region "mailsettings"

    /// <summary>
    /// A class that encapsulates the data from the appSettings section in the config file.
    /// </summary>
  
    public sealed class MailSettings
    {


        public const string ConfigSectionName = "mailSettings";
        #region "getcurrent"

        /// <summary>
        /// A factory method used to create an instance of the settings.
        /// </summary>
        /// <returns></returns>
        /// 

        public static MailSettings GetCurrent()
        {
            var settings = (MailSettings)ConfigurationManager.GetSection(MailSettings.ConfigSectionName);
            if (settings == null)
            {
                throw new Exception("Config file does not appear to have an entry for the mailSettings node");
            }
            return settings;
        }

        #endregion

        #region "crypto key"

        /// <summary>
        /// The key for the encyption and decryption
        /// </summary>

        public static byte[] CryptoKey = System.Text.Encoding.UTF8.GetBytes("Bu$hm1ll$");
        #endregion

        #region "properties"

        /// <summary>Mail Server name/ip</summary>
        public string Server
        {
            get { return m_Server; }
            private set { m_Server = value; }
        }

        private string m_Server;
        /// <summary>Mail Server port number</summary>
        public int Port
        {
            get { return m_Port; }
            private set { m_Port = value; }
        }

        private int m_Port;
        /// <summary>Generic Mail From Address Name Displayed</summary>
        public string FromName
        {
            get { return m_FromName; }
            private set { m_FromName = value; }
        }

        private string m_FromName;
        /// <summary>Generic Mail From Address</summary>
        public string From
        {
            get { return m_From; }
            private set { m_From = value; }
        }

        private string m_From;
        /// <summary>Mail Server Login Name</summary>
        public string Login
        {
            get { return m_Login; }
            private set { m_Login = value; }
        }

        private string m_Login;
        /// <summary>Mail Server Password</summary>
        public string Password
        {
            get { return m_Password; }
            private set { m_Password = value; }
        }

        private string m_Password;
        /// <summary>Determines if password is encrypted, so need to be decrypted.</summary>
        public bool IsEncrypted
        {
            get { return m_IsEncrypted; }
            private set { m_IsEncrypted = value; }
        }

        private bool m_IsEncrypted;
        /// <summary>Indicates whether emais are sent or not.</summary>
        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            private set { m_IsEnabled = value; }
        }

        private bool m_IsEnabled;
        /// <summary>Global BCC Address</summary>
        public string GlobalBcc
        {
            get { return m_GlobalBcc; }
            private set { m_GlobalBcc = value; }
        }

        private string m_GlobalBcc;
        /// <summary>Determines if mail is send directly or send to queue</summary>
        public bool SendToQueue
        {
            get { return m_SendToQueue; }
            private set { m_SendToQueue = value; }
        }

        private bool m_SendToQueue;
        /// <summary>Determines if ssl is to be enabled</summary>
        public bool UseSsl
        {
            get { return m_UseSsl; }
            private set { m_UseSsl = value; }
        }

        private bool m_UseSsl;

        private IDictionary<string, ContactSetting> _contacts = new Dictionary<string, ContactSetting>();
        /// <summary>Holds the collection of contact settings</summary>        
        public IDictionary<string, ContactSetting> Contacts
        {
            get { return m_Contacts; }
            private set { m_Contacts = value; }
        }

        private IDictionary<string, ContactSetting> m_Contacts;
        /// <summary>Sms Gateway Sendto Email Address</summary>
        public string SmsGatewayEmail
        {
            get { return m_SmsGatewayEmail; }
            private set { m_SmsGatewayEmail = value; }
        }

        private string m_SmsGatewayEmail;
        /// <summary>Sms Gateway Account Email Address</summary>
        public string SmsGatewayAccountEmail
        {
            get { return m_SmsGatewayAccountEmail; }
            private set { m_SmsGatewayAccountEmail = value; }
        }

        private string m_SmsGatewayAccountEmail;
        /// <summary>Sms Gateway Sub Account</summary>
        public string SmsGatewaySubAccount
        {
            get { return m_SmsGatewaySubAccount; }
            private set { m_SmsGatewaySubAccount = value; }
        }

        private string m_SmsGatewaySubAccount;
        /// <summary>Sms Gateway Sub Account Password</summary>
        public string SmsGatewaySubAccountPwd
        {
            get { return m_SmsGatewaySubAccountPwd; }
            private set { m_SmsGatewaySubAccountPwd = value; }
        }

        private string m_SmsGatewaySubAccountPwd;
        /// <summary>Sms Gateway Sub Account Password</summary>
        public decimal SmsUnitPrice
        {
            get { return m_SmsUnitPrice; }
            private set { m_SmsUnitPrice = value; }
        }

        private decimal m_SmsUnitPrice;
        #endregion

        #region "constructor"
        /// <summary>
        /// Simple constructor
        /// </summary>
        public MailSettings()
        {
        }

        /// <summary>
        /// Overloaded constructor that is given the Settings node from the Config file and 
        /// populates the settings object.
        /// </summary>
        /// <param name="node"></param>
        public MailSettings(XmlNode node)
        {
            string message = "'{0}' attribute not found in MailSettings section of Web.config";

            // try to find a node with the name of the server.
            var serverName = Environment.MachineName.ToLower();
            var serverNode = node.SelectSingleNode(serverName);

            // if no server node exists, look for a "default" node.
            if (serverNode == null)
            {
                serverNode = node.SelectSingleNode("default");
            }

            var mailNode = serverNode.SelectSingleNode("mail");
            this.Server = mailNode.Attributes["server"].Value;
            this.From = mailNode.Attributes["from"].Value;
            this.Login = mailNode.Attributes["login"] == null ? string.Empty : mailNode.Attributes["login"].Value;
            this.Port = mailNode.Attributes["port"] == null ? 25 : int.Parse(mailNode.Attributes["port"].Value);
            this.UseSsl = mailNode.Attributes["usessl"] == null ? false : mailNode.Attributes["usessl"].Value.ToLower() == "true";
            var encrypted = mailNode.Attributes["isencrypted"].Value;
            this.IsEncrypted = string.IsNullOrEmpty(encrypted) ? false : bool.Parse(encrypted);
            this.Password = mailNode.Attributes["password"] == null ? string.Empty : mailNode.Attributes["password"].Value;
            this.GlobalBcc = mailNode.Attributes["globalbcc"] == null ? string.Empty : mailNode.Attributes["globalbcc"].Value;
            this.IsEnabled = mailNode.Attributes["enabled"] == null ? false : mailNode.Attributes["enabled"].Value.ToLower() == "true";
            this.SendToQueue = mailNode.Attributes["sendtoqueue"] == null ? false : mailNode.Attributes["sendtoqueue"].Value.ToLower() == "true";
            this.FromName = mailNode.Attributes["fromname"] == null ? string.Empty : mailNode.Attributes["fromname"].Value;

            var smsNode = serverNode.SelectSingleNode("emailtosmsgateway");
            if (smsNode != null)
            {
                this.SmsGatewayEmail = smsNode.Attributes["gatewayemail"].Value;
                this.SmsGatewayAccountEmail = smsNode.Attributes["accountemail"].Value;
                this.SmsGatewaySubAccount = smsNode.Attributes["subacct"].Value;
                this.SmsGatewaySubAccountPwd = smsNode.Attributes["subacctpwd"].Value;
                this.SmsUnitPrice = smsNode.Attributes["smsunitprice"] == null ? 0 : decimal.Parse(smsNode.Attributes["smsunitprice"].Value);
            }

            if (IsEncrypted)
            {
                Password = Decrypt(Password);
                this.SmsGatewaySubAccountPwd = Decrypt(this.SmsGatewaySubAccountPwd);
            }

            // extract the contacts node
            var contactsNode = serverNode.SelectSingleNode("contacts");

            foreach (XmlNode contactNode in contactsNode.ChildNodes)
            {
                foreach (string item in new string[] {
					"name",
					"email",
					"displayname"
				})
                {
                    if (contactNode.Attributes[item] == null || string.IsNullOrEmpty(contactNode.Attributes[item].Value.Trim()))
                    {
                        throw new Exception(string.Format(message, item));
                    }
                }
                _contacts.Add(contactNode.Attributes["name"].Value, new ContactSetting(contactNode.Attributes["name"].Value, contactNode.Attributes["email"].Value, contactNode.Attributes["displayname"].Value));
            }

            Contacts = _contacts;
        }

        private string Decrypt(string password)
        {
            return Crypto.Decrypt(password, CryptoKey);
        }


        #endregion

    }
}
namespace PermissionManagement.Utility
{

    #region "ContactSetting"

    /// <summary>
    /// encapsulates each Contact settings.
    /// </summary>
    public class ContactSetting
    {

        #region "constructor"

        /// <summary>
        /// Constructs an instance with all fields supplied.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public ContactSetting(string name, string email, string displayName)
        {
            _email = email;
            _name = name;
            _displayName = displayName;
        }

        #endregion

        #region "properties"

        public string Email
        {
            get { return _email; }
            private set { _email = value; }
        }

        private string _email;
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        private string _name;
        public string DisplayName
        {
            get { return _displayName; }
            private set { _displayName = value; }
        }

        private string _displayName;
        #endregion

    }
}

    #endregion

    #endregion