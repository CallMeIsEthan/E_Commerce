using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Commerce.Dto;

namespace E_Commerce.Web.Attributes
{
    /// <summary>
    /// Attribute để bảo vệ các action/controller chỉ dành cho Admin
    /// Kiểm tra session AdminUser và role Admin
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra session AdminUser
            var adminUser = httpContext.Session["AdminUser"] as UserDto;
            
            if (adminUser == null)
            {
                return false;
            }

            // Kiểm tra user có role Admin không
            if (adminUser.Roles == null || adminUser.Roles.Count == 0)
            {
                return false;
            }

            return adminUser.Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu không phải admin, trả về 401
            filterContext.Result = new RedirectResult("~/Error/Unauthorized");
        }
    }
}

