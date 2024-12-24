ARG BUILD_FROM
FROM $BUILD_FROM
USER $APP_UID
WORKDIR /app
EXPOSE 5546


# Этот этап используется для сборки проекта службы
FROM $BUILD_FROM AS build
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
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZigbeeBridgeAddon.dll"]