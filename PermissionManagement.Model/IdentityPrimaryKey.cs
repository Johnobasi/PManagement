using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IdentityPrimaryKeyAttribute : Attribute
    {

    }
}
