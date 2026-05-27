FROM node:18-alpine

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .


ARG REACT_APP_API_URL
ARG REACT_APP_AUTH_URL
ENV REACT_APP_API_URL=$REACT_APP_API_URL
ENV REACT_APP_AUTH_URL=$REACT_APP_AUTH_URL

RUN npm run build

RUN npm install -g serve

EXPOSE 5173

CMD ["npm", "run", "preview", "--", "--host", "0.0.0.0"]