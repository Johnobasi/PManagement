using System;
using System.Security.Principal;

namespace PermissionManagement.Utility
{
    [Serializable()]
    public class Principal : MarshalByRefObject, IPrincipal
    {

        #region "properties"

        private string _roleName;

        private IIdentity _identity;
        public IIdentity Identity
        {
            get { return _identity; }
        }

        private string _accountType;
        public string AccountType { get { return _accountType; } }

        #endregion

        #region "constructor"

        public Principal(IIdentity id, string roleName, string accountType)
        {
            _identity = id;
            _roleName = roleName;
            _accountType = accountType;
        }

        #endregion

        #region "Methods"

        public bool IsInRole(string role)
        {
            return role == _roleName;
        }

        #endregion

    }
}
