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
    --self-contained \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS run
EXPOSE 8080
RUN apk add --no-cache libldap && \
    mkdir /app && \
    ln -s /usr/lib/libldap.so.2 /app/libldap-2.5.so.0
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./Afra-App"]
