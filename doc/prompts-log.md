# Registre de Prompts i Iteracions (OpenSpec)

## Fase 1: Backend Base
**Data:** [Data d'avui]
**Prompt utilitzat:** `/opsx:apply Implementa només la Fase 1 definida al fitxer specs/plan.md (Fonaments del Backend en Node.js). Llegeix primer els fitxers foundations.md i spec.md per tenir el context clar. Crea l'estructura bàsica del servidor amb Express, WebSockets i el patró Repository en memòria.`

**Resultat esperat:** Estructura de carpetes de Node.js, package.json configurat i la base del servidor funcionant.
**Errors / Correccions:** (A omplir després de veure què fa la IA)

## Fase 2: Implementació del Controlador del Jugador (Unity)
**Data:** 25 de març de 2026

### Iteració 1: Definició de mecàniques avançades
**Prompt utilitzat:** `Vull començar la Fase 2 (Frontend). Abans de fer el codi bàsic, m'agradaria que el joc tingués un sistema de dash, doble salt, i alguna mecànica d'objectes o trampes per posar durant el recorregut i molestar els altres jugadors. Actualitza l'especificació.`

**Resultat esperat:** Inclusió d'aquestes mecàniques dins del document `spec.md` i validació tècnica sobre si són viables amb l'arquitectura de servidor proposada.

**Anàlisi del resultat:** La IA ha validat correctament que el dash i el doble salt es poden calcular localment al client (Unity), mentre que les trampes requereixen validació i sincronització mitjançant WebSockets des del servidor Node.js. Especificació actualitzada amb èxit.

### Iteració 2: Generació del script PlayerController
**Prompt utilitzat:** `Ara ajuda'm a fer el joc dins de Unity. Genera l'script PlayerController.cs implementant el moviment horitzontal, el doble salt i el sistema de dash utilitzant Rigidbody2D.`

**Resultat esperat:** Un script de C# preparat per assignar a un GameObject a Unity, amb variables exposades a l'Inspector per ajustar velocitats i forces.

**Errors detectats / Prevenció:** En la implementació del dash amb físiques, si no s'atura la gravetat, el personatge cau en paràbola mentre fa l'impuls, cosa que no és el comportament desitjat per a un plataformes àgil.

**Com s'ha corregit (Implementació final):** La IA ha introduït una corrutina (`FerDashCoroutine`) que guarda l'escala de gravetat original, la posa a 0 durant els mil·lisegons que dura el dash perquè el moviment sigui estrictament recte, i la restaura en acabar. També ha afegit un bloqueig (`fentDash = true`) per evitar que l'usuari interfereixi amb altres inputs durant l'impuls. El resultat és un controlador fluid i robust.

### Iteració 3: Actualització a les noves llibreries de Unity (Input System)
**Problema detectat:** Unity ha llançat un error perquè l'script utilitzava `UnityEngine.Input`, una classe obsoleta i incompatible amb l'estàndard actual del projecte configurat amb l'*Input System Package*.

**Primera solució proposada (i rebutjada):** L'agent ha suggerit canviar la configuració del projecte a *Both* per permetre el sistema antic. He decidit rebutjar aquesta opció perquè l'objectiu és treballar amb les tecnologies més actuals i evitar el codi *legacy*.

**Correcció aplicada:** He demanat a la IA que reescrigui l'script completament. S'ha importat la llibreria `UnityEngine.InputSystem` i s'ha substituït l'accés a les dades utilitzant directament `Keyboard.current` (ex: `Keyboard.current.spaceKey.wasPressedThisFrame` en comptes de `Input.GetButtonDown`).

**Resultat final:** El personatge es mou perfectament a la pantalla complint amb els estàndards moderns de Unity, mantenint la lògica de físiques intacta.

### Iteració 4: Creació del NetworkManager i connexió WebSocket nativa
**Prompt utilitzat:** `D'acord, el joc ja funciona bastant bé. Ara fem el NetworkManager per connectar Unity amb el servidor Node.js.`

**Resultat esperat:** Un script capaç de connectar-se per WebSockets al backend de la Fase 1 i preparar les funcions d'enviar i rebre missatges.

**Decisió arquitectònica i correccions:** Per evitar dependències externes, he sol·licitat l'ús de la classe nativa `System.Net.WebSockets`. Com que la recepció de dades per WebSockets s'executa en un fil secundari (background thread) i l'API de Unity només permet instanciar o moure objectes des del fil principal (Main Thread), s'ha implementat una `ConcurrentQueue<string>`. Els missatges rebuts s'emmagatzemen en aquesta cua i es processen de forma segura dins la funció `Update()`. A més, s'ha aplicat el patró Singleton per facilitar l'accés global des del jugador.

### Iteració 5: Correcció Arquitectònica (Autoritat del Host vs Servidor)
**Problema detectat:** L'agent d'IA estava orientant el codi cap a un servidor Node.js autoritatiu (encarregat de gestionar la lògica del joc en temps real). Això contradiu el punt 3.2.3 de l'enunciat, que especifica que el servidor s'ha de centrar en la gestió d'usuaris, lobbys i persistència de dades (API + BD), mentre que l'estat del joc l'ha de gestionar el *Host* de la partida.

**Correcció aplicada (Prompt):** `Creo que no has entendido bien bien, lo que tiene que hacer el servidor, el servidor solo tiene que gestionar principalmente datos de las partidas para luego almacenarlo en una BBDD y gestionar los usuarios del videojuego y las lobbys nada mas.`

**Resultat de la correcció:** S'ha redefinit el rol del servidor Node.js. A partir d'ara, Node.js actuarà exclusivament com a API REST per al registre/login i l'emmagatzematge de resultats mitjançant el Patró Repository. La part de WebSockets s'utilitzarà només com a sistema de *Matchmaking* (Lobby) i com a *Relay* (retransmissor de missatges cec) durant la partida, delegant l'autoritat de les mecàniques al client que actuï com a Host a Unity.

### Iteració 8: Integració d'animacions i gir de l'sprite
**Prompt utilitzat:** `Como hago para poner las animaciones dentro del juego? Ya tengo todos los sprites que necesito`

**Resultat esperat:** Implementació de la màquina d'estats a Unity (Animator) i connexió amb el codi del jugador per reflectir visualment els moviments.

**Solució tècnica aplicada:** S'ha configurat l'Animator Controller utilitzant paràmetres com `velocitatX` (Float), `tocaTerra` (Bool) i `dash` (Trigger) per gestionar les transicions entre els diferents estats (Idle, Run, Jump, Dash). A més, a nivell de codi s'ha implementat un mètode `GirarPersonatge()` que inverteix l'escala de l'eix X (`transform.localScale.x *= -1f`) quan l'usuari canvia de direcció, estalviant així haver de crear animacions duplicades per caminar cap a l'esquerra.

### Iteració 9: Refinament del tacte del salt (Game Feel)
**Prompt utilitzat:** `Como configuraias la animacion de salto para que se vea bien?`

**Resultat esperat:** Millorar l'aspecte visual del salt perquè no es vegi rígid ni flotant, separant les fases d'impuls i caiguda.

**Solució tècnica aplicada:** S'ha evitat el *blending* d'animacions per defecte de Unity, posant la `Transition Duration` a 0 i desactivant `Has Exit Time` en totes les transicions relacionades amb el salt per aconseguir una resposta immediata (estil arcade). A nivell de codi, s'ha afegit el paràmetre `velocitatY` (`rb.velocity.y`) per informar l'Animator. Això ha permès separar l'estat d'aire en dues fases: `Salto_Subiendo` (quan `velocitatY > 0.1`) i `Salto_Cayendo` (quan `velocitatY < -0.1`), donant un *Game Feel* molt més professional.

### Iteració 10: Pivotatge de desenvolupament (Aïllament del Core Loop local)
**Problema detectat:** El desenvolupament simultani del client (físiques, animacions) i el servidor (WebSockets, HTTP) estava generant confusió arquitectònica i dificultant la iteració ràpida del disseny de nivells.

**Decisió de Projecte (Mini Rollback):** He decidit pausar temporalment la integració amb Node.js. S'han desactivat els scripts `NetworkManager` i `AuthManager` a la jerarquia de Unity per centrar tots els esforços en polir el *Core Loop* del joc en mode local. L'objectiu és tenir les mecàniques de trampes, càmera i condició de victòria (Meta) totalment funcionals i testejades abans de reintroduir la capa de xarxa, aplicant així una metodologia de desenvolupament més iterativa i segura.

### Iteració 11: Implementació del Core Loop Local (Càmera, Mort i Victòria)
**Data:** 27 de març de 2026
**Prompt utilitzat:** `/opsx:apply Fes una pausa en la integració del servidor i centra't en el "Core Loop" local del joc a Unity. Necessito que em generis tres scripts en C# per donar forma al nivell: 1) 'CameraController.cs' perquè la càmera segueixi el jugador de forma suau (usant SmoothDamp). 2) 'DeathZone.cs' per posar als pinxos i forats, que detecti si el GameObject amb el tag "Player" hi xoca, i el faci reaparèixer a una posició inicial (Vector3). 3) 'Goal.cs' per posar a la línia d'arribada, que detecti quan el jugador la toca i mostri un missatge de "Nivell Completat". Posa tots els comentaris i noms de variables en català.`

**Resultat esperat:** Tres scripts funcionals per a Unity que permetin muntar un nivell de plataformes complet i autònom abans de reconnectar la capa de xarxa.

**Anàlisi del resultat:** L'agent ha generat correctament els scripts. S'ha delegat la responsabilitat de la càmera a un script independent (`CameraController`) en lloc de posar-ho dins del jugador, aplicant el principi de responsabilitat única. El sistema de `DeathZone` utilitza col·lisions 2D i tags per identificar el jugador i restaurar la seva posició de forma eficient.

### Iteració 12: Lògica d'animació d'entorn (Pintxos Temporitzats)
**Prompt utilitzat:** `Como harias para configurar la animacion de unos pinchos (trampa), tengo dos animaciones una que sube y otra que baja`

**Resultat esperat:** Un sistema cíclic i autònom per a les trampes del mapa sense necessitat d'escriure codi addicional (`Update`).

**Solució tècnica aplicada:** S'ha utilitzat la funcionalitat d'estats temporitzats de l'Animator Controller de Unity. S'han establert transicions circulars entre els estats `Pinchos_Subir` i `Pinchos_Bajar` basades exclusivament en la propietat `Exit Time` en lloc de paràmetres com `Bools` o `Triggers`. Ajustant l'`Exit Time` a valors majors a 1 (ex: 3) i la `Transition Duration` a 0, s'aconsegueix que la trampa romangui inactiva uns segons i s'activi instantàniament de forma rítmica, creant un patró de salt previsible per al jugador.

### Iteració 18: Millora de modularitat visual (3-Slicing per a Plataformes)
**Data:** 27 de març de 2026
**Prompt utilitzat:** `/opsx:apply Modifica el script AdvancedLevelGenerator.cs per substituir el sistema d'un sol prefab de plataforma estirat per un sistema modular de 3 prefabs: prefabPlatEsquerra, prefabPlatMig i prefabPlatDreta... [descripció de la concatenació modular de N parts de mig].`

**Resultat esperat:** Un script `AdvancedLevelGenerator.cs` reescrit que, en lloc d'instanciar i estirar un prefab rectangul·lar, construeix cada plataforma concatenant tres tipus d'objectes (tall esquerre, mig repetible N vegades, i tall dret).

**Anàlisi de l'optimització visual:** He rebutjat el mètode d'estirar sprites per codi perquè provocava distorsió de píxels als laterals. L'adopció del model modular (3-Slicing) resol el problema visual de forma professional, permetent plataformes de qualsevol longitud mantenint els perfils intactes. He exigit a l'agent d'IA que recalculi l'amplada total de la plataforma concatenada (Esquerra + (N * Mig) + Dreta) i utilitzi aquest valor final per als càlculs de la paràbola de salt, garantint que el nivell segueixi sent superable.

### Iteració 22: Integració de Recompenses i Checkpoints a la Generació Procedimental
**Data:** 27 de març de 2026
**Prompt utilitzat:** `/opsx:apply Modifica 'AdvancedLevelGenerator.cs' per integrar 3 nous prefabs: 'prefabMoneda', 'prefabCheckpoint' i 'prefabMeta'... [instruccions de fraccionament del bucle i nous scripts Coin.cs i Checkpoint.cs].`

**Resultat esperat:** El generador de nivells ha de col·locar estratègicament els objectes interactius: la meta al 100% del recorregut, el checkpoint al 50%, i monedes repartides de forma aleatòria. S'espera la creació dels scripts que gestionin la recol·lecció (destrucció del GameObject i suma de punts) i el guardat de la posició de reaparició.

**Anàlisi de disseny:** S'ha estructurat la generació perquè els elements clau no depenguin de l'atzar total (el checkpoint i la meta tenen posicions matemàticament garantides en relació a la longitud del nivell). S'ha requerit a la IA que els nous elements es col·loquin alineats amb la superfície Y de les plataformes, evitant que es generin a l'aire o sota terra, millorant així l'experiència d'usuari i el *Game Feel*.

### Iteració 23 (Alternativa): Correcció de Pivots mitjançant Offset per Codi
**Problema detectat:** Els nous elements interactius (banderes i monedes) apareixien parcialment enterrats a les plataformes a causa del seu Pivot central per defecte.

**Decisió Tècnica i Solució:** En lloc de modificar l'asset visual des del Sprite Editor, s'ha optat per una solució programàtica que ofereix més flexibilitat des de l'Inspector de Unity. S'ha indicat a l'agent d'IA que introdueixi una variable `offsetYObjectes` a l'script `AdvancedLevelGenerator.cs`. Aquest valor se suma a la coordenada Y durant la instanciació dels objectes, elevant-los automàticament per reposar-los sobre la superfície geomètrica de la plataforma. Aquesta solució permet ajustar l'alçada visual en temps d'execució sense alterar els fitxers d'origen.

### Fase 3: Represa del Backend i Microservei d'Usuaris

**Iteració 24: Desenvolupament del Sistema de Registre i Accés amb Patró Repository
Prompt utilitzat: /opsx: Actua com un desenvolupador Senior Full-Stack especialitzat en Unity, Node.js i disseny d'arquitectures de programari (microserveis). He de desenvolupar el "Microservei d'Usuaris" (Registre i Login) per a un videojoc multijugador complint estrictament amb la meva rúbrica. Tot el codi, comentaris i documentació han d'estar en català. 1. FRONTEND (UNITY): Crea la interfície d'Accés i Identificació EXCLUSIVAMENT amb UI Toolkit. Proporciona el .uxml (panells Login/Registre, camps, botons), el .uss (estils bàsics flexbox) i el controlador C# (AuthUIController.cs) que llegeixi l'UI Toolkit i faci peticions HTTP POST amb UnityWebRequest (JSON). 2. BACKEND (NODE.JS + SQL + SEQUELIZE): Crea una API REST amb Express per al microservei (rutes: POST /api/users/register i POST /api/users/login). És OBLIGATORI implementar el Patró Repository per separar responsabilitats: Controllers (gestió HTTP), Services (lògica, JWT, bcrypt) i Repositories (accés a dades). Dins de Repositories, crea una classe base i DUES implementacions: SqlUserRepository (amb Sequelize per a PostgreSQL/MySQL) i InMemoryUserRepository (per a testing). 3. TESTS UNITARIS: Proporciona un test bàsic per al UserService utilitzant exclusivament el InMemoryUserRepository per comprovar el registre/login sense tocar la DB real. 4. DOCKER: Genera el Dockerfile per aquest microservei Node i un docker-compose.yml que aixequi la base de dades SQL i el microservei. 5. ENTREGABLES: Codi de Unity, codi de backend separat per capes (controllers, services, repositories, models), tests, configuració Docker i instruccions clares. Justifica l'arquitectura als comentaris del codi.

Resultat esperat: La creació completa del front-end a Unity utilitzant exclusivament UI Toolkit per al Login/Registre, i la generació del backend a Node.js estructurat com un microservei independent. El codi ha d'incloure el Patró Repository amb dues implementacions (Real amb SQL/Sequelize i InMemory per a tests), a més de la configuració de contenidors amb Docker Compose.

Decisió Arquitectònica i Prevenció d'Errors: S'ha evitat l'antipatró d'encapsular Node.js i una base de dades SQL complexa en un únic contenidor Docker. En el seu lloc, s'ha optat per requerir un docker-compose.yml que orquestri els dos serveis per separat, respectant els estàndards de la indústria. A nivell de codi, per complir estrictament amb la rúbrica d'avaluació, s'ha exigit el desacoblament absolut de la lògica de negoci (Services), l'accés a dades (Repositories) i el routing (Controllers). El rol del servidor s'ha mantingut estrictament com a API REST HTTP, deixant la implementació de WebSockets per al futur microservei de partides.