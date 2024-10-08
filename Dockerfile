# Используем официальный образ .NET для ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Используем официальный образ .NET для SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта
COPY ["UserChat/UserChat.csproj", "UserChat/"]
RUN dotnet restore "UserChat/UserChat.csproj"

# Копируем все остальные файлы
COPY ["UserChat/", "UserChat/"]
WORKDIR "/src/UserChat"
RUN dotnet build "UserChat.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "UserChat.csproj" -c Release -o /app/publish

# Настраиваем финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserChat.dll"]
