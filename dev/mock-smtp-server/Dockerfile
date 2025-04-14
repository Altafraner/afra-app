FROM alpine:3.21

RUN apk add --no-cache ruby \
    ruby-json libc6-compat sqlite-libs libstdc++ \
    ruby-dev make g++ sqlite-dev \
    && gem install sqlite3 --version="~> 1.3" --platform=ruby \
    && gem install mailcatcher --no-document \
    && apk del --rdepends --purge ruby-dev make g++ sqlite-dev \
    && rm -rf /var/cache/apk/* /tmp/* /var/tmp/*

EXPOSE 1025 1080
ENTRYPOINT ["mailcatcher", "--foreground"]
CMD ["--ip", "0.0.0.0"]
