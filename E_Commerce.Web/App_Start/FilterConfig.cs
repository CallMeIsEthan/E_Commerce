using System.Web;
using System.Web.Mvc;
using E_Commerce.Web.Filters;

namespace E_Commerce.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CategoryActionFilter()); // Populate categories for navigation menu
        }
    }
}
