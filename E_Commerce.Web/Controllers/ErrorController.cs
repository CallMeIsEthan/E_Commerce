using System.Web.Mvc;

namespace E_Commerce.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        public ActionResult Unauthorized()
        {
            Response.StatusCode = 401;
            return View();
        }
    }
}

