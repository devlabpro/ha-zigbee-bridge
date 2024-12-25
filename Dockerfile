FROM mcr.microsoft.com/dotnet/sdk:9.0
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZigbeeBridgeAddon/ZigbeeBridgeAddon/ZigbeeBridgeAddon.csproj", "ZigbeeBridgeAddon/ZigbeeBridgeAddon/"]
COPY ["ZigbeeBridgeAddon.SerialClient/ZigbeeBridgeAddon.SerialClient.csproj", "ZigbeeBridgeAddon.SerialClient/"]
RUN dotnet restore "./ZigbeeBridgeAddon/ZigbeeBridgeAddon/ZigbeeBridgeAddon.csproj"
COPY . .
WORKDIR "/src/ZigbeeBridgeAddon/ZigbeeBridgeAddon"
RUN dotnet build "./ZigbeeBridgeAddon.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ZigbeeBridgeAddon.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM publish AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZigbeeBridgeAddon.dll"]
