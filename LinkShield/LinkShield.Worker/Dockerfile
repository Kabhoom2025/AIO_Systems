FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LinkShield.Worker/LinkShield.Worker.csproj LinkShield.Worker/
COPY LinkShield.Application/LinkShield.Application.csproj LinkShield.Application/
COPY LinkShield.Domain/LinkShield.Domain.csproj LinkShield.Domain/
COPY LinkShield.Infrastructure/LinkShield.Infrastructure.csproj LinkShield.Infrastructure/
RUN dotnet restore LinkShield.Worker/LinkShield.Worker.csproj

COPY . .
RUN dotnet publish LinkShield.Worker/LinkShield.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LinkShield.Worker.dll"]
