# Utilitzar imatge lleugera de Node, versió 18 LTS
FROM node:18-alpine

# Crear i establir directori on s'allotjarà l'API
WORKDIR /usr/src/app

# Copiar preferentment només els arxius de paquets
COPY package*.json ./

# Executar instrucció d'instal·lar els requeriments del node global.
RUN npm install

# Passar a realitzar la copia de la resta del microservei al path working dir
COPY . .

# Exposar la definició de ports, de manera orientativa al doc
EXPOSE 3000

# Comanda Base
CMD [ "npm", "start" ]
