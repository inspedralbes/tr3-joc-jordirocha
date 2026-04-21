/**
 * Interfície / Classe Base per al Patró Repository de Resultats.
 * Defineix el contracte obligatori per a qualsevol implementació.
 */
class BaseResultRepository {
    async create(resultData) {
        throw new Error('Mètode "create()" no implementat');
    }

    async findAll() {
        throw new Error('Mètode "findAll()" no implementat');
    }
}

module.exports = BaseResultRepository;
