﻿FROM node:alpine AS build
WORKDIR /app

COPY ./ .

RUN npm install
RUN npm run build

FROM docker.io/nginx:alpine

COPY --from=build /app/dist /app
COPY nginx.conf /etc/nginx/conf.d/default.conf