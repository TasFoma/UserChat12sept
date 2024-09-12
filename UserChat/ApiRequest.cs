using MySqlConnector;
using System;
using System.Collections.Generic;
using UserChat.Models;

namespace UserChat
{
    public class ApiRequest
    {
        private MySqlConnection _connection = new MySqlConnection("Server=localhost;Database=chatdatabase;User=root;Password=igdathun;");

        public void InsertMSG(int id_user, string msg, string type_user)
        {
            // Открываем соединение
            _connection.Open();
            try
            {
                MySqlCommand command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO chatdatabase.chat(UserName, Message, type) VALUES (@a1, @a2, @a3)";
                command.Parameters.AddWithValue("@a1", id_user);
                command.Parameters.AddWithValue("@a2", msg);
                command.Parameters.AddWithValue("@a3", type_user);
                command.ExecuteNonQuery();
            }
            finally
            {
                // Закрываем соединение
                _connection.Close();
            }
        }

        public class Chat
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsAdminResponse { get; set; }
        }

        public List<Chat> ReadMSG(string uid)
        {
            List<Chat> messages = new List<Chat>();

            // Открываем соединение
            _connection.Open();
            try
            {
                MySqlCommand command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM chatdatabase.chat WHERE `uid` = @a1";
                command.Parameters.AddWithValue("@a1", uid); 

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Chat message = new Chat
                        {
                            Id = reader.GetInt32("Id"),
                            UserName = reader.GetString("UserName"),
                            Message = reader.GetString("Message"),
                            Timestamp = reader.GetDateTime("data"),
                            IsAdminResponse = reader.GetBoolean("uid")
                        };
                        messages.Add(message);
                    }
                }
            }
            finally
            {
                // Закрываем соединение
                _connection.Close();
            }

            return messages;
        }
    }
}