
FROM mcr.microsoft.com/dotnet/sdk
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out --no-restore

ENTRYPOINT dotnet out/CoinReleaseMonitor.dll
