const BaseRepository = require('./base.repository');

class SqlUserRepository extends BaseRepository {
    constructor(userModel) {
        super();
        this.User = userModel; // Model de Sequelize injectat via constructor
    }

    async create(userData) {
        // Capturarà automàticament els trencaments o valors invàlids i els enviarà a ser capturats pel bloc superior (Service)
        try {
            const user = await this.User.create(userData);
            return user.toJSON();
        } catch (error) {
            throw error; 
        }
    }

    async findByUsername(username) {
        const user = await this.User.findOne({ where: { username } });
        return user ? user.toJSON() : null;
    }

    async findById(id) {
        const user = await this.User.findByPk(id);
        return user ? user.toJSON() : null;
    }
}

module.exports = SqlUserRepository;
