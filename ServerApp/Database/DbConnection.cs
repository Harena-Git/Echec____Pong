using Npgsql;

namespace ServerApp.Database;

/// <summary>
/// Gère la connexion à PostgreSQL
/// </summary>
public class DbConnection
{
    private string _connectionString;
    private NpgsqlConnection? _connection;
    
    public DbConnection(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    /// <summary>
    /// Établit la connexion à la base de données
    /// </summary>
    public void Connect()
    {
        // TODO: Créer et ouvrir la connexion Npgsql
    }
    
    /// <summary>
    /// Ferme la connexion
    /// </summary>
    public void Disconnect()
    {
        // TODO: Fermer la connexion
    }
    
    /// <summary>
    /// Exécute une requête SQL
    /// </summary>
    public async Task ExecuteQuery(string query)
    {
        // TODO: Exécuter la requête
    }
    
    /// <summary>
    /// Retourne la connexion active
    /// </summary>
    public NpgsqlConnection? GetConnection()
    {
        return _connection;
    }
}

