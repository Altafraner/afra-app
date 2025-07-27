#!/bin/bash

# Create directories
mkdir -p certs

# Create certificates for backend, sp and mock idp
openssl req -x509 -newkey rsa:4096 -keyout certs/app.key     -out certs/app.pem     -sha256 -days 36500 -nodes -subj "/C=DE/ST=Saxony/L=Meißen/O=Verein der Altafraner/OU=Afra-App/CN=altafraner.de"

