# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Suwayami.csproj", "."]
RUN dotnet restore "Suwayami.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Suwayami.csproj" -c Release -o /app/build

# Этап публикации
FROM build AS publish
RUN dotnet publish "Suwayami.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный этап runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Suwayami.dll"]
