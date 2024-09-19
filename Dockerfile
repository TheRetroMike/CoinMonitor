﻿FROM mcr.microsoft.com/dotnet/sdk as build
WORKDIR /app

COPY *.csproj ./
COPY NuGet.Config ./
RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out --no-restore

ENTRYPOINT dotnet CoinReleaseMonitor.dll