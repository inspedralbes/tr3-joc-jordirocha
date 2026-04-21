/**
 * Interfície / Classe Base per al Patró Repository.
 * Qualsevol repositori d'usuari (SQL, o InMemory) ha de complir aquest contracte tancat.
 */
class BaseRepository {
    async create(userData) {
        throw new Error('Mètode "create()" no implementat');
    }

    async findByUsername(username) {
        throw new Error('Mètode "findByUsername()" no implementat');
    }

    async findById(id) {
        throw new Error('Mètode "findById()" no implementat');
    }
}

module.exports = BaseRepository;
