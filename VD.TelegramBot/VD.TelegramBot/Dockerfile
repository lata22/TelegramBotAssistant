#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
# Copy each .csproj and restore as distinct layers
COPY ["VD.TelegramBot/VD.TelegramBot.csproj", "VD.TelegramBot/"]
COPY ["TelegramBot/TelegramBot.csproj", "TelegramBot/"]
COPY ["VD.TelegramBot.Db/VD.TelegramBot.Db.csproj", "VD.TelegramBot.Db/"]

# Restore for all projects
RUN dotnet restore "VD.TelegramBot/VD.TelegramBot.csproj"

# Copy all source code and build
COPY . .
WORKDIR "/src/VD.TelegramBot"
RUN dotnet build "VD.TelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VD.TelegramBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VD.TelegramBot.dll"]