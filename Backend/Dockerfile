# syntax=docker/dockerfile:1-labs

FROM rust:trixie AS rust-build
WORKDIR /rust-src
COPY Altafraner.Typst/typst_with_bindings/Cargo.toml Altafraner.Typst/typst_with_bindings/Cargo.toml
COPY Altafraner.Typst/typst_with_bindings/Cargo.lock  Altafraner.Typst/typst_with_bindings/Cargo.lock
COPY Altafraner.Typst/typst_with_bindings/ Altafraner.Typst/typst_with_bindings/
WORKDIR /rust-src/Altafraner.Typst/typst_with_bindings/
RUN --mount=type=cache,target=/usr/local/cargo/registry \
    --mount=type=cache,target=/usr/local/cargo/git \
    --mount=type=cache,target=/rust-src/target \
    cargo build --release

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/
COPY --link Afra-App.slnx Directory.Build.props Directory.Packages.props ./
COPY --link --parents **/*.csproj ./
RUN dotnet restore "Afra-App.slnx" \
    --runtime linux-x64
COPY ./ ./
COPY --from=rust-build /rust-src/Altafraner.Typst/typst_with_bindings/Typst.g.cs                               /src/Altafraner.Typst/typst_with_bindings/Typst.g.cs
COPY --from=rust-build /rust-src/Altafraner.Typst/typst_with_bindings/target/release/libtypst_with_bindings.so /src/Altafraner.Typst/typst_with_bindings/target/release/libtypst_with_bindings.so
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
ENTRYPOINT ["./Altafraner.AfraApp"]
