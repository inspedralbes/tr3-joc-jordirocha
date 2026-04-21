FROM node:18-alpine

# Definim el directori de treball dins el contenidor
WORKDIR /usr/src/app

# Copiem el package.json
COPY package*.json ./

# Instal·lem les dependències
RUN npm install --omit=dev

# Copiem la resta del codi en el directori de treball
COPY . .

# Exposem el port del servidor WebSocket (per defecte 8080)
EXPOSE 8080

# Comanda per defecte per aixecar l'aplicació
CMD [ "node", "server.js" ]
