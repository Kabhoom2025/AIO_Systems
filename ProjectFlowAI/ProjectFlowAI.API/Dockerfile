FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ProjectFlowAI.API/ProjectFlowAI.API.csproj ProjectFlowAI.API/
COPY ProjectFlowAI.Application/ProjectFlowAI.Application.csproj ProjectFlowAI.Application/
COPY ProjectFlowAI.Domain/ProjectFlowAI.Domain.csproj ProjectFlowAI.Domain/
COPY ProjectFlowAI.Infrastructure/ProjectFlowAI.Infrastructure.csproj ProjectFlowAI.Infrastructure/
RUN dotnet restore ProjectFlowAI.API/ProjectFlowAI.API.csproj

COPY . .
RUN dotnet publish ProjectFlowAI.API/ProjectFlowAI.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjectFlowAI.API.dll"]
