1. Fase 1: Fonaments del Backend (Node.js)

Inicialitzar el projecte Node.js amb Express.

Configurar un servidor WebSocket natiu (ws) per acceptar connexions entrants.

Implementar l'estructura base del Patró Repository (només amb la implementació InMemory inicialment per poder testejar ràpid sense base de dades).

Crear el servei de partides (Lobby) per gestionar jugadors connectats i desconnectats.

2. Fase 2: Fonaments del Frontend (Unity 2D)

Crear el projecte Unity 2D i dissenyar una escena de proves (terra bàsic i parets).

Programar el PlayerController local:

Moviment horitzontal amb Rigidbody2D.

Detecció de terra (Ground Check) per permetre el salt.

Lògica del doble salt i temps de recàrrega (cooldown) del dash.

3. Fase 3: Sincronització Multijugador (Core)

Crear un NetworkManager a Unity usant classes natives de C# per connectar-se al WebSocket de Node.js.

Establir el flux de dades: Unity envia la posició (X, Y) i l'estat (animació actual) al servidor a intervals regulars (tick rate).

El servidor fa un broadcast d'aquestes dades a la resta de clients.

Unity instància "clons" (prefabs) per representar els altres jugadors a la pantalla.

4. Fase 4: Mecàniques Competitives (Trampes)

Afegir la funció al PlayerController per instanciar un objecte "Trampa".

Enviar l'esdeveniment al servidor (TrapPlaced amb coordenades i tipus).

El servidor valida i reenvia l'ordre perquè tots els clients instanciïn la trampa a la mateixa posició.

Implementar la lògica de penalització a Unity quan un jugador xoca amb la trampa d'un rival.

5. Fase 5: Persistència i ML-Agents

Connectar el Patró Repository a una base de dades real (MySQL o MongoDB) per guardar estadístiques de victòries i usuaris.

Integrar el paquet ML-Agents a Unity i entrenar un agent senzill perquè sàpiga saltar obstacles bàsics i recórrer el mapa de proves de forma autònoma.

6: Fase 1.5 Sistema de Login i Registre (Full-Stack)
Backend: Crear el `UserRepository`, `UserService` i `UserController` a Node.js.
Backend: Configurar les rutes `/api/users/register` i `/api/users/login`.
Frontend: Crear l'script `AuthManager.cs` a Unity preparat per enllaçar amb la UI (camps de text i botons).
Frontend: Implementar les crides HTTP POST des de Unity cap a Node.js.