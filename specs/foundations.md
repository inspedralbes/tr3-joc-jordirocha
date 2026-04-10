1. Context i Visió General
El projecte és un videojoc 2D multijugador d'estil "cursa de supervivència" (inspirat en Fall Guys). Entre 2 i 4 jugadors competeixen per arribar del punt A al punt B d'un mapa ple d'obstacles abans que s'acabi el temps. S'utilitza Unity per al client i Node.js per al backend.

2. Objectius Principals

Desenvolupar una mecànica de moviment àgil per al jugador.

Sincronitzar la posició dels jugadors en temps real (de forma limitada) mitjançant WebSockets.

Implementar un sistema de lobby per crear i unir-se a partides (via API HTTP amb UnityWebRequest).

Garantir la persistència de dades (usuaris, resultats de les partides) utilitzant el patró Repository al backend Node.js.

Integrar un agent autònom (ML-Agents) capaç de recórrer el mapa esquivant els obstacles per actuar com a "bot" competidor.

3. Restriccions Tècniques i de Disseny

Sense col·lisions entre jugadors: Per simplificar la xarxa, els jugadors no xoquen entre ells, només amb l'entorn.

Autoritat de l'entorn: El jugador Host (creador de la partida) o el servidor s'encarregaran de calcular el cicle de les trampes (ex. portes que s'obren/tanquen) i ho sincronitzaran per a la resta.

Backend net: El servidor ha de separar estrictament l'accés a dades (Repository), la lògica (Service) i l'exposició (Controller/WebSockets).

Temps de desenvolupament: Cal limitar-se a un sol mapa/escenari i un nombre reduït de trampes diferents per assegurar l'assoliment del projecte.