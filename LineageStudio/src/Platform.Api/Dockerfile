FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Platform.Api/Platform.Api.csproj src/Platform.Api/
COPY src/Platform.Application/Platform.Application.csproj src/Platform.Application/
COPY src/Platform.Domain/Platform.Domain.csproj src/Platform.Domain/
COPY src/Platform.Infrastructure/Platform.Infrastructure.csproj src/Platform.Infrastructure/
COPY src/Platform.Runtime/Platform.Runtime.csproj src/Platform.Runtime/
COPY src/Platform.Lineage/Platform.Lineage.csproj src/Platform.Lineage/
RUN dotnet restore src/Platform.Api/Platform.Api.csproj

COPY . .
RUN dotnet publish src/Platform.Api/Platform.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Platform.Api.dll"]
