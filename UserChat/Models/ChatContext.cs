using Microsoft.EntityFrameworkCore;

namespace UserChat.Models
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>().ToTable("chat"); //  имя существующей таблицы
        }
    }
/*
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsAdminResponse { get; set; }
    }*/
}