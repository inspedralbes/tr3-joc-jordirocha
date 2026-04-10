// Càrrega de variables d'entorn si existeixen al directori root
require('dotenv').config();

const { Sequelize } = require('sequelize');
const createApp = require('./app');

// Connexió a la base de dades SQL utilitzant Sequelize. Rebutja paràmetres del codi i utilitza el proveït pel Contenidor Docker
const dbUri = process.env.DATABASE_URL || 'mysql://joc_user:joc_pass@localhost:3306/gameserver';

const sequelize = new Sequelize(dbUri, {
    dialect: 'mysql',
    logging: false // Evitar brutícia de console.log a la consola de producció
});

// Arrencar servidor API
const PORT = process.env.PORT || 3000;

async function bootstrap() {
    try {
        await sequelize.authenticate();
        console.log('✔ Connexió a la base de dades completada amb èxit.');

        const { app } = createApp(sequelize);

        // Sincronitzar estructures amb la BD real (Crearà taules si no n'hi ha)
        await sequelize.sync({ force: false, alter: true });
        console.log('✔ Models de dades en línia sincronitzats');

        app.listen(PORT, () => {
            console.log(`🚀 Microservei d'Usuaris en execució actiu al port ${PORT}`);
        });

    } catch (error) {
        console.error('❌ No s\'ha pogut arrancar el sevidor per error amb la DB:', error);
        process.exit(1);
    }
}

bootstrap();
