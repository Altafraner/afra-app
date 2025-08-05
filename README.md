# Afra-App

![backend build](https://github.com/Altafraner/afra-app/actions/workflows/backend.yml/badge.svg)
![frontend build](https://github.com/Altafraner/afra-app/actions/workflows/webclient.yml/badge.svg)
![docker image build](https://github.com/Altafraner/afra-app/actions/workflows/docker.yml/badge.svg)

## Überblick
Dies ist ein Programm, welches am [Sächsischen Landesgymnasium Sankt-Afra](https://sankt-afra.de) die Einwahl in die Nachmittagsangebote ermöglichen soll.

## Entwicklungsumgebung
Das Projekt besteht aus einer Vue-SPA im Ordner `/WebClient` und einer ASP.NET Core WebAPI im Ordner `/Afra-App`. In Zukunft würde es sich anbieten, die Teile zu trennen.

Zum Aufsetzen muss in den Ordner `/dev` navigiert und (z. B. mit dem WSL) das Skript `create_dev.sh` ausgeführt werden.

Dieses erstellt Zertifikate zur Verschlüsselung.

Mit `docker-compose up -d` bzw. `podman-compose up -d` kann eine Entwicklungsumgebung mit Postgres (Port 5432) und
Mailcatcher (Port 8090) gestartet werden.

Die WebApi und die Client-App müssen jeweils eigenständig aus ihren entsprechenden Ordnern heraus gestartet werden. (in `WebClient`: `npm run dev` und in `Afra-App` `dotnet run`)

Die Entwicklungsumgebung setzt die Datenbank bei jedem Neustart zurück. Durch einen Aufruf von `http://localhost:5043/api/test/seed` kann ein Grundbestand an Daten angelegt werden.

### Endpunkte zum Testen:
- `http://localhost:5173/api/test/seed` (Datenbank füllen)
- `http://localhost:5173/api/test/seed/users` (Datenbank füllen)
- `http://localhost:5173/api/test/reset` (Datenbank zurücksetzen)

### Hinweise zum Login:
Normalerweise wird zur Authentifizierung ein Microsoft AD mit LDAP verwendet. In Zukunft soll evtl. SAML zum Einsatz kommen. Da beides zum Testen unpraktisch ist, kann LDAP in der `appsettings(.Development).json` deaktiviert werden. Für diesen Fall erfolgt der Login mit beliebigem Passwort und dem Anfang einer E-Mail-Adresse. Am besten liest man einfach einmal die Datenbank aus um die über den `/api/test/seed/users`-Endpunkt generierten Nutzer einzusehen.

# Lizenz:
Dieses Programm wird unter der [AGPL Version 3.0](LICENSE) zur Verfügung gestellt.

## Ausnahmen:
Die folgenden Dateien bzw. alle Dateien in den folgenden Ordnern sind nicht unter der genannten Lizenz lizenziert und dürfen nicht ohne explizite Erlaubnis der Urheber verwendet werden:
- `/WebClient/public/vdaa/`

Diese Dateien sind ggf. urheberrechtlich geschützt und dürfen nicht ohne ausdrückliche Genehmigung des jeweiligen Rechteinhabers verwendet oder verändert werden. Für weitere Informationen wenden Sie sich bitte an den [Verein der Altafraner e. V.](https://verein-der-altafraner.de/).
