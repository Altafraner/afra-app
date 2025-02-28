# Afra-App

## Überblick
Dies ist ein Programm, welches am [Sächsischen Landesgymnasium Sankt-Afra](https://sankt-afra.de) die Einwahl in die Nachmittagsangebote ermöglichen soll.

## Entwicklungsumgebung
Das Projekt besteht aus einer Vue-SPA im Ordner `/WebClient` und einer ASP.NET Core WebAPI im Ordner `/Afra-App`. In Zukunft würde es sich anbieten, die Teile zu trennen.

Zum Aufsetzen muss in den Ordner `/dev` navigiert und (z. B. mit dem WSL) das Skript `create_dev.sh` ausgeführt werden.

Dieses erstellt Zertifikate zur Verschlüsselung, sowie für den SAML IdP und SP.

Mit `docker-compose up -d` bzw. `podman-compose up -d` kann eine Entwicklungsumgebung mit einem Test-IdP (Port 4000) und Postgres (Port 5432) gestartet werden.

Die WebApi und die Client-App müssen jeweils eigenständig gestartet werden. (`npm run dev` und `dotnet run`)

Die Entwicklungsumgebung setzt die Datenbank bei jedem Neustart zurück. Durch einen Aufruf von `http://localhost:5043/api/test/seed` kann ein Grundbestand an Daten angelegt werden.

## Endpunkte zum Testen:
- `http://localhost:5030/api/test/seed` (Datenbank füllen)
- `http://localhost:5030/api/test/authenticate/{guid}` (Als Nutzer mit `guid` einloggen)
- `http://localhost:5030/api/test/reset` (Datenbank zurücksetzen)