FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
COPY ["Afra-App/Afra-App.csproj", "/src/Afra-App/"]
WORKDIR /src/Afra-App
RUN dotnet restore "Afra-App.csproj" \
    --runtime linux-x64
COPY ./Afra-App /src/Afra-App
RUN dotnet publish "Afra-App.csproj" \
    -c release \
    --no-restore \
    --self-contained \
    --runtime linux-x64 \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0 AS run
EXPOSE 8080
RUN apt-get update && apt-get -y install libldap-2.5-0
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./Afra-App"]
