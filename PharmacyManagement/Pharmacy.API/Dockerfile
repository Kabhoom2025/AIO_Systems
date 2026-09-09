FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Pharmacy.API/Pharmacy.API.csproj Pharmacy.API/
COPY Pharmacy.Application/Pharmacy.Application.csproj Pharmacy.Application/
COPY Pharmacy.Domain/Pharmacy.Domain.csproj Pharmacy.Domain/
COPY Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj Pharmacy.Infrastructure/
RUN dotnet restore Pharmacy.API/Pharmacy.API.csproj

COPY . .
RUN dotnet publish Pharmacy.API/Pharmacy.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Pharmacy.API.dll"]
