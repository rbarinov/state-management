FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Events/Events.csproj", "Events/"]
RUN dotnet restore "Events/Events.csproj"
COPY . .
WORKDIR "/src/Events"
RUN dotnet build "Events.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Events.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Events.dll"]
