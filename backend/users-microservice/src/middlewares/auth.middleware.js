const jwt = require('jsonwebtoken');

// Obtenim la clau secreta des de l'entorn (o utilitzem la per defecte per seguretat en entorns locals)
const JWT_SECRET = process.env.JWT_SECRET || 'clauSuperSecretaJocMultijugador123';

/**
 * Middleware d'Autenticació JWT
 * Verifica que la petició tingui un token vàlid a la capçalera Authorization abans de permetre l'accés.
 */
function verifyToken(req, res, next) {
    const authHeader = req.headers['authorization'];
    
    // Si no hi ha capçalera o no comença per Bearer
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
        return res.status(401).json({ success: false, message: 'Accés denegat. No s\'ha proporcionat cap token.' });
    }

    const token = authHeader.split(' ')[1];

    try {
        // Validem el token de manera asíncrona (síncrona en aquest cas amb verify directe)
        const verified = jwt.verify(token, JWT_SECRET);
        
        // Adjuntem les dades del payload del token (com l'ID o username) a la petició
        req.user = verified;
        
        // Pujem a la següent capa (Controlador)
        next();
    } catch (error) {
        return res.status(403).json({ success: false, message: 'Token invàlid o expirat.' });
    }
}

module.exports = verifyToken;
