FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Dzeta.TonWatcher/Dzeta.TonWatcher.csproj", "src/Dzeta.TonWatcher/"]
RUN dotnet restore "src/Dzeta.TonWatcher/Dzeta.TonWatcher.csproj"
COPY . .
WORKDIR "/src/src/Dzeta.TonWatcher"
RUN dotnet build "Dzeta.TonWatcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dzeta.TonWatcher.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dzeta.TonWatcher.dll"] 