using System.Collections.Generic;

namespace UserChat.Models
{
    public class ChatViewModel
    {
        public List<ChatMessage> Messages { get; set; } // Сообщения в чате
        public string Uid { get; set; } // Уникальный идентификатор пользователя
        public List<MainQuestion> MainQuestions { get; set; } // Главные вопросы
        public List<Question> RelatedQuestions { get; set; } // Связанные вопросы (если нужно)
        public string GuestUid { get; set; } // Добавлено свойство для uid гостя
        public string GuestName { get; set; } // Имя гостя

        // Это новое свойство для хранения последних сообщений
        public List<ChatMessage> RecentMessages => Messages.TakeLast(5).ToList(); // Например, последние 5 сообщений


    }
    public class QuestionButtonsViewModel
    {
        public List<MainQuestion> MainQuestions { get; set; }
        public string Uid { get; set; }
    }
    public class MainQuestion
    {
        public int Id { get; set; } // ID вопроса
        public string Name { get; set; } // Имя вопроса (то, что выводится в чат)
        public int Tree { get; set; } // уровень
        public List<MainQuestion> SubQuestions { get; set; } = new List<MainQuestion>();
    }
    public class QuestionViewModel
    {
        public List<MainQuestion> MainQuestions { get; set; }

        // Вы можете добавить дополнительные свойства, если нужно
        public string SelectedQuestionName { get; set; }
    }

       
    public class Question
    {
        public int Id { get; set; } // ID вопроса
        public string Name { get; set; } // Имя вопроса
        public int Tree { get; set; } // уровень
        public int IdTree { get; set; } // Связь с предыдущим вопросом
        public List<Question> SubQuestions { get; set; } = new List<Question>();  // Под-вопросыс
    }
}