FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY NovaERP.API/NovaERP.API.csproj NovaERP.API/
COPY NovaERP.Application/NovaERP.Application.csproj NovaERP.Application/
COPY NovaERP.Domain/NovaERP.Domain.csproj NovaERP.Domain/
COPY NovaERP.Infrastructure/NovaERP.Infrastructure.csproj NovaERP.Infrastructure/
RUN dotnet restore NovaERP.API/NovaERP.API.csproj

COPY . .
RUN dotnet publish NovaERP.API/NovaERP.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NovaERP.API.dll"]
