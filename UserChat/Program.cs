using Microsoft.EntityFrameworkCore;
using UserChat;
using UserChat.Controllers;
using UserChat.Models;
using static UserChat.ApiRequest;
var builder = WebApplication.CreateBuilder(args);

// ��������� ������ ����������� � �������� ���� ������
builder.Services.AddDbContext<ChatContext>(options =>
    options.UseMySql("Server=localhost;Database=chatdatabase;User=root;Password=igdathun;",
        new MySqlServerVersion(new Version(10, 3, 11))));

//  MVC � WebSocket
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ChatHandler>();

var app = builder.Build();

// ��������� middleware
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

// �������� ��������� WebSocket
app.UseWebSockets();

// ������� ��� WebSocket
app.Map("/ws", async context =>
{
    var chatHandler = context.RequestServices.GetRequiredService<ChatHandler>();
    await chatHandler.HandleWebSocket(context);
});

app.UseAuthorization();

// ��������� ��������� ��� MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.Run();