# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for dependency caching
COPY ["App.Contracts/App.Contracts.csproj", "App.Contracts/"]
COPY ["App.Domain/App.Domain.csproj", "App.Domain/"]
COPY ["App.Application/App.Application.csproj", "App.Application/"]
COPY ["App.Infrastructure/App.Infrastructure.csproj", "App.Infrastructure/"]
COPY ["App.Persistence/App.Persistence.csproj", "App.Persistence/"]
COPY ["App.WebApi/App.WebApi.csproj", "App.WebApi/"]
COPY ["App.Application.Tests/App.Application.Tests.csproj", "App.Application.Tests/"]
COPY ["RealEstateMarketplace.slnx", "./"]

RUN dotnet restore "RealEstateMarketplace.slnx"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/App.WebApi"
RUN dotnet publish "App.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "App.WebApi.dll"]
