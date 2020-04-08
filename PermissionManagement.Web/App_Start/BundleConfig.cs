using System.Web;
using System.Web.Optimization;

namespace PermissionManagement.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            "~/Scripts/jquery-ui-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootbox.min.js",
                      "~/Scripts/bootstrap-dialog.min.js",
                      "~/Scripts/listbox.js",
                      "~/Scripts/custom.js",
                      "~/Scripts/jquery.metisMenu.js",
                      "~/Scripts/DataTables/dataTables.min.js",
                      "~/Scripts/DataTables/jquery.dataTables.yadcf.js",
                      "~/Scripts/DataTables/dataTables.fixedColumns.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/jqueryui").Include(
                "~/Content/themes/base/all.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/bootstrap-dialog.min.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/googlefont.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/DataTables/css/dataTables.min.css",
                      "~/Content/DataTables/css/jquery.dataTables.yadcf.css",
                      "~/Content/DataTables/css/dataTables.customLoader.walker.css"
                      ));
        }
    }
}
