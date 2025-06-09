# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoreBookingPlatform.AdapterService/CoreBookingPlatform.AdapterService.csproj", "CoreBookingPlatform.AdapterService/"]
COPY ["CoreBookingPlatform.ProductService/CoreBookingPlatform.ProductService.csproj", "CoreBookingPlatform.ProductService/"]
RUN dotnet restore "./CoreBookingPlatform.AdapterService/CoreBookingPlatform.AdapterService.csproj"
COPY . .
WORKDIR "/src/CoreBookingPlatform.AdapterService"
RUN dotnet build "./CoreBookingPlatform.AdapterService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Temporarily rename ProductService's appsettings files to prevent inclusion in publish output
RUN mv /src/CoreBookingPlatform.ProductService/appsettings.json /src/CoreBookingPlatform.ProductService/appsettings.json.bak || true
RUN mv /src/CoreBookingPlatform.ProductService/appsettings.Development.json /src/CoreBookingPlatform.ProductService/appsettings.Development.json.bak || true
RUN dotnet publish "./CoreBookingPlatform.AdapterService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_URLS=http://*:8080
ENTRYPOINT ["dotnet", "CoreBookingPlatform.AdapterService.dll"]