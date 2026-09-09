FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY HRMS.API/HRMS.API.csproj HRMS.API/
COPY HRMS.Application/HRMS.Application.csproj HRMS.Application/
COPY HRMS.Domain/HRMS.Domain.csproj HRMS.Domain/
COPY HRMS.Infrastructure/HRMS.Infrastructure.csproj HRMS.Infrastructure/
RUN dotnet restore HRMS.API/HRMS.API.csproj

COPY . .
RUN dotnet publish HRMS.API/HRMS.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HRMS.API.dll"]
