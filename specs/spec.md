1. Mecánicas del Jugador

Movimiento básico: Desplazamiento horizontal fluido con físicas locales (Rigidbody2D).

Salto y Doble Salto: Capacidad de saltar y realizar un segundo impulso en el aire para alcanzar plataformas más lejanas o esquivar peligros.

Dash (Impulso): Movimiento rápido y repentino hacia adelante con un tiempo de recarga (cooldown). Útil para ganar velocidad o esquivar ataques.

Condición de victoria: El primer jugador en tocar la "Línea de Meta" gana la partida.

2. Sistema de Objetos y Trampas Desplegables

Inventario / Recogida: Los jugadores pueden recoger ítems repartidos por el mapa o tener una trampa disponible cada cierto tiempo (cooldown).

Despliegue táctico: Los jugadores pueden colocar trampas en el escenario para entorpecer a los rivales. Ejemplos viables:

Muro bloqueador: Aparece un pequeño muro que frena a los jugadores durante unos segundos.

Zona ralentizadora: Un charco de barro o hielo que cambia las físicas de quien lo pisa.

Autoridad del Servidor: El servidor es el responsable de validar dónde se pone la trampa, guardar su tiempo de vida útil y avisar a todos los clientes de su existencia y posterior destrucción.

3. Obstáculos del Entorno

Zonas de caída (vacío) y plataformas que actúan como suelo básico.

Puntos de control (checkpoints): Si un jugador cae por un agujero o es eliminado por una trampa letal, reaparece en el último punto de control guardado tras una penalización de tiempo.

4. Sincronización Multijugador (WebSockets)

Envíos frecuentes de la posición, estado del jugador (corriendo, saltando, dash) y dirección hacia la que mira.

Eventos instantáneos para el sistema de objetos: ItemRecogido, TrampaColocada, TrampaDestruida.a

5. Sistema d'Autenticació i Usuaris
Interfície (Unity):Escena inicial amb camps de text per Usuari i Contrasenya, i botons per "Iniciar Sessió" i "Registrar-se".
Comunicació HTTP: Ús de `UnityWebRequest` per enviar les dades en format JSON al backend.

Backend (Node.js): Endpoints d'API REST per gestionar el registre i el login.

Persistència: Ús estricte del Patró Repository per separar l'accés a dades (implementació InMemory per a testeig inicial) de la lògica de negoci (Service) i les rutes (Controller). Les contrasenyes s'han de tractar de forma segura.