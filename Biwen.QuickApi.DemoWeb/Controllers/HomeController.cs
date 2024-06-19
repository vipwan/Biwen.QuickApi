using Microsoft.FeatureManagement.Mvc;

namespace Biwen.QuickApi.DemoWeb.Controllers
{
    [FeatureGate("mycontroller")]
    [Area("MyArea")]
    public class HomeController : Controller
    {
        //~/MyArea/home/index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
