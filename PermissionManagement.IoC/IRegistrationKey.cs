using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.IoC
{
      public interface IRegistrationKey
    {
        Type GetInstanceType();
        bool Equals(object obj);
        int GetHashCode();
    }

}
