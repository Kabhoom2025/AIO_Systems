FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Chatbot.API/Chatbot.API.csproj", "Chatbot.API/"]
COPY ["Chatbot.Application/Chatbot.Application.csproj", "Chatbot.Application/"]
COPY ["Chatbot.Infrastructure/Chatbot.Infrastructure.csproj", "Chatbot.Infrastructure/"]
COPY ["Chatbot.Domain/Chatbot.Domain.csproj", "Chatbot.Domain/"]
RUN dotnet restore "Chatbot.API/Chatbot.API.csproj"
COPY . .
WORKDIR /src/Chatbot.API
RUN dotnet build "Chatbot.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chatbot.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chatbot.API.dll"]
