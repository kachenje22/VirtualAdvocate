using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualAdvocate.Controllers
{
    public class TrackerController : BaseController
    {
        // GET: Tracker
        public ActionResult Index()
        {
            return View();
        }
    }
}