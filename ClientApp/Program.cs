using ClientApp.Client;
using ClientApp.Input;
using ClientApp.Render;

namespace ClientApp;

/// <summary>
/// Point d'entrée de l'application client
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // Configuration
        string serverIp = "localhost";
        int port = 8888;
        
        // Initialisation du client
        var gameClient = new GameClient();
        
        if (!gameClient.Connect(serverIp, port))
        {
            Console.WriteLine("Impossible de se connecter au serveur");
            return;
        }
        
        Console.WriteLine("Connecté au serveur !");
        
        // Initialisation des composants
        var keyboardHandler = new KeyboardHandler();
        var gameRenderer = new GameRenderer();
        
        // Démarrer l'écoute des mises à jour
        gameClient.StartListening();
        
        // Boucle principale
        keyboardHandler.ListenInput();
        
        // Nettoyage
        gameClient.Disconnect();
    }
}

