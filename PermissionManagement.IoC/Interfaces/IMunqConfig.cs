using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.IoC
{
    public interface IMunqConfig
    {
        ///<summary>
        /// Classes that implement this interface are automatically called to
        /// register type factories in the Munq IOC container
        /// </summary>
        /// <param name="container">The Munq Container.</param>

        void RegisterIn(IContainer container);
    }
}
