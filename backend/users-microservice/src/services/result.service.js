/**
 * Capa de Service per a Resultats.
 * Justificació de la separació de capes: 
 * Aquesta capa rep la informació dels Controladors i aplica la lògica de negoci
 * abans d'enviar-la als Repositoris (persitència pura). 
 * Encara que ara sigui una connexió directa, si afegim lògica com ara atorgar punts extra al guanyador,
 * aquesta lògica aniria aquí, deixant el controlador lliure de lògica de negoci.
 */
class ResultService {
    constructor(resultRepository) {
        // Injectem el repositori (Patró Injecció de Dependències)
        this.resultRepository = resultRepository;
    }

    async saveResult(resultData) {
        if (!resultData.winnerName || !resultData.roomCode || !resultData.duration) {
            throw new Error("Falten dades requerides per guardar el resultat.");
        }
        return await this.resultRepository.create(resultData);
    }
}

module.exports = ResultService;
