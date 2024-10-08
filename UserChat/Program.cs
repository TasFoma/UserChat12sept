using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using UserChat.Data;
using MySqlConnector;
using UserChat;

var builder = WebApplication.CreateBuilder(args);

// ���������� ��������� ���� ������
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)))); // ���������, ��� � ��� ���������� ������ MySQL

// ����������� ApiRequest ��� �������
builder.Services.AddScoped<ApiRequest>();

// ���������� ������������ � ���������������
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ��������� ��������� ��������� ��������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // ��������� ��������� ������
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ��������� �������������

app.UseEndpoints(endpoints =>
{
    // ��������� ���������
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Chat}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "login",
        pattern: "Login",
        defaults: new { controller = "User", action = "Login" });

    endpoints.MapControllerRoute(
        name: "createUser",
        pattern: "CreateUser",
        defaults: new { controller = "User", action = "CreateUser" });

    endpoints.MapControllerRoute(
        name: "adminChat",
        pattern: "AdminChat/{uid?}",
        defaults: new { controller = "AdminChat", action = "Index" });
});
app.Run();