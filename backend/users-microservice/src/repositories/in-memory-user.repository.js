const BaseRepository = require('./base.repository');

/**
 * Repositori temporal en memòria per test unitaris.
 * Assegura poder aïllar el servei de la base de dades durant les bateries de tests.
 */
class InMemoryUserRepository extends BaseRepository {
    constructor() {
        super();
        this.users = []; // Array pur per contenir dades temporalment en JS
        this.currentId = 1;
    }

    async create(userData) {
        // Simulem el llançament d'error si s'intenta afegir un correu / usuari repetit
        const existingInfo = this.users.find(u => u.username === userData.username || u.email === userData.email);
        if(existingInfo) {
            throw new Error('Validation error: username or email must be unique');
        }

        const newUser = {
            id: this.currentId++,
            ...userData,
            createdAt: new Date(),
            updatedAt: new Date()
        };
        this.users.push(newUser);
        return newUser;
    }

    async findByUsername(username) {
        const user = this.users.find(u => u.username === username);
        return user || null;
    }

    async findById(id) {
        const user = this.users.find(u => u.id === id);
        return user || null;
    }
}

module.exports = InMemoryUserRepository;
