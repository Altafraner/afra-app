#!/bin/bash

# Create certificates for backend, sp and mock idp
openssl req -x509 -newkey rsa:4096 -keyout certs/samlSP.key  -out certs/samlSP.pem  -sha256 -days 36500 -nodes -subj "/C=DE/ST=Saxony/L=Meißen/O=Verein der Altafraner/OU=Afra-App/CN=altafraner.de"
openssl req -x509 -newkey rsa:4096 -keyout certs/samlIdP.key -out certs/samlIdP.pem -sha256 -days 36500 -nodes -subj "/C=DE/ST=Saxony/L=Meißen/O=Verein der Altafraner/OU=Afra-App/CN=altafraner.de"
openssl req -x509 -newkey rsa:4096 -keyout certs/app.key     -out certs/app.pem     -sha256 -days 36500 -nodes -subj "/C=DE/ST=Saxony/L=Meißen/O=Verein der Altafraner/OU=Afra-App/CN=altafraner.de"

cp idp.env.example idp.env
echo "PRIVATE_KEY=$(base64 -w0 certs/samlIdP.key)" >> ../dev/idp.env
echo "PUBLIC_KEY=$(base64 -w0 certs/samlIdP.pem)" >> ../dev/idp.env