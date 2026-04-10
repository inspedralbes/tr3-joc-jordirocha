const UserService = require('../src/services/user.service');
const InMemoryUserRepository = require('../src/repositories/in-memory-user.repository');
const bcrypt = require('bcrypt');

describe('UserService Unit Test (amb InMemoryUserRepository)', () => {
    let userService;
    let inMemoryRepo;

    // Abans de cada test resseteja i inicia una base de dades local i plana en memòria completament foradada dels components de Sequelize
    beforeEach(() => {
        inMemoryRepo = new InMemoryUserRepository();
        userService = new UserService(inMemoryRepo);
    });

    describe('Funcionalitat de Registre (Register)', () => {
        it('Hauria de registrar un nou usuari de forma exitosa per parametres correctes', async () => {
             // 1. Arrange & Act
            const res = await userService.register('testuser', 'test@test.com', 'mypassword123');
            
            // 2. Assert
            expect(res.user.username).toBe('testuser');
            expect(res.user.email).toBe('test@test.com');
            expect(res.token).toBeDefined();

            // Comprovar si les dades s'han injectat en la base in-memory a mode de comprovació exhaustiva
            const userInRepo = await inMemoryRepo.findByUsername('testuser');
            expect(userInRepo).not.toBeNull();
            
            // Validem també que el Servei ha encomanat la feina bé per ecriptar pass normal
            const isMatch = await bcrypt.compare('mypassword123', userInRepo.password_hash);
            expect(isMatch).toBe(true);
        });

        it('No hauria de permetre dos usuaris amb mateix nom d\'usuari', async () => {
            await userService.register('testuser', 'test@test.com', 'password123');
            
            // El segon intent de registre de 'testuser' ha d'explotar amb missatge específic
            await expect(userService.register('testuser', 'other@test.com', 'pass')).rejects.toThrow('El nom d\'usuari ja es troba en ús');
        });
    });

    describe('Funcionalitat d\'Autenticació (Login)', () => {
        beforeEach(async () => {
            // Executem un registre per tenir algú de pre-condició als tests de login
            await userService.register('loginuser', 'login@test.com', 'correctpass');
        });

        it('Retornarà informació rellevant i Token un cop validats l\'username i pass', async () => {
            const res = await userService.login('loginuser', 'correctpass');
            expect(res.user.username).toBe('loginuser');
            expect(res.token).toBeDefined();
        });

        it('Donarà error d\'excepció si la contrasenya és incorrecte per seguretat', async () => {
            await expect(userService.login('loginuser', 'wrongpass')).rejects.toThrow('Usuari o contrasenya invàlits');
        });

        it('Donarà error similar si l\'usuari simple i llanerament no es troba present.', async () => {
            await expect(userService.login('non_existent', 'correctpass')).rejects.toThrow('Usuari o contrasenya invàlits');
        });
    });
});
