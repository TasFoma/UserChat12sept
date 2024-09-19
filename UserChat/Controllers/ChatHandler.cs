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
        private static readonly ConcurrentDictionary<WebSocket, string> Clients = new ConcurrentDictionary<WebSocket, string>();
        private readonly ChatContext _context;

        // Конструктор, принимающий контекст базы данных
        public ChatHandler(ChatContext context)
        {
            _context = context;
        }

        public async Task HandleWebSocket(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Clients.TryAdd(webSocket, string.Empty);

                await ReceiveMessages(webSocket);
            }
        }

        private async Task ReceiveMessages(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Clients.TryRemove(webSocket, out _);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var userName = message.Split(':')[0]; // Извлекаем имя пользователя из сообщения
                    var userMessage = message.Substring(userName.Length + 1).Trim(); // Извлекаем само сообщение

                    // Создаем объект сообщения
                    var chatMessage = new ChatMessage
                    {
                        UserName = userName, // Используем извлеченное имя пользователя
                        Message = userMessage, // Используем извлеченное сообщение
                        Data = DateTime.Now, // Добавляем временную метку
                        Type = UserType.User
                    };

                    // Сохраняем сообщение в базе данных
                    await SaveMessageToDatabase(chatMessage);

                    // Отправляем сообщение всем клиентам
                    await BroadcastMessage(buffer, result.Count);
                }
            }
        }

        private async Task SaveMessageToDatabase(ChatMessage chatMessage)
        {
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
        }

        private async Task BroadcastMessage(byte[] message, int count)
        {
            foreach (var client in Clients.Keys)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(message, 0, count), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}