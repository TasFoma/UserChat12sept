using Microsoft.EntityFrameworkCore;
using UserChat.Models;

namespace UserChat.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } // Набор данных для пользователей
        public DbSet<ChatMessage> ChatMessages { get; set; } // Набор данных для сообщений
    }
}