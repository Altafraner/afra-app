FROM alpine:latest

RUN apk add --no-cache tzdata
ENV TZ=Europe/Berlin
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime \
 && echo $TZ > /etc/timezone

RUN apk add --no-cache pgcli
ENTRYPOINT ["/bin/sh", "-c"]
CMD ["/usr/bin/pgcli \"postgres://$POSTGRES_USER:$POSTGRES_PASSWORD@$POSTGRES_HOST:$POSTGRES_PORT/$POSTGRES_DB\""]
