using Microsoft.AspNetCore.Mvc;
using UserChat.Models;

namespace UserChat.Controllers
{
    public class AdminChatController : Controller
    {
        private readonly ApiRequest _apiRequest;

        public AdminChatController(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                return RedirectToAction("Login");
            }

            var adminUser = await _apiRequest.GetUserByUidAsync(uid);
            if (adminUser == null || adminUser.Type != UserType.Admin)
            {
                return RedirectToAction("Login");
            }

            // Получение всех сообщений
            var messages = await _apiRequest.ReadMSGAsync(uid); // Здесь получаются все сообщения
            var chatViewModel = new ChatViewModel
            {
                Messages = messages,
                Uid = uid
            };

            return View(chatViewModel);
        }
        // Метод для отображения страницы входа
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // Метод для обработки входа
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _apiRequest.GetUserByPhoneAsync(model.Phone);
                if (user != null && user.Type == UserType.Admin)
                {
                    return RedirectToAction("Index", "AdminChat", new { uid = user.Uid });
                }
                ModelState.AddModelError("", "Неверный номер телефона или пользователь не является администратором.");
            }
            return View(model);
        }

        // Метод для отображения формы создания пользователя
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new CreateUserViewModel());
        }
        // Обработка отправки сообщения
        [HttpPost]
        public async Task<IActionResult> SendMessage(string uid, string messageContent, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return BadRequest("Сообщение не может быть пустым.");
            }

            // Создание нового сообщения
            var chatMessage = new ChatMessage
            {
                UserName = isAdmin ? "Администратор" : "Пользователь", // Замените на реальное имя отправителя
                Message = messageContent,
                Data = DateTime.Now,
                Type = isAdmin ? UserType.Admin : UserType.User,
                Uid = uid // Или другой уникальный идентификатор, если требуется
            };

            // Сохранение сообщения в базе данных или другом хранилище
            await _apiRequest.InsertMSGAsync(chatMessage.UserName, chatMessage.Message, chatMessage.Uid);

            return RedirectToAction("Index", "AdminChat", new { uid });
        }
        // Метод для обработки создания пользователя
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Логика для создания нового пользователя
                var uid = await _apiRequest.AddUserAsync(model.UserName, model.Phone, "Admin");
                if (!string.IsNullOrEmpty(uid))
                {
                    return RedirectToAction("Index", "AdminChat", new { uid });
                }
                ModelState.AddModelError("", "Не удалось создать пользователя.");
            }
            return View(model);
        }
    }
}