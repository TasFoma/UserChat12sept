using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserChat.Models;
using UserChat;

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

    [HttpPost]
    public async Task<IActionResult> Login(string phone)
    {
        var user = await _apiRequest.GetUserByPhoneAsync(phone);

        if (user != null && user.Type == UserType.Admin)
        {
            return RedirectToAction("Index", "Chat", new { uid = user.Uid }); // Перенаправляем с uid
        }

        ModelState.AddModelError("", "Неверный номер телефона или пользователь не является администратором.");
        return View();
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(string name, string phone)
    {
        if (await _apiRequest.UserExistsAsync(phone))
        {
            ModelState.AddModelError("", "Пользователь с таким номером телефона уже существует.");
            return View();
        }

        string uid = await _apiRequest.AddUserAsync(name, phone, "admin");

        if (uid != null)
        {
            return RedirectToAction("Index", "Chat", new { uid }); // Перенаправляем с uid
        }

        ModelState.AddModelError("", "Ошибка при создании пользователя.");
        return View();
    }
}