using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.IoC
{
   public interface IInstanceCreator
    {
        string Key { get; }
        object CreateInstance(ContainerCaching containerCache);
    }

    public enum ContainerCaching
    { 
        InstanceCachedInContainer,
        InstanceNotCachedInContainer
    }
}
