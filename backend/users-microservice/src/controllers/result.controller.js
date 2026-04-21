/**
 * Capa de Controlador per a Resultats.
 * Justificació de la separació de capes:
 * El controlador només té com a responsabilitat atendre les peticions HTTP (req, res),
 * extreure'n els paràmetres i derivar l'acció cap al Servei corresponent.
 * Mai ha d'interactuar directament amb la base de dades.
 */
class ResultController {
    constructor(resultService) {
        this.resultService = resultService;
        
        // Cal lligar els mètodes a la instància perquè Express no perdi el context de 'this'
        this.createResult = this.createResult.bind(this);
    }

    async createResult(req, res) {
        try {
            const { winnerName, roomCode, duration } = req.body;
            
            // Cridem al Servei (lògica de negoci)
            const newResult = await this.resultService.saveResult({ winnerName, roomCode, duration });
            
            return res.status(201).json({
                success: true,
                message: 'Resultat guardat correctament',
                data: newResult
            });
        } catch (error) {
            console.error('Error al guardar el resultat:', error.message);
            return res.status(400).json({
                success: false,
                message: error.message || 'Error intern en processar el resultat'
            });
        }
    }
}

module.exports = ResultController;
