FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StateManagement.EventStoreV5.Importer/StateManagement.EventStoreV5.Importer.csproj", "StateManagement.EventStoreV5.Importer/"]
RUN dotnet restore "StateManagement.EventStoreV5.Importer/StateManagement.EventStoreV5.Importer.csproj"
COPY . .
WORKDIR "/src/StateManagement.EventStoreV5.Importer"
RUN dotnet build "StateManagement.EventStoreV5.Importer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StateManagement.EventStoreV5.Importer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "StateManagement.EventStoreV5.Importer.dll"]
