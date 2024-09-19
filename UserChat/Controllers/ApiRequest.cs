using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserChat.Models;

namespace UserChat
{
    public class ApiRequest
    {

        private readonly string _connectionString = "Server=localhost;Database=chatdatabase;User=root;Password=igdathun;";

        // Метод для получения главных вопросов
        public async Task<List<MainQuestion>> GetMainQuestionsAsync()
        {
            var mainQuestions = new List<MainQuestion>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name FROM mainquestions WHERE tree = 0";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var question = new MainQuestion
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name")
                            };
                            mainQuestions.Add(question);
                        }
                    }
                }
            }

            return mainQuestions;
        }
        // Метод для проверки существования сообщения в таблице chat
        public async Task<bool> CheckIfMessageExistsAsync(string uid, string message)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM chat WHERE uid = @uid AND message = @message";
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@message", message);

                    // Получаем количество существующих записей
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());

                    // Если count больше 0, значит сообщение существует
                    return count > 0;
                }
            }
        }

        // Метод для получения под-вопросов по ID дерева и уровню дерева
        public async Task<List<Question>> GetQuestionsByIdTreeAsync(int idTree, int tree)
        {
            var questions = new List<Question>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    // Изменяем SQL-запрос, чтобы учитывать уровень дерева
                    command.CommandText = "SELECT id, name, idTree FROM question WHERE idTree = @idTree AND tree = @tree"; // Добавляем условие для tree
                    command.Parameters.AddWithValue("@idTree", idTree);
                    command.Parameters.AddWithValue("@tree", tree);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var question = new Question
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                IdTree = reader.GetInt32("idTree")
                            };
                            questions.Add(question);
                        }
                    }
                }
            }

            return questions; // Возвращаем список вопросов
        }

        // Метод для получения сообщений для пользователя
        public async Task<List<ChatMessage>> ReadMSGAsync(string uid)
        {
            var messages = new List<ChatMessage>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM chat WHERE uid = @uid ORDER BY data";
                    command.Parameters.AddWithValue("@uid", uid);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var msg = new ChatMessage
                            {
                                UserName = reader.GetString("userName"),
                                Message = reader.GetString("message"),
                                Data = reader.GetDateTime("data")
                            };
                            messages.Add(msg);
                        }
                    }
                }
            }

            return messages;
        }

        // Метод для вставки сообщения в таблицу chat
        public async Task InsertMSGAsync(string userName, string msg, string uid)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO chat (userName, message, uid, data) VALUES (@userName, @message, @uid, @data)";
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@message", msg);
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@data", DateTime.Now);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Метод для вставки системного сообщения
        public async Task InsertMessageAsync(string userName, string msg, string uid)
        {
            await InsertMSGAsync(userName, msg, uid); // Используем метод InsertMSGAsync для вставки сообщения
        }

        // Метод для добавления нового пользователя
        public async Task<string> AddUserAsync(string name, string phone)
        {
            string uid = Guid.NewGuid().ToString(); // Генерация уникального идентификатора
            string type = "user"; // Тип пользователя
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO users (uid, name, phone, type) VALUES (@uid, @name, @phone, @type)";
                        command.Parameters.AddWithValue("@uid", uid);
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@phone", phone);
                        command.Parameters.AddWithValue("@type", type);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
                    return null;
                }
            }

            return uid; // Возвращаем сгенерированный uid
        }
        // Метод для получения пользователя по номеру телефона
        public async Task<User> GetUserByPhoneAsync(string phone)
        {
            User user = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM users WHERE phone = @phone";
                        command.Parameters.AddWithValue("@phone", phone);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                user = new User
                                {
                                    Uid = reader.GetString("uid"),
                                    Name = reader.GetString("name"),
                                    Phone = reader.GetString("phone"),
                                    Type = (UserType)Enum.Parse(typeof(UserType), reader.GetString("type")) // Преобразуем строку в UserType
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении пользователя по номеру телефона: {ex.Message}");
                }
            }

            return user; // Возвращаем найденного пользователя или null
        }

        // Метод для получения пользователей по номеру телефона
        public async Task<List<User>> GetUsersByPhoneAsync(string phone)
        {
            List<User> users = new List<User>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM users WHERE phone = @phone";
                        command.Parameters.AddWithValue("@phone", phone);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                users.Add(new User
                                {
                                    Uid = reader.GetString("uid"),
                                    Name = reader.GetString("name"),
                                    Phone = reader.GetString("phone")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении пользователей: {ex.Message}");
                }
            }

            return users;
        }


        public async Task InsertWelcomeMessageAsync(string uid)
        {
            string welcomeMessage = "Здравствуйте, Вас приветствует компания Инвинтро, чем я могу помочь?";
            string userName = "Система"; // Имя отправителя приветственного сообщения

            // Вставляем сообщение в таблицу chat
            await InsertMSGAsync(userName, welcomeMessage, uid);
        }

        public async Task SystemMessageAsync(string uid, string sysMes)
        {
            string userName = "Система"; // Имя отправителя приветственного сообщения

            // Вставляем сообщение в таблицу chat
            await InsertMSGAsync(userName, sysMes, uid);
        }

        // Метод для проверки существования пользователя по uid
        public async Task<User> GetUserByUidAsync(string uid)
        {
            User user = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users WHERE uid = @uid";
                    command.Parameters.AddWithValue("@uid", uid);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                Uid = reader.GetString("uid"),
                                Name = reader.GetString("name"),
                                Phone = reader.GetString("phone"),
                                Type = (UserType)Enum.Parse(typeof(UserType), reader.GetString("type")) // Преобразуем строку в UserType
                            };
                        }
                    }
                }
            }

            return user; // Возвращаем найденного пользователя или null
        }

        // Метод для проверки существования пользователя по номеру телефона
        public async Task<bool> UserExistsAsync(string phone)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM users WHERE phone = @phone";
                    command.Parameters.AddWithValue("@phone", phone);

                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0; // Возвращаем true, если пользователь существует
                }
            }
        }
    }
}