using ClientApp.Client;
using ClientApp.Game;
using ClientApp.Input;
using ClientApp.Render;

namespace ClientApp;

/// <summary>
/// Point d'entrée de l'application client
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Configuration
        string serverIp = "localhost";
        int port = 7777; // Port par défaut du serveur
        
        // Initialisation des composants
        var gameClient = new GameClient();
        var gameManager = new GameManager();
        gameManager.Initialize(gameClient);
        
        var gameRenderer = new GameRenderer(gameManager);
        var keyboardHandler = new KeyboardHandler(gameManager);
        
        // Connexion au serveur
        Console.WriteLine($"Connexion au serveur {serverIp}:{port}...");
        if (!await gameClient.ConnectAsync(serverIp, port))
        {
            Console.WriteLine("Impossible de se connecter au serveur");
            return;
        }
        
        Console.WriteLine("Connecté au serveur !");
        Console.WriteLine("Entrez votre nom: ");
        string? playerName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player" + Random.Shared.Next(1000, 9999);
        
        // Envoyer la demande de connexion
        var joinRequest = new ClientApp.Network.JoinRequestMessage { PlayerName = playerName };
        await gameClient.SendMessageAsync(joinRequest);
        
        // Configurer les événements
        keyboardHandler.OnMove += (delta) =>
        {
            if (gameManager.LocalPlayer != null)
            {
                float newX = Math.Clamp(gameManager.LocalPlayer.PositionX + delta, 0f, 1f);
                gameManager.UpdatePlayerPosition(newX, 0f);
            }
        };
        
        keyboardHandler.OnHit += (power, angle) =>
        {
            gameManager.SendBallHit(power, angle);
        };
        
        keyboardHandler.OnChat += (text) =>
        {
            gameManager.SendChat(text);
        };
        
        keyboardHandler.OnQuit += () =>
        {
            gameManager.Disconnect();
            Environment.Exit(0);
        };
        
        // Démarrer l'écoute du clavier
        keyboardHandler.StartListening();
        
        // Boucle principale (attendre que l'utilisateur quitte)
        Console.WriteLine("Appuyez sur 'Q' pour quitter...");
        while (gameClient.IsConnected)
        {
            await Task.Delay(100);
        }
        
        // Nettoyage
        gameClient.Disconnect();
    }
}

