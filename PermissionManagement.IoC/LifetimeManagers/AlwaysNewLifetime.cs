using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.IoC
{
    public class AlwaysNewLifetime : ILifetimeManager
    {
        #region ILifetimeManager Members

        /// <summary>
        /// Gets a new instance.
        /// </summary>
        /// <param name="creator">The creator (registration) that can create an instance</param>
        /// <returns>The new instance.</returns>
        public object GetInstance(IInstanceCreator creator)
        {
            return creator.CreateInstance(ContainerCaching.InstanceNotCachedInContainer);
        }

        /// <summary>
        /// Invalidates any cached instances.
        /// </summary>
        /// <param name="registration">The registration.</param>
        public void InvalidateInstanceCache(IRegistration registration)
        {
            // there is no instance cache ...
        }
        #endregion
    }
}
