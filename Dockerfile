# Используем официальный образ .NET для ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# Используем официальный образ .NET для SDK
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["UserChat.csproj", "./"]
RUN dotnet restore "./UserChat.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "UserChat.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "UserChat.csproj" -c Release -o /app/publish

# Настраиваем финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserChat.dll"]