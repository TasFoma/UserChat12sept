using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserChat.Models;

namespace UserChat.Controllers
{
    public class ChatHandler
    {
        // Хранит активные WebSocket-клиенты
        private static readonly ConcurrentDictionary<WebSocket, string> Clients = new ConcurrentDictionary<WebSocket, string>();
        private readonly ChatContext _context; // Контекст базы данных

        public ChatHandler(ChatContext context)
        {
            _context = context;
        }

        // Обрабатывает WebSocket запросы
        public async Task HandleWebSocket(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Clients.TryAdd(webSocket, string.Empty); // Добавляем нового клиента
                await ReceiveMessages(webSocket); // Начинаем получать сообщения
            }
        }

        // Получает сообщения от клиента
        private async Task ReceiveMessages(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4]; // Буфер для получения сообщений

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // Закрываем соединение при получении сообщения о закрытии
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Clients.TryRemove(webSocket, out _); // Удаляем клиента из списка
                }
                else
                {
                    // Обрабатываем полученное сообщение
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var userName = message.Split(':')[0]; // Извлекаем имя пользователя
                    var userMessage = message.Substring(userName.Length + 1).Trim(); // Извлекаем само сообщение

                    // Создаем объект сообщения
                    var chatMessage = new ChatMessage
                    {
                        UserName = userName,
                        Message = userMessage,
                        Timestamp = DateTime.Now,
                        IsAdminResponse = false
                    };

                    // Сохраняем сообщение в базе данных
                    await SaveMessageToDatabase(chatMessage);

                    // Отправляем сообщение всем клиентам
                    await BroadcastMessage(buffer, result.Count);
                }
            }
        }

        // Сохраняет сообщение в базе данных
        private async Task SaveMessageToDatabase(ChatMessage chatMessage)
        {
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
        }

        // Рассылает сообщение всем подключенным клиентам
        private async Task BroadcastMessage(byte[] message, int count)
        {
            foreach (var client in Clients.Keys)
            {
                if (client.State == WebSocketState.Open)
                {
                    // Отправляем сообщение клиенту
                    await client.SendAsync(new ArraySegment<byte>(message, 0, count), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}