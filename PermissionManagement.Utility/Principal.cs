using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
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

        #endregion

        #region "constructor"

        public Principal(IIdentity id, string roleName)
        {
            _identity = id;
            _roleName = roleName;
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
