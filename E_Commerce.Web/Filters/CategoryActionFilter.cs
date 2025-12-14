using System.Web.Mvc;
using E_Commerce.Service;

namespace E_Commerce.Web.Filters
{
    public class CategoryActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Chỉ populate categories cho frontend (không phải admin area)
            if (filterContext.RouteData.DataTokens["area"]?.ToString() != "Admin")
            {
                var categoryService = DependencyResolver.Current.GetService<ICategoryService>();
                if (categoryService != null)
                {
                    var rootCategories = categoryService.GetRootCategories();
                    var allCategories = categoryService.GetActiveCategories();
                    
                    filterContext.Controller.ViewBag.RootCategories = rootCategories;
                    filterContext.Controller.ViewBag.AllCategories = allCategories;
                }
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}

