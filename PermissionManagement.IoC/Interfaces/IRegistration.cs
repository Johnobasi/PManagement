using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.IoC
{
    public interface IRegistration
    {
        string Name     { get; }
        string Key      { get; }
        Type ResolvesTo { get; }

        IRegistration WithLifetimeManager(ILifetimeManager manager);

        void InvalidateInstanceCache();
    }
}
