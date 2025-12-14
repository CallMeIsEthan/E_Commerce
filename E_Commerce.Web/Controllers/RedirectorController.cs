using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace E_Commerce.Web.Controllers
{
    public class RedirectorController : Controller
    {
        // GET: Redirector
        public ActionResult ToUser()
        {
            return RedirectToAction("Index", "Home", new { area = "User" });
        }
    }
}