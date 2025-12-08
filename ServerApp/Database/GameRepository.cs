using Npgsql;
using ServerApp.Database;

namespace ServerApp.Database;

/// <summary>
/// Repository pour les opérations sur les données de jeu
/// </summary>
public class GameRepository
{
    private DbConnection _dbConnection;
    
    public GameRepository(DbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }
    
    /// <summary>
    /// Sauvegarde l'état actuel du jeu
    /// </summary>
    public async Task SaveGameState(int gameId, string gameDataJson)
    {
        // TODO: Insérer ou mettre à jour l'état du jeu dans la table games
    }
    
    /// <summary>
    /// Récupère l'état du jeu
    /// </summary>
    public async Task<string?> GetGameState(int gameId)
    {
        // TODO: Récupérer l'état du jeu depuis la base de données
        return null;
    }
    
    /// <summary>
    /// Enregistre un mouvement
    /// </summary>
    public async Task SaveMove(int gameId, int playerId, string moveDataJson)
    {
        // TODO: Insérer le mouvement dans la table moves
    }
}

