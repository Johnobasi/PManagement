//using PermissionManagement.IoC;
using PermissionManagement.Model;
using PermissionManagement.Repository;
using PermissionManagement.Services;
using PermissionManagement.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DryIoc;
using DryIoc.Mvc;

namespace PermissionManagement.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        const string CsFileExtensions = "cshtml";
        protected void Application_Start()
        {
           // AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SetupContainer2();

            ViewEngines.Engines.Clear();
            var razorEngine = new RazorViewEngine() { FileExtensions = new[] { CsFileExtensions } };
            ViewEngines.Engines.Add(razorEngine);

            var mailSender = GetContainer().Resolve<IMessageService>("StaticMessageService");
            mailSender.Start();
        }

        private void SetupContainer2()
        {

            var mainContainer = new DryIoc.Container();
            var container = DryIoc.Mvc.DryIocMvc.WithMvc(mainContainer);

            DryIoc.IReuse requestLifetimeScope = DryIoc.Reuse.InWebRequest;
            DryIoc.IReuse containerLifetimeScope = DryIoc.Reuse.Singleton;

            container.Register<DapperContext>(reuse: requestLifetimeScope,
                 serviceKey: "RequestDBContext", made: Made.Of(() => new DapperContext()));

            container.Register<DapperContext>(reuse: containerLifetimeScope,
                serviceKey: "StaticDBContext", made: Made.Of(() => new DapperContext()));

            container.RegisterInstance(ConfigurationManager.ConnectionStrings["finConnectionString"].Name,
                serviceKey: "oracleDBConnectionString");

            container.Register<DapperContext>(reuse: requestLifetimeScope, serviceKey: "OracleDBContext",
                made: Made.Of(() => new DapperContext(Arg.Of<string>("oracleDBConnectionString"))));

            container.Register<IFinacleRepository, FinacleRepository>(reuse: requestLifetimeScope,
                made: Made.Of(() => new FinacleRepository(Arg.Of<DapperContext>("OracleDBContext"))));

            container.Register<ISecurityRepository, SecurityRepository>(reuse: requestLifetimeScope,
                made: Made.Of(() => new SecurityRepository(Arg.Of<DapperContext>("RequestDBContext"),
                                                           Arg.Of<IPortalSettingsRepository>())));

            container.Register<IDatastoreValidationRepository, DatastoreValidationRepository>(reuse: requestLifetimeScope,
                 made: Made.Of(() => new DatastoreValidationRepository(Arg.Of<DapperContext>("RequestDBContext"))));

            container.Register<IMessageRepository, MessageRepository>(reuse: containerLifetimeScope, serviceKey: "StaticMessageRepository",
                made: Made.Of(() => new MessageRepository(Arg.Of<DapperContext>("StaticDBContext"))));

            container.Register<IMessageRepository, MessageRepository>(reuse: requestLifetimeScope, serviceKey: "RequestMessageRepository",
                 made: Made.Of(() => new MessageRepository(Arg.Of<DapperContext>("RequestDBContext"))));

            container.Register<ILogRepository, LogRepository>(reuse: containerLifetimeScope, serviceKey: "StaticLogRepository",
                 made: Made.Of(() => new LogRepository(Arg.Of<DapperContext>("StaticDBContext"))));

            container.Register<ILogRepository, LogRepository>(reuse: requestLifetimeScope, serviceKey: "RequestLogRepository",
                 made: Made.Of(() => new LogRepository(Arg.Of<DapperContext>("RequestDBContext"))));

            container.Register<IAuditRepository, AuditRepository>(reuse: requestLifetimeScope,
                made: Made.Of(() => new AuditRepository(Arg.Of<DapperContext>("RequestDBContext"))));

            container.Register<ILogService, LogService>(reuse: containerLifetimeScope, serviceKey: "StaticLogService",
                made: Made.Of(() => new LogService(Arg.Of<ILogRepository>("StaticLogRepository"), Arg.Of<IMessageRepository>("StaticMessageRepository"))));

            container.Register<ILogService, LogService>(reuse: requestLifetimeScope, serviceKey: "RequestLogService",
                made: Made.Of(() => new LogService(Arg.Of<ILogRepository>("RequestLogRepository"), Arg.Of<IMessageRepository>("RequestMessageRepository"))));

            container.Register<IMessageService, MessageService>(reuse: containerLifetimeScope, serviceKey: "StaticMessageService",
                made: Made.Of(() => new MessageService(Arg.Of<IMessageRepository>("StaticMessageRepository"))));
            container.Register<IReportsRepository, ReportsRepository>(reuse: requestLifetimeScope,
                made: Made.Of(() => new ReportsRepository(Arg.Of<DapperContext>("RequestDBContext"),
                                                          Arg.Of<IPortalSettingsRepository>())));

            container.Register<IPortalSettingsRepository, PortalSettingsRepository>(reuse: requestLifetimeScope,
               made: Made.Of(() => new PortalSettingsRepository(Arg.Of<DapperContext>("RequestDBContext"))));
            container.Register<IPasswordHistoryRepository, PasswordHistoryRepository>(reuse: requestLifetimeScope,
               made: Made.Of(() => new PasswordHistoryRepository(Arg.Of<DapperContext>("RequestDBContext"))));

            container.Register<IPortalSettingsService, PortalSettingsService>(reuse: requestLifetimeScope);
            container.Register<IPasswordHistoryService, PasswordHistoryService>(reuse: requestLifetimeScope,
                made: Made.Of(() => new PasswordHistoryService(
                    Arg.Of<IPasswordHistoryRepository>(),
                    Arg.Of<IPortalSettingsRepository>(),
                    Arg.Of<ICacheService>())));

            container.Register<IAuditService, AuditService>(reuse: requestLifetimeScope);
            container.Register<ICacheService, CacheService>(reuse: containerLifetimeScope);
            container.Register<IReportService, ReportService>(reuse: requestLifetimeScope);
            container.Register<ISecurityService, SecurityService>(reuse: requestLifetimeScope,
                made: Made.Of(() => new SecurityService(Arg.Of<ISecurityRepository>(),
                    Arg.Of<IDatastoreValidationRepository>(), Arg.Of<ICacheService>(),
                    Arg.Of<ILogService>("RequestLogService"),
                     Arg.Of<IPortalSettingsService>(),
                     Arg.Of<IPasswordHistoryService>()
                    )));

            Application.Add("container", container);
        }

        //private void SetupContainer()
        //{
        //    IContainer container = new PermissionManagement.IoC.Container();

        //    RegisterControllerFactory(container);

        //    container.Register<DapperContext>(
        //        c => new DapperContext()).WithLifetimeManager(new RequestLifetime());

        //    container.Register<DapperContext>("AuditLogDBContext",
        //         c => new DapperContext()).WithLifetimeManager(new ContainerLifetime());
        //    container.Register<DapperContext>("StaticDBContext",
        //            c => new DapperContext()).WithLifetimeManager(new ContainerLifetime());
        //    container.Register<DapperContext>("OracleDBContext",
        //        c => new DapperContext(ConfigurationManager.ConnectionStrings["finConnectionString"].Name)).WithLifetimeManager(new RequestLifetime());

        //    container.RegisterInstance<ICacheService>(new CacheService()).WithLifetimeManager(new ContainerLifetime());

        //    container.Register<IFinacleRepository>(
        //        c => new FinacleRepository(c.Resolve<DapperContext>("OracleDBContext"))).WithLifetimeManager(new RequestLifetime());

        //    container.Register<ISecurityService>(
        //        c => new SecurityService(c.Resolve<ISecurityRepository>(), c.Resolve<IDatastoreValidationRepository>(), c.Resolve<ICacheService>(), c.Resolve<ILogService>())).WithLifetimeManager(new RequestLifetime());
        //    container.Register<ISecurityRepository>(c => new SecurityRepository(c.Resolve<DapperContext>())).WithLifetimeManager(new RequestLifetime());

        //    container.Register<IDatastoreValidationRepository>(c => new DatastoreValidationRepository(c.Resolve<DapperContext>())).WithLifetimeManager(new RequestLifetime());

        //    container.Register<IMessageRepository>("StaticMessageRepository",
        //        c => new MessageRepository(c.Resolve<DapperContext>("StaticDBContext"))).WithLifetimeManager(new ContainerLifetime());

        //    container.Register<IMessageService>("StaticMessageService", c => new MessageService(c.Resolve<IMessageRepository>("StaticMessageRepository"))).WithLifetimeManager(new ContainerLifetime());
        //    container.Register<IMessageRepository>(c => new MessageRepository(c.Resolve<DapperContext>())).WithLifetimeManager(new RequestLifetime());

        //    container.Register<ILogService>("StaticLogService",
        //        c => new LogService(c.Resolve<ILogRepository>("StaticLogRepository"), c.Resolve<IMessageRepository>("StaticMessageRepository"))).WithLifetimeManager(new ContainerLifetime());
        //    container.Register<ILogRepository>("StaticLogRepository", c => new LogRepository(c.Resolve<DapperContext>("StaticDBContext"))).WithLifetimeManager(new ContainerLifetime());

        //    container.Register<ILogService>(
        //        c => new LogService(c.Resolve<ILogRepository>(), c.Resolve<IMessageRepository>())).WithLifetimeManager(new RequestLifetime());
        //    container.Register<ILogRepository>(c => new LogRepository(c.Resolve<DapperContext>())).WithLifetimeManager(new RequestLifetime());

        //    container.Register<IAuditService>(
        //        c => new AuditService(c.Resolve<IAuditRepository>())).WithLifetimeManager(new RequestLifetime());
        //    container.Register<IAuditRepository>(c => new AuditRepository(c.Resolve<DapperContext>("AuditLogDBContext"))).WithLifetimeManager(new RequestLifetime());

        //    // Register the Controllers
        //    container.Register<IController>("Home", c => new HomeController(c.Resolve<ISecurityService>(), c.Resolve<IFinacleRepository>()));
        //    container.Register<IController>("Admin", c => new AdminController(c.Resolve<ISecurityService>(), c.Resolve<IAuditService>()));
        //    container.Register<IController>("MyProfile", c => new MyProfileController(c.Resolve<ISecurityService>()));
        //    container.Register<IController>("UserSetup", c => new UserSetupController(c.Resolve<ISecurityService>(), c.Resolve<ICacheService>(), c.Resolve<IFinacleRepository>()));
        //    container.Register<IController>("Audit", c => new AuditController(c.Resolve<IAuditService>(), c.Resolve<ISecurityService>()));
            
        //    Application.Add("container", container);
        //}
        
        //private void RegisterControllerFactory(IContainer ioc)
        //{
        //    // Create the Default Factory
        //    var controllerFactory = new MunqControllerFactory(ioc);

        //    // set the controller factory
        //    ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        //}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            CultureInfo culture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            HttpContext.Current.Response.AddHeader("x-frame-options", "SAMEORIGIN"); //Click jack prevention
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            foreach (var s in HttpContext.Current.Items.Values)
            {
                var s2 = s as IDisposable;
                if (s2 != null) { s2.Dispose(); }
            }
        }

        protected void Application_OnError(object sender, EventArgs e)
        {
            // Get the most recent exception - we are going to log this to help in fixing it
            Exception ex = default(Exception);
            ex = HttpContext.Current.Server.GetLastError();
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            //comment out for now.

            var errorModel = BuildError(ex);
            if (string.Compare(errorModel.ExceptionPage, "robots.txt", true) != 0)
            {
                ILogService logger = GetContainer().Resolve<ILogService>("StaticLogService");
                logger.LogError(errorModel, false);
            }

            var httpException = ex as HttpException;
            var errorPage = httpException != null && httpException.GetHttpCode() == 404 ? Constants.General.PageNotFound : Constants.General.ErrorPage;
            Response.StatusCode = httpException != null ? httpException.GetHttpCode() : 500;

            HttpContext.Current.Server.ClearError();

            if (!HttpContext.Current.Request.Url.ToString().StartsWith(Helper.GetRootURL() + "/" + errorPage))
            {
                if (errorPage == Constants.General.ErrorPage)
                {
                    Response.Redirect(Helper.GetRootURL() + "/" + errorPage + "?errorCode=" + errorModel.ExceptionId.ToString(), true);
                }
                else
                {
                    Response.Redirect(Helper.GetRootURL() + "/" + errorPage, true);
                }
            }
        }

        protected void Application_PreSendRequestHeaders()
        {
            // ensure that if GZip/Deflate Encoding is applied that headers are set
            // also works when error occurs if filters are still active
            HttpResponse response = HttpContext.Current.Response;
            if (response.Filter is GZipStream && response.Headers["Content-encoding"] != "gzip")
                response.AppendHeader("Content-encoding", "gzip");
            else if (response.Filter is DeflateStream && response.Headers["Content-encoding"] != "deflate")
                response.AppendHeader("Content-encoding", "deflate");
        }

        public static ExceptionLog BuildError(Exception errorObject)
        {
            ExceptionLog errorClass = new ExceptionLog();
            errorClass.ExceptionDetails = FormatException(errorObject);

            System.Web.HttpContext context = HttpContext.Current;

            errorClass.UserIPAddress = context.Request.ServerVariables["REMOTE_ADDR"];
            errorClass.ExceptionPage = context.Request.ServerVariables["SCRIPT_NAME"].Substring(System.Web.HttpRuntime.AppDomainAppVirtualPath.Length);

            string account = context.User == null ? "anonymous" : context.User.Identity.Name;
            errorClass.LoggedInUser = string.IsNullOrEmpty(account) ? "anonymous" : account;

            errorClass.ExceptionType = errorObject.GetType().FullName;
            errorClass.ExceptionMessage = errorObject.Message;
            errorClass.ExceptionDateTime = Helper.GetLocalDate();

            string version = System.Reflection.Assembly.GetCallingAssembly().FullName;
            version = version.Substring(version.IndexOf("Version=") + 8);
            version = version.Substring(0, version.IndexOf(","));
            errorClass.ExceptionVersion = version;

            return errorClass;
        }

        public static string FormatException(Exception ex)
        {
            System.Text.StringBuilder body = new System.Text.StringBuilder();

            string exDetails = ex.ToString();
            System.Web.HttpContext context = HttpContext.Current;

            body.Append("<style>.fixed { font: 8pt Lucida Console; white-space: nowrap; padding-right: 15px; }</style>" + Environment.NewLine + Environment.NewLine);
            body.Append("<style>body { padding: 0; margin: 0; }</style>" + Environment.NewLine);
            body.Append("<style>.errh1 { padding: 10px; font: 12pt Verdana; background-color: red; color: white; }</style>" + Environment.NewLine);

            // General information.
            body.Append(Environment.NewLine + "<div class=\"errh1\">Exception Occurred in: <b>" + ConfigurationManager.AppSettings["CompanyName"] + "</b></div>" + Environment.NewLine);

            body.Append("<br/>");
            body.Append("<b>Date/Time:</b> " + Helper.GetLocalDate().ToString() + "&nbsp;&nbsp;&nbsp;" + Environment.NewLine);
            int headpos = body.Length - 1;

            body.Append("<br/><br/>");
            string authUser = context.Request.ServerVariables["AUTH_USER"];
            if (authUser.Length > 0)
            {
                body.Append("<b>User:</b> " + authUser + "&nbsp;&nbsp;&nbsp;" + Environment.NewLine);
            }
            if (authUser != System.Threading.Thread.CurrentPrincipal.Identity.Name)
            {
                body.Append("<b>User:</b> " + System.Threading.Thread.CurrentPrincipal.Identity.Name + "&nbsp;&nbsp;&nbsp;" + Environment.NewLine);
            }

            string url = "http://" + context.Request.ServerVariables["HTTP_HOST"] + context.Request.ServerVariables["SCRIPT_NAME"];
            string qs = context.Request.QueryString.Count == 0 ? string.Empty : "?" + context.Request.QueryString.ToString();
            body.Append("<br/><b>Page:</b> <a href=\"" + url + qs + "\">" + url + "</a> " + context.Server.UrlDecode(qs) + "<br/><br/>" + Environment.NewLine);
            body.Append("<br/><b>Stack Trace:</b> " + ex.StackTrace + "<br/>" + Environment.NewLine);

            body.Append("</p>");

            body.Replace("<b> ", "<b>");
            body.Replace("<b>", "&nbsp;<b>");

            body.Replace("&nbsp;<b>Exception Details: </b>", "&nbsp;<b>Exception Details: </b><span style='color: red; font: bold 12pt Arial'>");

            return body.Length > 4000 ? body.Remove(3995, body.Length - 3995).Append("</p>").ToString() : body.ToString();
        }

        private IContainer GetContainer()
        {
            return (IContainer)Application["container"];
        }
    }
}
