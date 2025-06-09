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
COPY ["CoreBookingPlatform.OrderService/CoreBookingPlatform.OrderService.csproj", "CoreBookingPlatform.OrderService/"]
COPY ["CoreBookingPlatform.CartService/CoreBookingPlatform.CartService.csproj", "CoreBookingPlatform.CartService/"]
COPY ["CoreBookingPlatform.AdapterService/CoreBookingPlatform.AdapterService.csproj", "CoreBookingPlatform.AdapterService/"]
COPY ["CoreBookingPlatform.ProductService/CoreBookingPlatform.ProductService.csproj", "CoreBookingPlatform.ProductService/"]
RUN dotnet restore "./CoreBookingPlatform.OrderService/CoreBookingPlatform.OrderService.csproj"
COPY . .
WORKDIR "/src/CoreBookingPlatform.OrderService"
RUN dotnet build "./CoreBookingPlatform.OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Rename cart-service appsettings files to prevent them from being included in the publish output
RUN mv /src/CoreBookingPlatform.CartService/appsettings.json /src/CoreBookingPlatform.CartService/appsettings.json.bak || true
RUN mv /src/CoreBookingPlatform.CartService/appsettings.Development.json /src/CoreBookingPlatform.CartService/appsettings.Development.json.bak || true
RUN dotnet publish "./CoreBookingPlatform.OrderService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_URLS=http://*:8080
ENTRYPOINT ["dotnet", "CoreBookingPlatform.OrderService.dll"]