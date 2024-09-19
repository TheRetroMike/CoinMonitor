
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet build -c Release

ENV WEBHOOK=xxx

ENTRYPOINT dotnet bin/Release/net6.0/CoinReleaseMonitor.dll ${WEBHOOK}
