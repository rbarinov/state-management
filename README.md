# State Management

## Run Host

```
docker pull ghcr.io/rbarinov/state-management:latest

docker run -ti --rm \
    -p 8080:8080 \
    -e "ConnectionStrings:Default=Host=host.docker.internal;Username=postgres;Password=postgres;Database=state_management" \
    ghcr.io/rbarinov/state-management:latest
```

## EventStoreV5 Import CLI


```
docker pull ghcr.io/rbarinov/state-management/eventstorev5-import-cli:latest

docker run -ti --rm \
    -e "Configuration:ConnectionString=ConnectTo=tcp://admin:changeit@192.168.1.165:1113; HeartBeatTimeout=500" \
    -e "Configuration:ReadChannelCapacity=2048" \
    -e "Configuration:BufferSize=1024" \
    -e "Configuration:MaxLiveQueueSize=128" \
    -e "Configuration:ReadBatchSize=128" \
    -e "Configuration:StateManagementUrl=http://host.docker.internal:8080" \
    -e "Configuration:WriteThreads=16" \
    -e "Configuration:WriteBatchSize=100" \
    ghcr.io/rbarinov/state-management/eventstorev5-import-cli:latest
```

## SQL Init

Init database

```
# dotnet tool install --global dotnet-ef --prerelease

cd StateManagement

dotnet ef database update --connection "Host=localhost;Username=postgres;Password=postgres;Database=state_management"
```
