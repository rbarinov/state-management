﻿FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StateManagement/StateManagement.csproj", "StateManagement/"]
RUN dotnet restore "StateManagement/StateManagement.csproj"
COPY . .
WORKDIR "/src/StateManagement"
RUN dotnet build "StateManagement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StateManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "StateManagement.dll"]
