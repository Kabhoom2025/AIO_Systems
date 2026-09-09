FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FoodOrder.API/FoodOrder.API.csproj FoodOrder.API/
COPY FoodOrder.Application/FoodOrder.Application.csproj FoodOrder.Application/
COPY FoodOrder.Domain/FoodOrder.Domain.csproj FoodOrder.Domain/
COPY FoodOrder.Infrastructure/FoodOrder.Infrastructure.csproj FoodOrder.Infrastructure/
COPY FoodOrder.Shared/FoodOrder.Shared.csproj FoodOrder.Shared/
RUN dotnet restore FoodOrder.API/FoodOrder.API.csproj

COPY . .
RUN dotnet publish FoodOrder.API/FoodOrder.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FoodOrder.API.dll"]
