using Microsoft.EntityFrameworkCore;
using UserChat;
using UserChat.Controllers;
using UserChat.Models;
using static UserChat.ApiRequest;
var builder = WebApplication.CreateBuilder(args);

// Добавляем строку подключения и контекст базы данных
builder.Services.AddDbContext<ChatContext>(options =>
    options.UseMySql("Server=localhost;Database=chatdatabase;User=root;Password=igdathun;",
        new MySqlServerVersion(new Version(10, 3, 11))));

//  MVC и WebSocket
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ChatHandler>();

var app = builder.Build();

// Настройка middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Включаем поддержку WebSocket
app.UseWebSockets();

// Маршрут для WebSocket
app.Map("/ws", async context =>
{
    var chatHandler = context.RequestServices.GetRequiredService<ChatHandler>();
    await chatHandler.HandleWebSocket(context);
});

app.UseAuthorization();

// Настройка маршрутов для MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.Run();