const express = require('express');
const verifyToken = require('../middlewares/auth.middleware');

module.exports = (resultController) => {
    const router = express.Router();

    // Ruta HTTP POST per guardar els resultats.
    // Està protegida pel middleware 'verifyToken' injectat prèviament.
    router.post('/', verifyToken, resultController.createResult);

    return router;
};
