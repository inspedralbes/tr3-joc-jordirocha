const { Router } = require('express');

// Accepta el controlador com a paràmetre per tal de fer fàcil el canvi o mockejat de controladors des de l'arxiu de configuració global.
module.exports = (userController) => {
    const router = Router();

    // Definició de rutes API REST
    router.post('/register', userController.register);
    router.post('/login', userController.login);

    return router;
};
