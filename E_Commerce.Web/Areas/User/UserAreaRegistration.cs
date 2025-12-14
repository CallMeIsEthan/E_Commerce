using System.Web.Mvc;

namespace E_Commerce.Web.Areas.User
{
    public class UserAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "User";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            // Route cho trang chủ (Home/Index) - có thể truy cập không cần prefix "User"
            context.MapRoute(
                "User_Home",
                "",
                new { controller = "Home", action = "Index", area = "User" }
            );
            
            // Route mặc định cho các controller khác trong User area
            context.MapRoute(
                "User_default",
                "User/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}