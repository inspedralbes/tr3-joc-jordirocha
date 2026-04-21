const express = require('express');
const cors = require('cors');

// Importar les capes del domini
const defineUserModel = require('./models/user.model');
const SqlUserRepository = require('./repositories/sql-user.repository');
const UserService = require('./services/user.service');
const UserController = require('./controllers/user.controller');
const userRoutes = require('./routes/user.routes');

// Importar les capes del domini de Resultats
const defineResultModel = require('./models/result.model');
const SqlResultRepository = require('./repositories/sql-result.repository');
const ResultService = require('./services/result.service');
const ResultController = require('./controllers/result.controller');
const resultRoutes = require('./routes/result.routes');

module.exports = (sequelize) => {
    const app = express();

    // Middlewares bàsics de comunicació
    app.use(cors());
    app.use(express.json());

    // 1. Inicialització de Models (SQL Model real)
    const UserModel = defineUserModel(sequelize);
    const ResultModel = defineResultModel(sequelize);

    // 2. Acoblament del Repository Pattern. Aquesta és la justificació arquitectural de la rúbrica
    // Les capes superiors no reben dependència directa de com funciona la informació, simplement invoquen el Repository
    const userRepository = new SqlUserRepository(UserModel);
    const userService = new UserService(userRepository);
    const userController = new UserController(userService);

    const resultRepository = new SqlResultRepository(ResultModel);
    const resultService = new ResultService(resultRepository);
    const resultController = new ResultController(resultService);

    // 3. Injectar les rutes acoblades dins de l'app de l'API
    app.use('/api/users', userRoutes(userController));
    app.use('/api/results', resultRoutes(resultController));

    // Ruta de comprovació ràpida (Health Check per avaluar que no ha caigut en el contenidor docker)
    app.get('/', (req, res) => res.json({ status: "Microservei corrent correctament" }));

    // Exportar models per si cal fer migracions o alter des del main entry
    return { app, UserModel, ResultModel };
};
