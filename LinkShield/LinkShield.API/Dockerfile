FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LinkShield.API/LinkShield.API.csproj LinkShield.API/
COPY LinkShield.Application/LinkShield.Application.csproj LinkShield.Application/
COPY LinkShield.Domain/LinkShield.Domain.csproj LinkShield.Domain/
COPY LinkShield.Infrastructure/LinkShield.Infrastructure.csproj LinkShield.Infrastructure/
RUN dotnet restore LinkShield.API/LinkShield.API.csproj

COPY . .
RUN dotnet publish LinkShield.API/LinkShield.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LinkShield.API.dll"]
