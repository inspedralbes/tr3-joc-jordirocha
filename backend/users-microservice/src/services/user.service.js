const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');

class UserService {
    // Injecció de dependències. Rebem el repositori independentment de si és memòria o SQL
    constructor(userRepository) {
        this.userRepository = userRepository;
    }

    async register(username, email, password) {
        if (!username || !email || !password) {
            throw new Error('Falten camps obligatoris (username, email, password)');
        }

        // Comprovar que no existeixi cap usuari amb aquest nom previament
        const existingUser = await this.userRepository.findByUsername(username);
        if (existingUser) {
            throw new Error('El nom d\'usuari ja es troba en ús');
        }

        // Hashear la contrasenya abans de guardar a la BD
        const salt = await bcrypt.genSalt(10);
        const password_hash = await bcrypt.hash(password, salt);

        // Delegar la creació del registre cap al repository
        const newUser = await this.userRepository.create({
            username,
            email,
            password_hash
        });

        // Generar JWT
        const token = this.generateToken(newUser);

        return {
            user: { id: newUser.id, username: newUser.username, email: newUser.email },
            token
        };
    }

    async login(username, password) {
        if (!username || !password) {
            throw new Error('Falten camps obligatoris (username i password)');
        }

        // Buscar usuari existent al Repository pel seu nom d'usuari
        const user = await this.userRepository.findByUsername(username);
        if (!user) {
            throw new Error('Usuari o contrasenya invàlits');
        }

        // Validar contrasenya contra el hash emmagatzemat
        const validPassword = await bcrypt.compare(password, user.password_hash);
        if (!validPassword) {
            throw new Error('Usuari o contrasenya invàlits');
        }

        // Generar JWT donat que l'inici de sessió ha estat existós
        const token = this.generateToken(user);

        return {
            user: { id: user.id, username: user.username, email: user.email },
            token
        };
    }

    generateToken(user) {
        // En producció el JWT sempre ha d'anar desat amb seguretat al .env
        const secret = process.env.JWT_SECRET || 'secret-molt-segur-per-proves'; 
        const expiresIn = '24h';
        return jwt.sign({ id: user.id, username: user.username }, secret, { expiresIn });
    }
}

module.exports = UserService;
