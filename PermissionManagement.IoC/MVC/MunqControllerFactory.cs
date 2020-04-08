using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Web.Mvc;
using System.Web.Routing;
using System.Web;
using System.Net;
using System.Web.SessionState;
using System.Web.Mvc;

namespace PermissionManagement.IoC
{
      public class MunqControllerFactory : IControllerFactory
    {
        public IContainer IOC { get; private set; }

        public MunqControllerFactory(IContainer container)
        {
            IOC = container;
        }

        #region IControllerFactory Members

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            try
            {
                return IOC.Resolve<IController>(controllerName);
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                throw new HttpException(404, ex.Message);
            }
            catch { return null; }
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.Disabled;
        }

        public void ReleaseController(IController controller)
        {
            var disposable = controller as IDisposable;

            if (disposable != null)
            {
                foreach (var s in HttpContext.Current.Items.Values)
                {
                    var s2 = s as IDisposable;
                    if (s2 != null) { s2.Dispose(); }
                }
                disposable.Dispose();
            }
        }

        #endregion
    }

}
