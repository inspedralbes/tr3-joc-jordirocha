class UserController {
    constructor(userService) {
        this.userService = userService;
        
        // Lliguem explícitament els mètodes al context (this) per poder passar les funcions ràpidament com middlewares o handlers d'express sense perdre referència al service
        this.register = this.register.bind(this);
        this.login = this.login.bind(this);
    }

    async register(req, res) {
        try {
            // Rep el cos HTTP de la request de Unity
            const { username, email, password } = req.body;
            const result = await this.userService.register(username, email, password);
            
            // Retorna CODI 201 Created si tot flueix correctament
            res.status(201).json({
                success: true,
                message: 'Usuari registrat correctament',
                data: result
            });
        } catch (error) {
            // Qualsevol excepció o error del service es tradueix a bad request
            res.status(400).json({
                success: false,
                message: error.message
            });
        }
    }

    async login(req, res) {
        try {
            const { username, password } = req.body;
            const result = await this.userService.login(username, password);
            
            // Retorna 200 OK
            res.status(200).json({
                success: true,
                message: 'Sessió iniciada correctament',
                data: result
            });
        } catch (error) {
            // Un error a l'inici de sessió implica "No autoritzat" HTTP 401
            res.status(401).json({
                success: false,
                message: error.message
            });
        }
    }
}

module.exports = UserController;
