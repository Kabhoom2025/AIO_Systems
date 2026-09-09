FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FlowSphere.API/FlowSphere.API.csproj FlowSphere.API/
COPY FlowSphere.Application/FlowSphere.Application.csproj FlowSphere.Application/
COPY FlowSphere.Domain/FlowSphere.Domain.csproj FlowSphere.Domain/
COPY FlowSphere.Infrastructure/FlowSphere.Infrastructure.csproj FlowSphere.Infrastructure/
COPY FlowSphere.Execution/FlowSphere.Execution.csproj FlowSphere.Execution/
RUN dotnet restore FlowSphere.API/FlowSphere.API.csproj

COPY . .
RUN dotnet publish FlowSphere.API/FlowSphere.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FlowSphere.API.dll"]
