# Security Policy

Definitions and notations from [DEFINITIONS.md](DEFINITIONS.md) apply to this
document.

## Supported Versions

Afra-App is developed continuously. Only the `main` branch receives security
fixes.

Currently, the only supported configuration depends on the following services.
- MS AD DS as LDAP Provider
- Keycloak as OIDC IdP
- Postgres 18 as Database
- Linux as OS
- NGINX as reverse proxy and for serving static assets

Whilst other configurations might also function, we do not currently support
them. Please note that we do not support using development servers or
development builds in production environments.

## Reporting a Vulnerability

You MUST NOT report security vulnerabilities through public GitHub issues, pull
requests or discussions.

Instead, send an email to the security team. You SHOULD encrypt your email using
OpenPGP if possible. You SHOULD send your email to all members listed below.

- [mail@johann-hoepfner.de](mailto:mail@johann-hoepfner.de), public key:
  [FF87638F553F96A91D512A8588DD2BB2F6178E40](https://keys.openpgp.org/search?q=FF87638F553F96A91D512A8588DD2BB2F6178E40)
- [r.schlossarek@verein-der-altafraner.de](mailto:r.schlossarek@verein-der-altafraner.de), public key:
  [6D9B89D76823736196F1D4DB4204A60CE7C54E5D](https://keys.openpgp.org/search?q=6D9B89D76823736196F1D4DB4204A60CE7C54E5D)

Reports SHOULD be submitted in either English or German.

## Scope and Rules

- only test against your own local instance or with accounts you are authorized
  to use
- do not access, modify or delete data belonging to others beyond what is
  strictly necessary to demonstrate the issue. If you have modified any data
  revert it. If that is not possible, communicate to us ASAP exactly what data
  you have modified.
- do not perform denial-of-service attacks, spam, or social engineering on live
  instances.
- adhere to our [AI Policy](AI_POLICY.md) also when sending security reports.

We will not pursue legal action against researchers who act in good faith and
follow these rules. We will recommend to any schools running this software to
adapt the same position.
