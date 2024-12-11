using Microsoft.AspNetCore.Mvc;

namespace istemci.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
