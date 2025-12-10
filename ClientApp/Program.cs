using ClientApp.Client;
using ClientApp.Game;
using ClientApp.Input;
using ClientApp.Render;
using ClientApp.Network;
using ClientApp.UI;

namespace ClientApp;

/// <summary>
/// Point d'entrée de l'application client
/// </summary>
class Program
{
    private static UIManager? _uiManager;
    private static GameClient? _gameClient;
    private static GameManager? _gameManager;
    private static GameRenderer? _gameRenderer;
    private static KeyboardHandler? _keyboardHandler;
    private static bool _isInGame = false;
    
    static async Task Main(string[] args)
    {
        Console.Title = "Échec-Pong Client";
        
        // Configuration de la connexion
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
        _uiManager = new UIManager();
        _gameClient = new GameClient();
        _gameManager = new GameManager();
        _gameManager.Initialize(_gameClient);
        
        // Connexion au serveur
        Console.WriteLine($"\n🔗 Connexion au serveur {serverIp}:{port}...");
        if (!await _gameClient.ConnectAsync(serverIp, port))
        {
            Console.WriteLine("❌ Impossible de se connecter au serveur");
            Console.WriteLine("Vérifiez que le serveur est démarré et accessible.");
            Console.WriteLine("\nAppuyez sur une touche pour quitter...");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine("✅ Connecté au serveur !");
        
        // Écouter les messages réseau
        _gameClient.OnMessageReceived += OnMessageReceived;
        
        // PAGE 1: Saisie du nom
        _uiManager.ShowNameInputPage();
        string? playerName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player" + Random.Shared.Next(1000, 9999);
            
        _uiManager.PlayerName = playerName;
        
        // Envoyer la demande de connexion
        var joinRequest = new JoinRequestMessage { PlayerName = playerName };
        await _gameClient.SendMessageAsync(joinRequest);
        
        _uiManager.ShowWaitingMessage("En attente de la réponse du serveur...");
        
        // Attendre l'initialisation complète avant de démarrer le jeu
        while (!_isInGame && _gameClient.IsConnected)
        {
            await Task.Delay(100);
        }
        
        // Boucle principale
        while (_gameClient.IsConnected)
        {
            await Task.Delay(100);
        }
        
        // Nettoyage
        _gameClient.Disconnect();
    }
    
    private static async void OnMessageReceived(string json)
    {
        var message = GameMessage.FromJson(json);
        
        if (message is JoinResponseMessage joinResponse)
        {
            if (joinResponse.Success)
            {
                if (_uiManager != null)
                {
                    _uiManager.PlayerId = joinResponse.PlayerId;
                    _uiManager.PlayerSide = joinResponse.Side;
                    _uiManager.ShowSuccess($"Connecté en tant que {joinResponse.PlayerName} ({joinResponse.Side})");
                }
                
                await Task.Delay(1000);
                
                // Si c'est le joueur 1 (North), afficher la page de configuration
                if (joinResponse.Side == "north" && _uiManager != null)
                {
                    _uiManager.OnConfigSubmitted += OnConfigSubmitted;
                    _uiManager.ShowGameConfigPage();
                    
                    // Écouter les entrées pour la configuration
                    _ = Task.Run(() => HandleConfigInput());
                }
                else if (_uiManager != null)
                {
                    // Joueur 2 attend la configuration
                    _uiManager.ShowWaitingMessage("En attente de la configuration par le Joueur 1...");
                }
            }
            else
            {
                _uiManager?.ShowError(joinResponse.ErrorMessage ?? "Connexion refusée");
            }
        }
        else if (message is GameStateUpdateMessage stateUpdate)
        {
            // Quand le jeu commence, passer à la page de jeu
            if (!_isInGame && stateUpdate.GameState.Match.Status == "playing")
            {
                StartGame();
            }
        }
    }
    
    private static void HandleConfigInput()
    {
        while (_uiManager?.CurrentPage == UIManager.UIPage.GameConfig)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                _uiManager.HandleConfigInput(key.Key);
            }
            Thread.Sleep(50);
        }
    }
    
    private static async void OnConfigSubmitted(int numberOfColumns)
    {
        if (_gameClient == null || _uiManager == null) return;
        
        // Envoyer la configuration au serveur
        var configMessage = new GameConfigMessage
        {
            PlayerId = _uiManager.PlayerId ?? 0,
            NumberOfColumns = numberOfColumns
        };
        
        await _gameClient.SendMessageAsync(configMessage);
        _uiManager.ShowSuccess($"Configuration envoyée: {numberOfColumns} colonnes");
        _uiManager.ShowWaitingMessage("Démarrage du jeu...");
    }
    
    private static void StartGame()
    {
        _isInGame = true;
        
        if (_uiManager != null)
        {
            _uiManager.ShowInGamePage();
        }
        
        // Initialiser le renderer et le keyboard handler
        if (_gameManager != null)
        {
            _gameRenderer = new GameRenderer(_gameManager);
            _keyboardHandler = new KeyboardHandler(_gameManager);
            
            // Configurer les événements clavier
            _keyboardHandler.OnMove += (delta) =>
            {
                if (_gameManager.LocalPlayer != null)
                {
                    float newX = Math.Clamp(_gameManager.LocalPlayer.PositionX + delta, 0f, 1f);
                    _gameManager.UpdatePlayerPosition(newX, 0f);
                }
            };
            
            _keyboardHandler.OnChat += (text) =>
            {
                _gameManager.SendChat(text);
            };
            
            _keyboardHandler.OnQuit += () =>
            {
                _gameManager.Disconnect();
                Environment.Exit(0);
            };
            
            // Démarrer l'écoute du clavier
            _keyboardHandler.StartListening();
        }
    }
}

