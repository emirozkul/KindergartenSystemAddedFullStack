using System.Web.Mvc;

namespace KindergartenSystem.Controllers
{
    public class DebugController : Controller
    {
        public ActionResult Index()
        {
            return View("Debug");
        }
    }
}