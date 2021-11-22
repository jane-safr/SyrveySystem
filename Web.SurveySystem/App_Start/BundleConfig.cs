using System.Web.Optimization;

namespace Web.SurveySystem
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery/jquery-{version}.js")
                .Include("~/Scripts/jquery/jquery-ui-1.12.1.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/theme").Include(
                "~/Scripts/app/share/global.js",
                "~/Scripts/app/share/sb-admin.js",
                "~/Scripts/jquery/jquery-migrate-3.3.0.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap/bootstrap.bundle.min.js"
            ));
            //jquery-easing
            bundles.Add(new ScriptBundle("~/bundles/jqueryeasing").Include(
                "~/Scripts/jquery-easing/jquery.easing.min.js"));

            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/Content/css/all.min.css",
                "~/Content/css/bootstrap.min.css",
                "~/Content/css/util.css",
                "~/Content/css/sb-admin.css",
                "~/Content/css/Site.css",
                "~/Content/css/fontawesome.min.css",
                "~/Content/css/material-design-iconic-font.min.css"
            ));

            //login
            bundles.Add(new StyleBundle("~/bundles/loginCss")
                .Include("~/Content/css/main.min.css",
                    "~/Content/css/bootstrap/bootstrap.min.css",
                    "~/Content/css/util.min.css",
                    "~/Content/css/all.min.css",
                    "~/Content/css/material-design-iconic-font.min.css",
                    "~/Content/css/jquery.toast.min.css",
                    "~/Content/css/fontawesome.min.css"));
            bundles.Add(new ScriptBundle("~/bundles/loginJs")
                .Include(
                    "~/Scripts/jquery/jquery-migrate-3.3.0.min.js",
                    "~/Scripts/jquery/jquery.unobtrusive-ajax.min.js",
                    "~/Scripts/bootstrap/bootstrap.min.js",
                    "~/Scripts/login/main.js",
                    "~/Scripts/login/loginForm.js",
                    "~/Scripts/login/jquery.toast.min.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/kendocss").Include(
                "~/Content/kendo/kendo.common.min.css",
                "~/Content/kendo/kendo.bootstrap-v4.min.css",
                "~/Content/kendo/kendo.common-bootstrap.min.css",
                "~/Content/kendo/kendo.common-material.min.css"));
            // kendo-angular
            bundles.Add(new ScriptBundle("~/bundles/kendojs")
                .Include("~/Scripts/kendo-ui/angular.min.js")
                .Include("~/Scripts/kendo-ui/jszip.min.js")
                .Include("~/Scripts/kendo-ui/kendo.all.min.js")
            );

        #if !DEBUG
            BundleTable.EnableOptimizations = true;
        #endif
        }
    }
}
