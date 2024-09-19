namespace UserChat.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Data { get; set; }
        public UserType Type { get; set; }
        public string Uid { get; set; } // Уникальный идентификатор пользователя (если требуется)
    }

    public enum UserType
    {
        User,
        Admin
    }
}