using Microsoft.AspNetCore.Mvc;

namespace UserChat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Chat"; // Устанавливаем заголовок
            return View();
        }
    }
}