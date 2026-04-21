# 2D Fall Guys Multijugador

**Grup de Treball:** [Espai per definir: Grup X / Noms dels integrants]

---

## Descripció del Projecte i Arquitectura

Aquest projecte és un joc multijugador 2D d'estil "Fall Guys" / cursa de plataformes competitiu. Està dissenyat seguint una arquitectura de microserveis avançada que garanteix una clara separació de responsabilitats entre el client, el servei d'autenticació i la lògica de la partida en temps real.

L'arquitectura Client-Servidor s'estructura de la següent manera:
- **Client (Unity):** Desenvolupat en C#, gestiona la lògica de presentació (HUD, UI Toolkit), el moviment del jugador (físiques i inputs), i la sincronització de l'estat visual mitjançant la recepció i emissió d'esdeveniments.
- **Reverse Proxy (Nginx):** S'encarrega d'unificar totes les peticions externes en un únic punt d'entrada (port 80). Enruta les connexions de WebSockets cap al microservei de matchmaking i les crides REST (HTTP) cap al microservei d'usuaris.
- **Matchmaking Microservice (Node.js + WebSockets natius):** Manté l'estat de les sales actives, gestiona l'entrada/sortida de jugadors en temps real, sincronitza posicions i calcula la lògica de finalització de la partida per declarar els guanyadors.
- **Users Microservice (Node.js + Express + Sequelize):** Administra la lògica de domini de dades persistents, proveint una API RESTful protegida amb JSON Web Tokens (JWT) per a usuaris i resultats de les partides.
- **Base de Dades (MySQL):** Emmagatzema de manera persistent els usuaris registrats i els resultats històrics. S'acompanya de phpMyAdmin per a l'administració visual.

---

## Enllaços als Recursos de Gestió (OpenSpec)

- **Tauler de Disseny d'Interfície i UX (Figma):** [Enganxar aquí la URL del Figma]
- **Tauler de Gestió Àgil i Sprints (Taiga):** [Enganxar aquí la URL del Taiga]

---

## Estat Actual del Desenvolupament

- [x] Arquitectura de microserveis Dockeritzada.
- [x] Proxy Nginx operatiu.
- [x] Lògica de Registre, Login i JWT integrada.
- [x] Connexió WebSockets funcional (Sales, Moviment, Guanyadors).
- [x] Interfície Unity actualitzada (UI Toolkit per al HUD i ResultScreen).
- [x] Implementació del Patró Repository a l'API.
- [x] Persistència dels resultats de la partida automàtica.

---

## Instruccions d'Execució i Desplegament

L'aplicació està completament "dockeritzada" per facilitar el seu desplegament agnòstic a l'entorn. Tota la infraestructura s'aixeca de cop.

**Passos previs:**
Necessites tenir Docker i Docker Compose instal·lats a la teva màquina.

1. Obre un terminal a l'arrel d'aquest repositori.
2. Executa la següent comanda per compilar i aixecar la infraestructura en segon pla:
   ```bash
   docker-compose up -d --build
   ```
3. Verifica que tots els contenidors funcionen correctament:
   ```bash
   docker-compose ps
   ```
4. *Opcional:* Pots accedir a la gestió de la Base de Dades mitjançant phpMyAdmin navegant a `http://localhost:8081`.

**Serveis Actius:**
- API unificada i WebSockets (Nginx): `http://localhost` (Port 80)
- Microservei Usuaris intern: `http://localhost:3000`
- Microservei Matchmaking intern: `ws://localhost:8080`

Per aturar tota la infraestructura i netejar els contenidors, executa:
```bash
docker-compose down
```
