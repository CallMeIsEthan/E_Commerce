using System.Web;
using System.Web.Optimization;

namespace E_Commerce.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Tắt minification trong development để dễ debug và tránh lỗi WebGrease
            BundleTable.EnableOptimizations = false;
            
            // jQuery
            var jqueryBundle = new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.7.0.js");
            jqueryBundle.Transforms.Clear();
            bundles.Add(jqueryBundle);

            var jqueryvalBundle = new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*");
            jqueryvalBundle.Transforms.Clear();
            bundles.Add(jqueryvalBundle);

            // Modernizr
            var modernizrBundle = new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*");
            modernizrBundle.Transforms.Clear();
            bundles.Add(modernizrBundle);

            // Bootstrap JS - Removed vì đã có trong vendor bundle (bootstrap.bundle.min.js đã bao gồm Popper)
            // var bootstrapBundle = new ScriptBundle("~/bundles/bootstrap").Include(
            //           "~/Scripts/bootstrap.js");
            // bootstrapBundle.Transforms.Clear();
            // bundles.Add(bootstrapBundle);

            // Vendor JS từ template NiceShop - tắt minification vì đã là file .min.js
            // bootstrap.bundle.min.js đã bao gồm Popper.js
            var vendorBundle = new ScriptBundle("~/bundles/vendor").Include(
                      "~/Content/vendor/bootstrap/js/bootstrap.bundle.min.js",
                      "~/Content/vendor/swiper/swiper-bundle.min.js",
                      "~/Content/vendor/aos/aos.js",
                      "~/Content/vendor/glightbox/js/glightbox.min.js",
                      // "~/Content/vendor/drift-zoom/Drift.min.js", // Removed - using GLightbox instead
                      "~/Content/vendor/purecounter/purecounter_vanilla.js");
            vendorBundle.Transforms.Clear(); // Tắt minification
            bundles.Add(vendorBundle);

            // Main JS từ template - tắt minification
            var mainBundle = new ScriptBundle("~/bundles/main").Include(
                      "~/Scripts/main.js");
            mainBundle.Transforms.Clear(); // Tắt minification
            bundles.Add(mainBundle);

            // Bootstrap CSS
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                      "~/Content/bootstrap.css"));

            // Vendor CSS từ template NiceShop (đã được include trong ~/Content/css)
            // bundles.Add(new StyleBundle("~/Content/vendor").Include(...)); // Comment để tránh duplicate

            // Main CSS từ template (đã được include trong ~/Content/css)
            // bundles.Add(new StyleBundle("~/Content/main").Include(...)); // Comment để tránh duplicate

            // Site CSS (nếu có custom CSS)
            bundles.Add(new StyleBundle("~/Content/site").Include(
                      "~/Content/Site.css"));

            // Bundle tổng hợp cho CSS (đơn giản hóa, tránh duplicate)
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/vendor/bootstrap/css/bootstrap.min.css",
                      "~/Content/vendor/bootstrap-icons/bootstrap-icons.css",
                      "~/Content/vendor/swiper/swiper-bundle.min.css",
                      "~/Content/vendor/aos/aos.css",
                      "~/Content/vendor/glightbox/css/glightbox.min.css",
                      // "~/Content/vendor/drift-zoom/drift-basic.css", // Removed - using GLightbox instead
                      "~/Content/css/main.css",
                      "~/Content/Site.css"));
        }
    }
}
