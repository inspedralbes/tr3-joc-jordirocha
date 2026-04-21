const { DataTypes } = require('sequelize');

module.exports = (sequelize) => {
    const Result = sequelize.define('Result', {
        id: {
            type: DataTypes.INTEGER,
            autoIncrement: true,
            primaryKey: true
        },
        winnerName: {
            type: DataTypes.STRING,
            allowNull: false
        },
        roomCode: {
            type: DataTypes.STRING,
            allowNull: false
        },
        duration: {
            type: DataTypes.FLOAT, // Temps en segons
            allowNull: false
        },
        date: {
            type: DataTypes.DATE,
            defaultValue: DataTypes.NOW
        }
    }, {
        tableName: 'results',
        timestamps: false // No necessitem updatedAt, ja tenim date.
    });

    return Result;
};
