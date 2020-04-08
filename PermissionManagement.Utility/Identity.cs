using System;
using System.Security.Principal;

namespace PermissionManagement.Utility
{
    [Serializable()]
    public class Identity : IIdentity
    {
        #region "constructors"

        /// <summary>
        /// Default constructor for an Identity.
        /// </summary>
        public Identity()
        {
        }

        /// <summary>
        /// A constructor that provides means to initialize fields.
        /// </summary>
        /// <param name="id">Session Id</param>
        /// <param name="roles"></param>
        public Identity(Guid id, string name, string roles, string fullName, string branchCode, string accountType)
        {
            m_Name = name;
            this.Roles = roles;
            this.SessionUid = id;
            m_IsAuthenticated = true;
            FullName = fullName;
            BranchCode = branchCode;
            AccountType = accountType;
            //m_StaffID = StaffID;
            //Me.PersonName = personName
            //Me.EmailAddress = emailAddress
        }

        #endregion

        #region "IIdentity properties"

        /// <summary>
        /// Gets the Authentication Type of this identity.
        /// </summary>
        public string AuthenticationType
        {
            get { return m_AuthenticationType; }
        }

        private string m_AuthenticationType = string.Empty;
        /// <summary>
        /// Indicates whether the User is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get { return m_IsAuthenticated; }
        }

        private bool m_IsAuthenticated;
        /// <summary>
        /// Gets or sets the Account Name of the user.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        private string m_Name;
        /// <summary>
        /// Gets or sets the StaffID of the user.
        /// </summary>
        //public string StaffID
        //{
        //    get { return m_StaffID; }
        //}

        //private string m_StaffID;
        #endregion

        #region "extra properties"

        /// <summary>
        /// The session uid assigned to this used by the security system.
        /// </summary>
        public Guid SessionUid
        {
            get { return m_SessionUid; }
            set { m_SessionUid = value; }
        }

        private Guid m_SessionUid;
        /// <summary>
        /// Gets or sets the Roles of the User
        /// </summary>
        public string Roles
        {
            get { return m_Roles; }
            set { m_Roles = value; }
        }

        private string m_Roles;

        public string FullName { get; set; }
        public string BranchCode { get; set; }

        private string m_AccountType;
        public string AccountType
        {
            get { return m_AccountType; }
            set { m_AccountType = value; }
        }
        #endregion
    }
}
