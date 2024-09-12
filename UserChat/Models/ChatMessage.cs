using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserChat.Models
{
    public class ChatMessage
    {
        public int Id { get; set; } // Поле идентификатора
        public string UserName { get; set; } // Имя пользователя
        public string Message { get; set; } // Сообщение
        public DateTime Timestamp { get; set; } // Время отправки сообщения
        public bool IsAdminResponse { get; set; } // Является ли ответом администратора
    }
}