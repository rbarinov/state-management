FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["EventImporter/EventImporter.csproj", "EventImporter/"]
RUN dotnet restore "EventImporter/EventImporter.csproj"
COPY . .
WORKDIR "/src/EventImporter"
RUN dotnet build "EventImporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventImporter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventImporter.dll"]
