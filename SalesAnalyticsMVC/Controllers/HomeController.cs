using System.Web.Mvc;

namespace SalesAnalytics.Web.Dashboard.Controllers
{
    public class HomeController : Controller {
        public ActionResult Index()
        {
            return RedirectToAction("Accounts","Dashboard", new { userInfo = "sitdW1bx0LZm0HuPctdcNuKCZtws0MLjLdwmxkEbDxqkXdMws0Z9kJcQ4KFUQWV3" });
        }
    }
}