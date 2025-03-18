FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Afra-App/Afra-App.csproj", "Afra-App/"]
RUN dotnet restore "Afra-App/Afra-App.csproj"
COPY . .
WORKDIR "/src/Afra-App"
RUN dotnet build "Afra-App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Afra-App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["Afra-App/appsettings.json", "./"]
ENTRYPOINT ["dotnet", "Afra-App.dll"]
