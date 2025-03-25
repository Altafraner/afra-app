FROM ruby:3.3-bullseye
RUN apt-get update -qq && apt-get install -y sqlite3 libsqlite3-dev \
    && gem install sqlite3 --no-document \
    && gem install mailcatcher --no-document

EXPOSE 1025 1080
ENTRYPOINT ["mailcatcher", "--foreground"]
CMD ["--ip", "0.0.0.0"]