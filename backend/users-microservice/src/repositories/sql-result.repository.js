const BaseResultRepository = require('./base-result.repository');

class SqlResultRepository extends BaseResultRepository {
    constructor(resultModel) {
        super();
        this.Result = resultModel; // Injecció de dependència del model Sequelize
    }

    async create(resultData) {
        try {
            const result = await this.Result.create(resultData);
            return result.toJSON();
        } catch (error) {
            throw error;
        }
    }

    async findAll() {
        try {
            const results = await this.Result.findAll({
                order: [['date', 'DESC']]
            });
            return results.map(r => r.toJSON());
        } catch (error) {
            throw error;
        }
    }
}

module.exports = SqlResultRepository;
