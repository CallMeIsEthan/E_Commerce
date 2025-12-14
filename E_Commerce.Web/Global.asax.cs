using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;

namespace E_Commerce.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            
            // Đăng ký Web API routes TRƯỚC MVC routes
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Cấu hình AutoMapper TRƯỚC Unity (vì Unity cần IMapper instance)
            AutoMapperConfig.Configure();

            // Cấu hình Unity Dependency Injection
            UnityConfig.RegisterComponents();
        }

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 404)
            {
                var path = Context.Request.Path;
                if (!path.StartsWith("/Error/NotFound", StringComparison.OrdinalIgnoreCase))
                {
                    Context.Response.Clear();
                    Context.Response.StatusCode = 404;
                    Context.Server.TransferRequest("/Error/NotFound");
                }
            }
        }
    }
}
