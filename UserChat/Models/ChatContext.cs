using Microsoft.EntityFrameworkCore;

namespace UserChat.Models
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; } // Связываем с таблицей chat

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>().ToTable("chat"); // Указываем имя существующей таблицы
        }
    }
    /*
    public class ChatMessage
    {
        public int Id { get; set; } // Поле идентификатора
        public string UserName { get; set; } // Имя пользователя
        public string Message { get; set; } // Сообщение
        public DateTime Timestamp { get; set; } // Время отправки сообщения
        public bool IsAdminResponse { get; set; } // Является ли ответом администратора
    }*/
}