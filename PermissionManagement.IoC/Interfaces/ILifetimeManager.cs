using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.IoC
{
   public interface ILifetimeManager
    {
       object GetInstance(IInstanceCreator creator);
       void InvalidateInstanceCache(IRegistration registration);
    }
}
