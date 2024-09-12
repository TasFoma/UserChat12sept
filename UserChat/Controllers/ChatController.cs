using Microsoft.AspNetCore.Mvc;
using UserChat.Models;
using static UserChat.ApiRequest;

namespace UserChat.Controllers
{
    public class ChatController: Controller
    {

        private readonly ApiRequest _apiRequest;
        public ChatController()
        {
            _apiRequest = new ApiRequest();
        }

        public IActionResult Index() 
        {
            List<Chat> messages = _apiRequest.ReadMSG("2");

            return View();
        }
    }
}
