using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserChat.Models;

namespace UserChat.Controllers
{
    public class UserController : Controller
    {
        private readonly ApiRequest _apiRequest;

        public UserController(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Метод для проверки существования пользователя по uid (например, для входа)
        public async Task<IActionResult> CheckUserByUid(string uid)
        {
            var user = await _apiRequest.GetUserByUidAsync(uid);
            if (user != null)
            {
                // Перенаправление на страницу чата
                return RedirectToAction("Index", "Chat", new { uid = user.Uid });
            }
            return RedirectToAction("Login", "User"); // Если не найден, перенаправляем на страницу входа
        }

        [HttpPost]
        public async Task<IActionResult> Login(string phone)
        {
            var users = await _apiRequest.GetUsersByPhoneAsync(phone);

            if (users.Count > 1)
            {
                // Если найдено несколько пользователей с одним номером
                ViewBag.Users = users;
                return View("SelectUser"); // Передаем на страницу выбора пользователя
            }
            else if (users.Count == 1)
            {
                // Если найден только один пользователь
                return RedirectToAction("Index", "Chat", new { uid = users[0].Uid });
            }
            else
            {
                // Если пользователь не найден, перенаправляем на страницу создания пользователя
                return RedirectToAction("CreateUser", new { phone = phone }); // Передаем номер телефона на страницу создания пользователя
            }
        }

        [HttpGet]
        public IActionResult CreateUser(string phone)
        {
            return View(new { Phone = phone }); // Передаем номер телефона для заполнения формы
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string name, string phone)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                ModelState.AddModelError("", "Имя и номер телефона не могут быть пустыми.");
                return View(new { Phone = phone }); // Возврат на страницу создания пользователя
            }

            // Проверяем, существует ли пользователь с таким номером телефона
            if (await _apiRequest.UserExistsAsync(phone))
            {
                // Если пользователь существует, перенаправляем в чат
                var user = await _apiRequest.GetUserByPhoneAsync(phone);
                return RedirectToAction("Index", "Chat", new { uid = user.Uid });
            }

            // Если пользователь не найден, создаем нового
            var uid = await _apiRequest.AddUserAsync(name, phone);
            if (uid != null)
            {
                return RedirectToAction("Index", "Chat", new { uid = uid });
            }

            return View("Error"); // Обработка ошибки
        }
    }
}