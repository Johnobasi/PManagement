using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
   public class ExceptionLog
    {
       [IdentityPrimaryKey()]

       public long ExceptionId { get; set; }
       public System.DateTime ExceptionDateTime { get; set; }
       public string ExceptionDetails { get; set; }
       public string ExceptionPage { get; set; }
       public string LoggedInUser { get; set; }
       public string UserIPAddress { get; set; }
       public string ExceptionType { get; set; }
       public string ExceptionMessage { get; set; }
       public string ExceptionVersion { get; set; }
    }
}
