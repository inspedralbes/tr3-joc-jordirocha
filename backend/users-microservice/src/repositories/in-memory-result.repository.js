const BaseResultRepository = require('./base-result.repository');

class InMemoryResultRepository extends BaseResultRepository {
    constructor() {
        super();
        this.results = [];
        this.currentId = 1;
    }

    async create(resultData) {
        const result = {
            id: this.currentId++,
            winnerName: resultData.winnerName,
            roomCode: resultData.roomCode,
            duration: resultData.duration,
            date: new Date()
        };
        this.results.push(result);
        return result;
    }

    async findAll() {
        // Retorna els resultats ordenats per data descendent
        return [...this.results].sort((a, b) => b.date - a.date);
    }
}

module.exports = InMemoryResultRepository;
