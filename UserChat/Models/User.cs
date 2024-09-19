namespace UserChat.Models
{
    public class User
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public UserType Type { get; set; } // Добавляем поле для типа пользователя
    }
}