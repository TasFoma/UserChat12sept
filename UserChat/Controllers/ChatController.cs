using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserChat.Models;

namespace UserChat.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApiRequest _apiRequest;

        public ChatController(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
        }

        // Главная страница чата
        [HttpGet]
        public async Task<IActionResult> Index(string uid)
        {
            // Если uid не предоставлен, создаем нового пользователя "Система" с типом "Admin"
            if (string.IsNullOrEmpty(uid))
            {
                uid = await _apiRequest.AddUserAsync("Система", "", "Admin");
                if (string.IsNullOrEmpty(uid))
                {
                    return View("Error");
                }
            }

            // Получаем пользователя по uid
            var user = await _apiRequest.GetUserByUidAsync(uid);
            if (user == null)
            {
                uid = await _apiRequest.AddUserAsync("Система", "", "Admin");
                if (string.IsNullOrEmpty(uid))
                {
                    return View("Error");
                }
            }

            // Создаем нового пользователя "Вы"
            var guestUid = await _apiRequest.AddUserAsync("Вы", "", "Guest");
            if (string.IsNullOrEmpty(guestUid))
            {
                return View("Error");
            }

            // Получаем сообщения для пользователя
            var messages = await _apiRequest.ReadMSGAsync(uid);
            var chatViewModel = new ChatViewModel
            {
                Messages = messages,
                Uid = uid,
                GuestUid = guestUid, // Сохраняем uid гостя в модели
                GuestName = "Вы", // Или другое имя, если нужно
                MainQuestions = new List<MainQuestion>() // Изначально пустой список
            };

            // Приветственное сообщение
            var welcomeMessage = "Здравствуйте, Вас приветствует компания Инвинтро, чем я могу помочь?";
            var selectMessage = "Выберите интересующий раздел:";

            // Проверяем, существует ли уже приветственное сообщение
            var existingWelcomeMessage = await _apiRequest.CheckIfMessageExistsAsync(uid, welcomeMessage);
            if (!existingWelcomeMessage)
            {
                await _apiRequest.SystemMessageAsync(uid, welcomeMessage);
            }

            // Проверяем, существует ли уже сообщение о выборе раздела
            var existingSelectMessage = await _apiRequest.CheckIfMessageExistsAsync(uid, selectMessage);
            if (!existingSelectMessage)
            {
                await _apiRequest.SystemMessageAsync(uid, selectMessage);
            }

            // Получаем обновленные сообщения для пользователя
            chatViewModel.Messages = await _apiRequest.ReadMSGAsync(uid);

            // Получаем главные вопросы после отправки сообщения
            chatViewModel.MainQuestions = await _apiRequest.GetMainQuestionsAsync();

            // Сохраняем их в ViewBag или передаем в модель
            ViewBag.MainQuestions = chatViewModel.MainQuestions;

            return View(chatViewModel); // Передаем модель в представление
        }

        [HttpPost]
        public async Task<IActionResult> SaveReturnMessage(string uid, string questionName)
        {
            var message = $"Вы вернулись к вопросу: {questionName}";
            string userName = "Система"; // Или любое другое имя пользователя, которое вы хотите использовать для системных сообщений

            // Вызов метода для вставки сообщения в базу данных
            await _apiRequest.InsertMSGAsync(userName, message, uid);

            return Ok();
        }

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        [HttpPost]
        public async Task<IActionResult> SelectQuestion(int questionId, int tree, string uid, string questionName, string guestUid)
        {
            await semaphore.WaitAsync(); // Ожидаем, пока семафор будет доступен
            try
            {
                // Получаем все под-вопросы для текущего дерева
                var questions = await _apiRequest.GetQuestionsByIdTreeAsync(tree, tree);
                var selectedQuestion = questions.FirstOrDefault(q => q.Id == questionId);

                // Получаем под-вопросы для заданного questionId
                var mainQuestions = await GetSubQuestions(questionId, tree);

                // Формируем системное сообщение с использованием имени вопроса
                string systemMessage = $"Пожалуйста, выберите следующий раздел.";
                await _apiRequest.SystemMessageAsync(uid, systemMessage);
               
                // Получаем имя пользователя для сохранения сообщения
                var user = await _apiRequest.GetUserByUidAsync(guestUid); // Получаем пользователя по guestUid

                // Сохраняем сообщение от пользователя "Вы"
                if (!string.IsNullOrEmpty(guestUid) && user != null)
                {
                    await _apiRequest.InsertMSGAsync(user.Name, questionName, guestUid); // Передаем имя пользователя и сообщение
                }

                // Если нужно передать uid в частичное представление, создаем новую модель
                var questionButtonsViewModel = new QuestionButtonsViewModel
                {
                    MainQuestions = mainQuestions,
                    Uid = uid // Передаем uid, если он нужен в частичном представлении
                };

                // Возвращаем обновленные под-вопросы
                return PartialView("_QuestionButtons", questionButtonsViewModel); // Здесь возвращаем новую модель
            }
            finally
            {
                semaphore.Release(); // Освобождаем семафор
            }
        }
        // Рекурсивный метод для получения под-вопросов
        private async Task<List<MainQuestion>> GetSubQuestions(int questionId, int currentTreeLevel)
        {
            var subQuestions = new List<MainQuestion>();

            // Получаем под-вопросы по ID вопроса и следующему значению Tree
            var questions = await _apiRequest.GetQuestionsByIdTreeAsync(questionId, currentTreeLevel + 1);

            foreach (var question in questions)
            {
                var mainQuestion = new MainQuestion
                {
                    Id = question.Id,
                    Name = question.Name,
                    Tree = currentTreeLevel + 1, // Увеличиваем уровень дерева для под-вопросов
                    SubQuestions = await GetSubQuestions(question.Id, currentTreeLevel + 1) // Рекурсивный вызов
                };
                subQuestions.Add(mainQuestion);
            }

            return subQuestions;
        }
        [HttpPost]
        public async Task<IActionResult> CheckQuestion(string uid, string question)
        {
            try
            {
                // Проверяем существование вопроса в базе данных
                var (exists, questionId, tree) = await _apiRequest.CheckIfQuestionExistsAsync(question);

                if (exists)
                {
                    // Если вопрос существует, можем получить под-вопросы
                    var mainQuestions = await GetSubQuestions(questionId, tree); // Получаем под-вопросы

                    // Формируем ответ для обновления интерфейса
                    var questionButtonsViewModel = new QuestionButtonsViewModel
                    {
                        MainQuestions = mainQuestions,
                        Uid = uid
                    };

                    // Возвращаем частичное представление с под-вопросами
                    return PartialView("_QuestionButtons", questionButtonsViewModel);
                }
                else
                {
                    return NotFound(); // Если вопрос не найден, возвращаем 404
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка в CheckQuestion: {ex.Message}");
                return StatusCode(500, "Произошла ошибка на сервере.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveUserMessage(string uid, string message, string guestUid)
        {
            // Логика сохранения сообщения в базе данных
            await _apiRequest.InsertMSGAsync(guestUid, message, guestUid);
            return Ok(); // Возвращаем успешный ответ
        }

        // Обработка отправки сообщения
        [HttpPost]
        public async Task<IActionResult> SendMessage(string uid, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("", "Сообщение не может быть пустым.");
                return RedirectToAction("Index", new { uid });
            }

            // Получаем пользователя по uid
            var user = await _apiRequest.GetUserByUidAsync(uid);

            // Проверяем, существует ли пользователь
            if (user == null)
            {
                ModelState.AddModelError("", "Пользователь не найден.");
                return RedirectToAction("Index", new { uid });
            }

            // Вставляем сообщение в таблицу chat
            await _apiRequest.InsertMSGAsync(user.Name, message, uid);
            return RedirectToAction("Index", new { uid });
        }
    }
}