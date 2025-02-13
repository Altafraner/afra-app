FROM node:lts
WORKDIR /app

COPY WebClient .

RUN yarn install

RUN yarn build

RUN npm install

ENTRYPOINT [ "npm", "run", "dev"]
