using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PermissionManagement.Web
{
    public class UnknownRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            throw new HttpException(404, "Requested page not found.");
            return new MvcHandler(requestContext);
        }
    }

}