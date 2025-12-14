using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace E_Commerce.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable CORS nếu cần
            // config.EnableCors();

            // Map attribute routes
            config.MapHttpAttributeRoutes();

            // Default API route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // API route cho Admin area (sử dụng attribute routing thay vì namespaces)
            // Controllers trong Admin area sẽ sử dụng [RoutePrefix] để định nghĩa route

            // Cấu hình JSON formatter
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = 
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = 
                Newtonsoft.Json.PreserveReferencesHandling.None;
            
            // Chỉ trả về JSON (tùy chọn)
            // config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
