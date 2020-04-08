using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
   public  class AuthenticationDataDto
    {
       private string _sessionId;
       public string SessionId
       {
           get { return _sessionId; }
           set { _sessionId = value; }
       }

       private string _username;

       public string Username
       {
           get { return _username; }
           set { _username = value; }
       }

       private string _roles;

       public string Roles
       {
           get { return _roles; }
           set { _roles = value; }
       }

       public string FullName { get; set; }
       public bool IsFirstLogIn { get; set; }

       public bool IsPasswordSet { get; set; }

       public bool IsRoleSet { get; set; }

       public bool AppAuthenticationFailed { get; set; }

       public string BranchCode { get; set; }
    }
}
