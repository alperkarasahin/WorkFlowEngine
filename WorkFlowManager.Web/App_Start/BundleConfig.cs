using System.Web.Optimization;

namespace WorkFlowManager.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/highlight.js",
                      "~/Scripts/main.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/workflow").Include(
                      "~/Scripts/mermaid.js",
                      "~/Scripts/bootstrap-switch.min.js",
                      "~/Content/plugins/iCheck/icheck.min.js",
                      "~/Scripts/workflow.js",
                      "~/Scripts/kendo.all.min.js",
                      "~/Scripts/PostEditor.js",
                      "~/Scripts/kendo.aspnetmvc.min.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/_all-skins.css",
                      "~/Content/kendo.common.min.css",
                      "~/Content/kendo.default.min.css",
                      "~/Content/AdminLTE.min.css",
                      "~/Content/progress-wizard.min.css",
                      "~/Content/mermaid.forest.css",
                      "~/Content/bootstrap-switch.min.css",
                      "~/Content/plugins/iCheck/all.css",
                      "~/Content/fileUploader.css",
                      "~/Content/workflow.css",
                      "~/Content/site.css"));
        }
    }
}
