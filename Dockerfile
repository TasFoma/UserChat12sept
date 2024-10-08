# Используем официальный образ .NET для ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# Используем официальный образ .NET для SDK
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

# Копируем файл проекта
COPY ["User Chat/User Chat.csproj", "User Chat/"]  # Убедитесь, что путь указан правильно
RUN dotnet restore "User Chat/User Chat.csproj"

# Копируем все остальные файлы
COPY ["User Chat/", "User Chat/"]  # Копируем все файлы проекта
WORKDIR "/src/User Chat"
RUN dotnet build "User Chat.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "User Chat.csproj" -c Release -o /app/publish

# Настраиваем финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User Chat.dll"]
