using System.IO.Compression;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PermissionManagement.Web
{
    public class CompressFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

            HttpRequestBase request = filterContext.HttpContext.Request;
            string acceptEncoding = request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(acceptEncoding)) return;
            acceptEncoding = acceptEncoding.ToUpperInvariant();
            HttpResponseBase response = filterContext.HttpContext.Response;
            if (response.ContentType != "application/ms-excel")
            {
                if (acceptEncoding.Contains("GZIP"))
                {
                    response.AppendHeader("Content-encoding", "gzip");
                    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                }
                else if (acceptEncoding.Contains("DEFLATE"))
                {
                    response.AppendHeader("Content-encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                } 
            }
        }
    }

    public class LogFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var response = HttpContext.Current.Response;
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.End();

            // Prevent the action from actually being executed

            filterContext.Result = new EmptyResult();
            // Now What?
            base.OnActionExecuting(filterContext);

        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var response = HttpContext.Current.Response;
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.End();

            // Prevent the action from actually being executed

            filterContext.Result = new EmptyResult();
            // Now What?
            base.OnActionExecuted(filterContext);

        }
    }
}