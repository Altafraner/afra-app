FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
COPY ["Afra-App/Afra-App.csproj", "/src/Afra-App/"]
WORKDIR /src/Afra-App
RUN dotnet restore "Afra-App.csproj" \
    --runtime linux-musl-x64
COPY ./Afra-App /src/Afra-App
RUN dotnet publish "Afra-App.csproj" \
    -c release \
    --no-restore \
    --runtime linux-musl-x64 \
    --self-contained true /p:PublishSingleFile=true \
    -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS run
RUN apk add --no-cache openldap
EXPOSE 8080
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./Afra-App"]
