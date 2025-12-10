using ClientApp.Client;
using ClientApp.Game;
using ClientApp.Input;
using ClientApp.Render;
using ClientApp.Network;

namespace ClientApp;

/// <summary>
/// Point d'entrée de l'application client
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Échec-Pong Client";
        
        // Configuration
        string? serverIp = null;
        int port = 7777; // Port par défaut du serveur
        
        // Menu de connexion
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║       ÉCHEC-PONG - CLIENT              ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Comment voulez-vous vous connecter ?");
        Console.WriteLine("1. Recherche automatique (réseau local)");
        Console.WriteLine("2. Connexion manuelle (localhost)");
        Console.WriteLine("3. Connexion manuelle (IP personnalisée)");
        Console.Write("\nVotre choix (1-3): ");
        
        var choice = Console.ReadLine();
        
        if (choice == "1")
        {
            // Découverte automatique
            var discovery = new ServerDiscovery();
            var servers = await discovery.FindServersAsync(3000);
            
            if (servers.Count == 0)
            {
                Console.WriteLine("❌ Aucun serveur trouvé sur le réseau local");
                Console.WriteLine("Essayez de vous connecter manuellement...");
                serverIp = "localhost";
            }
            else if (servers.Count == 1)
            {
                serverIp = servers[0].IpAddress;
                port = servers[0].Port;
                Console.WriteLine($"✅ Connexion au serveur: {servers[0].ServerName} ({serverIp}:{port})");
            }
            else
            {
                // Plusieurs serveurs trouvés
                Console.WriteLine($"\n{servers.Count} serveurs trouvés:");
                for (int i = 0; i < servers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {servers[i].ServerName} ({servers[i].IpAddress}:{servers[i].Port})");
                }
                Console.Write("\nChoisissez un serveur (1-" + servers.Count + "): ");
                if (int.TryParse(Console.ReadLine(), out int serverChoice) && 
                    serverChoice >= 1 && serverChoice <= servers.Count)
                {
                    serverIp = servers[serverChoice - 1].IpAddress;
                    port = servers[serverChoice - 1].Port;
                }
                else
                {
                    serverIp = servers[0].IpAddress;
                    port = servers[0].Port;
                }
            }
        }
        else if (choice == "2")
        {
            serverIp = "localhost";
        }
        else if (choice == "3")
        {
            Console.Write("Entrez l'adresse IP du serveur: ");
            serverIp = Console.ReadLine();
            Console.Write("Entrez le port (7777 par défaut): ");
            var portInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(portInput) && int.TryParse(portInput, out int customPort))
            {
                port = customPort;
            }
        }
        else
        {
            serverIp = "localhost";
        }
        
        if (string.IsNullOrWhiteSpace(serverIp))
        {
            Console.WriteLine("❌ Adresse IP invalide");
            return;
        }
        
        // Initialisation des composants
        var gameClient = new GameClient();
        var gameManager = new GameManager();
        gameManager.Initialize(gameClient);
        
        var gameRenderer = new GameRenderer(gameManager);
        var keyboardHandler = new KeyboardHandler(gameManager);
        
        // Connexion au serveur
        Console.WriteLine($"\n🔗 Connexion au serveur {serverIp}:{port}...");
        if (!await gameClient.ConnectAsync(serverIp, port))
        {
            Console.WriteLine("❌ Impossible de se connecter au serveur");
            Console.WriteLine("Vérifiez que le serveur est démarré et accessible.");
            Console.WriteLine("\nAppuyez sur une touche pour quitter...");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine("✅ Connecté au serveur !");
        Console.Write("Entrez votre nom: ");
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

