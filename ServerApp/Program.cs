using ServerApp.Server;
using ServerApp.Database;

namespace ServerApp;

/// <summary>
/// Point d'entrée de l'application serveur
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // Configuration
        int port = 8888;
        string dbConnectionString = "Host=localhost;Database=echec_pong;Username=postgres;Password=your_password";
        
        // Initialisation de la base de données
        var dbConnection = new DbConnection(dbConnectionString);
        dbConnection.Connect();
        
        // Initialisation du serveur
        var gameServer = new GameServer();
        gameServer.Start(port);
        
        Console.WriteLine($"Serveur démarré sur le port {port}");
        Console.WriteLine("Appuyez sur 'q' pour quitter...");
        
        // Boucle principale
        while (Console.ReadKey().KeyChar != 'q')
        {
            // TODO: Gestion des commandes serveur (affichage données, etc.)
        }
        
        // Nettoyage
        gameServer.Stop();
        dbConnection.Disconnect();
    }
}

