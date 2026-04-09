FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["MusicMigrater/MusicMigrater.csproj", "MusicMigrater/"]
COPY ["MusicMigrater.BLL/MusicMigrater.BLL.csproj", "MusicMigrater.BLL/"]
COPY ["MusicMigrater.DAL/MusicMigrater.DAL.csproj", "MusicMigrater.DAL/"]
COPY ["MusicMigrater.Domain/MusicMigrater.Domain.csproj", "MusicMigrater.Domain/"]
RUN dotnet restore "./MusicMigrater/MusicMigrater.csproj"
COPY . .
WORKDIR "/src/MusicMigrater"
RUN dotnet build "./MusicMigrater.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MusicMigrater.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicMigrater.dll"]