const { WebSocketServer } = require('ws');
const crypto = require('crypto');
const jwt = require('jsonwebtoken');

const PORT = process.env.PORT || 8080;
const JWT_SECRET = process.env.JWT_SECRET || 'clauSuperSecretaJocMultijugador123';
const wss = new WebSocketServer({ port: PORT });

// Emmagatzematge en memòria de les sales actives.
// Estructura: { "CODI": { host: ws, players: [ { id, name, ws } ] } }
const rooms = {};

// Funció per generar un codi de sala únic
function generateRoomCode() {
    return crypto.randomBytes(3).toString('hex').toUpperCase(); // Ex: A1B2C3
}

// Funció per enviar missatges JSON segurs
function sendToSocket(socket, message) {
    if (socket.readyState === 1) { // 1 = OPEN
        socket.send(JSON.stringify(message));
    }
}

// Funció per actualitzar a tots els membres de la sala sobre els jugadors
function broadcastRoomUpdate(roomCode) {
    const room = rooms[roomCode];
    if (!room) return;

    // Recopilem les dades per amagar l'objecte WebSocket quan enviem
    const playersData = room.players.map(p => ({ id: p.id, name: p.name }));

    const updateMsg = {
        type: 'ROOM_UPDATED',
        roomCode: roomCode,
        players: playersData
    };

    // Enviem la llista a cada jugador present
    room.players.forEach(p => {
        sendToSocket(p.ws, updateMsg);
    });
}

// Elimina el jugador de qualsevol sala en què es trobi (útil en cas de desconnexió)
function removePlayerFromAnyRoom(socket) {
    for (const roomCode in rooms) {
        const room = rooms[roomCode];
        const playerIndex = room.players.findIndex(p => p.ws === socket);

        if (playerIndex !== -1) {
            const player = room.players[playerIndex];
            room.players.splice(playerIndex, 1);
            console.log(`Jugador destituït: ${player.name} de la sala ${roomCode}`);

            if (room.players.length === 0) {
                // Si ja no queda ningú a la sala, la destruïm.
                delete rooms[roomCode];
                console.log(`Eliminant sala buida ${roomCode}`);
            } else {
                // Si el Host ha marxat, designem el següent jugador com a nou Host (opcional, però bona pràctica).
                if (room.host === socket) {
                    room.host = room.players[0].ws;
                    sendToSocket(room.host, { type: 'BECAME_HOST', roomCode });
                }
                broadcastRoomUpdate(roomCode);
            }
        }
    }
}

console.log(`Servidor de Matchmaking (WebSocket) iniciat al port ${PORT}`);

// Lògica principal del servidor WebSocket
wss.on('connection', function connection(ws) {
    console.log('Nou client connectat.');

    ws.on('message', function incoming(message) {
        try {
            const data = JSON.parse(message);
            console.log('Missatge rebut:', data);

            switch (data.type) {
                case 'CREATE_ROOM': {
                    // Creació de la sala
                    const roomCode = generateRoomCode();
                    const playerId = crypto.randomUUID();
                    // REQUISIT: Fem servir data.username enviat des d'Unity si s'ha fet Login
                    const username = data.username || "Jugador_" + roomCode.substring(0, 3);

                    rooms[roomCode] = {
                        host: ws,
                        players: [{ id: playerId, name: username, ws: ws }],
                        finishedPlayers: []
                    };

                    console.log(`Sala Creada: ${roomCode} pel host: ${username}`);
                    sendToSocket(ws, {
                        type: 'ROOM_CREATED',
                        roomCode: roomCode,
                        isHost: true,
                        playerId: playerId
                    });

                    // Actualitzem immediatament per rebre la llista
                    broadcastRoomUpdate(roomCode);
                    break;
                }

                case 'JOIN_ROOM': {
                    // Codi per afegir-se a la sala
                    const roomCode = data.roomCode;
                    // REQUISIT: Extraurem exactament l'string del nom retingut en l'Autenticació Frontend
                    const username = data.username || "Anònim";
                    const room = rooms[roomCode];

                    if (room) {
                        // REQUISIT ANTI-REBOT: Comprovem que el jugador no estigui duplicat a la sala
                        const isDuplicated = room.players.some(p => p.ws === ws || p.name === username);
                        if (isDuplicated) {
                            console.log(`Rebutjat intent duplicat d'unió per: ${username} a la sala ${roomCode}`);
                            sendToSocket(ws, { type: 'ERROR', message: 'Ja estàs dins de la sala o aquest usuari ja ha entrat.' });
                            break;
                        }

                        const playerId = crypto.randomUUID();
                        room.players.push({ id: playerId, name: username, ws: ws });

                        console.log(`Jugador ${username} s'ha unit a la sala ${roomCode}`);
                        sendToSocket(ws, { type: 'JOINED_ROOM', roomCode: roomCode, isHost: false, playerId: playerId });
                        broadcastRoomUpdate(roomCode);
                    } else {
                        // Notificació de sala no trobada
                        sendToSocket(ws, { type: 'ERROR', message: 'La sala no existeix.' });
                    }
                    break;
                }

                case 'START_GAME': {
                    const roomCode = data.roomCode;
                    const room = rooms[roomCode];

                    if (room) {
                        // Només permetem al host iniciar
                        if (room.host === ws) {
                            console.log(`El Host (sala ${roomCode}) ha iniciat la partida! Notificant a tothom.`);

                            // Generem una seed aleatòria gran per a la generació col·lectiva del mapa
                            const generatedSeed = Math.floor(Math.random() * 10000000);

                            // Recopilem les dades per amagar l'objecte WebSocket quan enviem
                            const playersData = room.players.map(p => ({ id: p.id, name: p.name }));

                            const matchStartedMsg = {
                                type: 'MATCH_STARTED',
                                roomCode: roomCode,
                                seed: generatedSeed,
                                players: playersData
                            };

                            room.players.forEach(p => {
                                sendToSocket(p.ws, matchStartedMsg);
                            });

                            // Eliminar la sala de matchmaking ja que estan jugant (opcional, ho fem aquí per simplicitat).
                            // delete rooms[roomCode]; 
                        } else {
                            sendToSocket(ws, { type: 'ERROR', message: 'Només el host pot iniciar la partida.' });
                        }
                    } else {
                        sendToSocket(ws, { type: 'ERROR', message: 'La sala no existeix.' });
                    }
                    break;
                }

                case 'PLAYER_TRANSFORM': {
                    const roomCode = data.roomCode;
                    const room = rooms[roomCode];

                    if (room) {
                        // Fem un broadcast a la resta de jugadors, excloent l'emissor de les dades
                        const transformMsg = {
                            type: 'PLAYER_TRANSFORM',
                            id: data.id,
                            x: data.x,
                            y: data.y,
                            scaleX: data.scaleX
                        };

                        room.players.forEach(p => {
                            if (p.ws !== ws) {
                                sendToSocket(p.ws, transformMsg);
                            }
                        });
                    }
                    break;
                }

                case 'PLAYER_FINISHED': {
                    const roomCode = data.roomCode;
                    const room = rooms[roomCode];

                    if (room) {
                        const playerId = data.id;
                        
                        // Evitem duplicats de meta
                        if (!room.finishedPlayers.some(p => p.id === playerId)) {
                            // Buscar jugador real
                            const playerObj = room.players.find(p => p.id === playerId) || { name: 'Desconegut' };

                            room.finishedPlayers.push({
                                id: playerId,
                                name: playerObj.name,
                                time: data.time,
                                score: data.score
                            });

                            // Notificar a tots que algú ha acabat (per si la HUD ho marca p.ex.)
                            room.players.forEach(p => {
                                sendToSocket(p.ws, {
                                    type: 'PLAYER_FINISHED_BROADCAST',
                                    id: playerId,
                                    time: data.time,
                                    score: data.score
                                });
                            });

                            console.log(`Jugador ${playerObj.name} ha acabat a la sala ${roomCode} amb temps ${data.time}`);

                            // Si TOTS els de la sala han passat la meta
                            if (room.finishedPlayers.length >= room.players.length) {
                                // Ordenar per temps (més curt guanya) i en cas d'empat per punts (més gran guanya)
                                room.finishedPlayers.sort((a, b) => {
                                    if (a.time !== b.time) return a.time - b.time;
                                    return b.score - a.score;
                                });

                                console.log(`[MATCH_RESULTS] Tots han acabat a la sala ${roomCode}`);

                                // Extreure el guanyador (el primer després d'ordenar per temps)
                                const winner = room.finishedPlayers[0];

                                // Generar token en calent per a peticions de servidor a servidor (dura 1 minut)
                                const token = jwt.sign({ service: 'matchmaking' }, JWT_SECRET, { expiresIn: '1m' });

                                // Fer la crida interna per persistir el resultat
                                fetch('http://users-api:3000/api/results', {
                                    method: 'POST',
                                    headers: {
                                        'Content-Type': 'application/json',
                                        'Authorization': `Bearer ${token}`
                                    },
                                    body: JSON.stringify({
                                        winnerName: winner.name,
                                        roomCode: roomCode,
                                        duration: winner.time
                                    })
                                }).then(res => res.json())
                                  .then(data => console.log('✔ Resultat de la partida desat a la BBDD:', data))
                                  .catch(err => console.error('❌ Error desant el resultat de la partida:', err));

                                const resultsMsg = {
                                    type: 'MATCH_RESULTS',
                                    roomCode: roomCode,
                                    matchResults: room.finishedPlayers // enviem id, name, time, score ordenats
                                };

                                room.players.forEach(p => {
                                    sendToSocket(p.ws, resultsMsg);
                                });

                                // Opcionalment podem eliminar la sala, o esperar que marxin un a un
                                // delete rooms[roomCode]; 
                            }
                        }
                    }
                    break;
                }
            }
        } catch (e) {
            console.error('Error processant el missatge:', e);
        }
    });

    ws.on('close', function close() {
        console.log('Client desconnectat.');
        removePlayerFromAnyRoom(ws);
    });
});
