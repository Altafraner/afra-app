# syntax=docker/dockerfile:1-labs

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/
COPY --link Afra-App.slnx Directory.Build.props Directory.Packages.props ./
COPY --link --parents **/*.csproj ./
RUN dotnet restore "Afra-App.slnx" \
    --runtime linux-x64
COPY ./ ./
RUN dotnet publish "Altafraner.AfraApp/Altafraner.AfraApp.csproj" \
    -c release \
    --no-restore \
    --self-contained \
    --runtime linux-x64 \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS run
EXPOSE 8080
RUN apt-get update && apt-get -y install libldap2
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./Afra-App"]
