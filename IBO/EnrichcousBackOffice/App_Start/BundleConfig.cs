using System.Web;
using System.Web.Optimization;

public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        bundles.Add(new StyleBundle("~/bundles/css").Include(
                        //"~/content/admin/bower_components/bootstrap/dist/css/bootstrap.min.css",
                        //"~/content/admin/bower_components/font-awesome/css/font-awesome.min.css",
                        //"~/content/admin/bower_components/Ionicons/css/ionicons.min.css",
                        "~/content/admin/bower_components/select2/dist/css/select2.min.css",
                        "~/content/admin/dist/css/AdminLTE.css",
                        "~/content/admin/dist/css/skins/_all-skins.css",
                        "~/Content/noty/jquery.noty.css",
                        "~/Content/noty/noty_theme_default.css",
                        //"~/Content/css/dataTables.bootstrap4.min.css",
                        "~/Content/css/SansProFont.css"
                        ));
        bundles.Add(new ScriptBundle("~/bundles/js").Include(
            //"~/content/admin/bower_components/bootstrap/dist/js/bootstrap.min.js",
            "~/Scripts/date.format.js",
            //"~/content/admin/bower_components/select2/dist/js/select2.full.min.js",
            //"~/content/admin/bower_components/jquery-sparkline/dist/jquery.sparkline.min.js",
            //"~/content/admin/bower_components/jquery-slimscroll/jquery.slimscroll.min.js",
            "~/content/admin/bower_components/fastclick/lib/fastclick.js",
            //"~/content/admin/dist/js/adminlte.min.js",
            "~/Content/noty/jquery.noty.js",
            "~/Content/Scrollbar/sticky-kit.min.js"
            //"~/Content/scripts/jquery.dataTables.min.js",
            //"~/Content/scripts/dataTables.bootstrap4.min.js",

            ));
        BundleTable.EnableOptimizations = true;
    }
}