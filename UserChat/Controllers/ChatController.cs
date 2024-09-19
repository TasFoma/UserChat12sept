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
            // Если uid не предоставлен, создаем нового пользователя
            if (string.IsNullOrEmpty(uid))
            {
                uid = await _apiRequest.AddUserAsync("Guest", "");
                if (string.IsNullOrEmpty(uid))
                {
                    return View("Error");
                }
            }

            // Получаем пользователя по uid
            var user = await _apiRequest.GetUserByUidAsync(uid);
            if (user == null)
            {
                uid = await _apiRequest.AddUserAsync("Guest", "");
                if (string.IsNullOrEmpty(uid))
                {
                    return View("Error");
                }
            }

            // Получаем сообщения для пользователя
            var messages = await _apiRequest.ReadMSGAsync(uid);
            var chatViewModel = new ChatViewModel
            {
                Messages = messages,
                Uid = uid,
                MainQuestions = new List<MainQuestion>() // Изначально пустой список
            };
            await _apiRequest.InsertWelcomeMessageAsync(uid);
            
            // Отправляем системное сообщение перед загрузкой вопросов
            await _apiRequest.SystemMessageAsync(uid, "Выберите интересующий раздел:");
            
            chatViewModel.Messages = await _apiRequest.ReadMSGAsync(uid);

            // Получаем главные вопросы после отправки сообщения
            chatViewModel.MainQuestions = await _apiRequest.GetMainQuestionsAsync();

            return View(chatViewModel); // Передаем модель в представление
        }
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        [HttpPost]
        public async Task<IActionResult> SelectQuestion(int questionId, int tree, string uid, string questionName)
        {
            await semaphore.WaitAsync(); // Ожидаем, пока семафор будет доступен
            try
            {
                // Получаем все под-вопросы для текущего дерева
                var questions = await _apiRequest.GetQuestionsByIdTreeAsync(tree, tree);
                var selectedQuestion = questions.FirstOrDefault(q => q.Id == questionId);

                // Формируем системное сообщение с использованием имени вопроса
                string systemMessage = $"Вы выбрали вопрос: {questionName}. Пожалуйста, выберите следующий раздел.";

                // Проверяем, существует ли уже такое сообщение в базе данных
                var existingMessage = await _apiRequest.CheckIfMessageExistsAsync(uid, systemMessage);
                if (!existingMessage)
                {
                    await _apiRequest.SystemMessageAsync(uid, systemMessage);
                }
                else
                {
                    // Логирование для отладки
                    Console.WriteLine("Сообщение уже существует, не отправляем повторно.");
                }

                // Получаем под-вопросы для заданного questionId
                var mainQuestions = await GetSubQuestions(questionId, tree);

                // Получаем все сообщения для данного uid
                var messages = await _apiRequest.ReadMSGAsync(uid);

                // Создаем модель для передачи в представление
                var chatViewModel = new ChatViewModel
                {
                    Messages = messages, // Передаем все сообщения
                    Uid = uid,
                    MainQuestions = mainQuestions
                };

                // Возвращаем обновленное представление
                return PartialView("_QuestionButtons", chatViewModel);
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