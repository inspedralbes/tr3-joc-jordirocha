# Documentació de l'API REST i Arquitectura

Aquest document detalla els endpoints de l'API que ofereix el microservei d'usuaris (`users-microservice`) i exposa l'estratègia d'arquitectura sota el **Patró Repository**, alineada amb les expectatives metodològiques del projecte.

Totes les peticions que passen a través del Nginx des del port 80 s'enruten de forma transparent sota el prefix `/api/`.

---

## Documentació d'Endpoints

### 1. Registre d'Usuari
Crea un nou usuari dins la base de dades persistint les credencials amb un format encriptat segur.

- **Endpoint:** `POST /api/users/register`
- **Autorització Requerida:** No
- **Body de la Petició (JSON):**
  ```json
  {
    "username": "NouJugador_99",
    "email": "jugador99@exemple.com",
    "password": "PasswordSecreta123"
  }
  ```
- **Resposta d'Èxit (201 Created):**
  ```json
  {
    "success": true,
    "message": "Usuari registrat correctament.",
    "user": {
      "id": 14,
      "username": "NouJugador_99",
      "email": "jugador99@exemple.com"
    }
  }
  ```

### 2. Inici de Sessió (Login)
Valida les credencials i permet la integració segura.

- **Endpoint:** `POST /api/users/login`
- **Autorització Requerida:** No
- **Body de la Petició (JSON):**
  ```json
  {
    "username": "NouJugador_99",
    "password": "PasswordSecreta123"
  }
  ```
- **Resposta d'Èxit (200 OK):**
  ```json
  {
    "success": true,
    "message": "Autenticació completada amb èxit."
  }
  ```

### 3. Persistència de Resultats
Guarda de forma segura els resultats oficials quan una partida finalitza des del microservei de WebSockets.

- **Endpoint:** `POST /api/results/`
- **Autorització Requerida:** Sí (`Authorization: Bearer <JWT_TOKEN>`)
- **Body de la Petició (JSON):**
  ```json
  {
    "winnerName": "NouJugador_99",
    "roomCode": "A1B2C3",
    "duration": 45.32
  }
  ```
- **Resposta d'Èxit (201 Created):**
  ```json
  {
    "success": true,
    "message": "Resultat guardat correctament",
    "data": {
      "id": 1,
      "winnerName": "NouJugador_99",
      "roomCode": "A1B2C3",
      "duration": 45.32,
      "date": "2024-04-21T07:15:30.000Z"
    }
  }
  ```

---

## Justificació de l'Arquitectura: El Patró Repository

La construcció d'aquest microservei Node.js no s'ha dut a terme fusionant tota la lògica als controladors, sinó que s'ha subdividit seguint el disseny net i modular del **Patró Repository**, una aproximació vital en projectes "enterprise" o de grau corporatiu.

Les capes es divideixen de la següent manera per mantenir el **Principi de Responsabilitat Única (SRP)**:

1. **Routes (Rutes):** Actuen com un índex de les adreces web de l'aplicació. Només declaren l'URI i injecten els *middlewares* de seguretat pertinents (com validacions de JWT).
2. **Controllers (Controladors):** El punt d'entrada lògic de la xarxa. S'encarreguen exclusivament de parsejar la petició (`req`), de validar la seva estructura, i de cridar la capa de Serveis. Gestionen l'objecte de resposta (`res`) en format JSON amb el seu pertinent status HTTP.
3. **Services (Serveis):** Aquest és el cor del negoci. Els serveis recullen les crides del controlador, apliquen les normes i polítiques específiques de negoci (ex. càlculs, comprovacions) i dicten els errors transaccionals si l'operació no és lícita. No saben si l'API es consumeix via HTTP, consola o un altre procediment.
4. **Repositories (Repositoris):** Es defineixen de forma dual: existeix una **interfície abstracta** obligatòria i una **implementació concreta** d'aquesta. Tota manipulació SQL, emmagatzematge en memòria o ORM es realitza exclusivament aquí. Els serveis desconeixen quin motor de base de dades s'utilitza.
5. **Models:** L'esquema de les dades o taules dissenyades exclusivament per a modelar objectes com les entitats de la base de dades.

**Beneficis tangibles obtinguts gràcies a aquest patró:**
- **Sostituibilitat (Injecció de dependències):** Podríem canviar la base de dades de MySQL a MongoDB (o directament mantenir-la en memòria per als tests unitaris `in-memory-result.repository.js`) modificant únicament el paràmetre que passem per constructor al Servei, sense tocar absolutament cap línia de codi del model de negoci.
- **Testeig robust:** Atès que el model de dades no està estrictament vinculat amb la lògica central, es poden desenvolupar proves TDD (Test-Driven Development) d'una manera aïllada i senzilla.
