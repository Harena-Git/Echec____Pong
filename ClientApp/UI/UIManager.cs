using ClientApp.Network;

namespace ClientApp.UI;

/// <summary>
/// Gestionnaire d'interface utilisateur multi-page
/// </summary>
public class UIManager
{
    public enum UIPage
    {
        NameInput,      // Page 1: Saisie du nom
        GameConfig,     // Page 2: Configuration du jeu
        InGame          // Page 3: Jeu en cours
    }
    
    private UIPage _currentPage = UIPage.NameInput;
    public UIPage CurrentPage => _currentPage;
    
    public string? PlayerName { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerSide { get; set; }
    public int NumberOfColumns { get; set; } = 8;
    
    public event Action<string>? OnNameSubmitted;
    public event Action<int>? OnConfigSubmitted;
    
    public void ShowNameInputPage()
    {
        _currentPage = UIPage.NameInput;
        RenderNameInputPage();
    }
    
    public void ShowGameConfigPage()
    {
        _currentPage = UIPage.GameConfig;
        RenderGameConfigPage();
    }
    
    public void ShowInGamePage()
    {
        _currentPage = UIPage.InGame;
    }
    
    /// <summary>
    /// Page 1: Saisie du nom du joueur
    /// </summary>
    public void RenderNameInputPage()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              ÉCHEC-PONG - HYBRIDE                      ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                        ║");
        Console.WriteLine("║              PAGE 1: SAISIE DES NOMS                   ║");
        Console.WriteLine("║                                                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        if (PlayerSide != null)
        {
            Console.ForegroundColor = PlayerSide == "north" ? ConsoleColor.Cyan : ConsoleColor.Yellow;
            string sideText = PlayerSide == "north" ? "JOUEUR NORD" : "JOUEUR SUD";
            Console.WriteLine($"  Vous êtes: {sideText}");
            Console.ResetColor();
            Console.WriteLine();
        }
        
        Console.Write("  Entrez votre nom: ");
    }
    
    /// <summary>
    /// Page 2: Configuration du jeu (Player 1 uniquement)
    /// </summary>
    public void RenderGameConfigPage()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              ÉCHEC-PONG - HYBRIDE                      ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                        ║");
        Console.WriteLine("║         PAGE 2: CONFIGURATION DU JEU                   ║");
        Console.WriteLine("║                                                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        Console.WriteLine($"  Bienvenue, {PlayerName}!");
        Console.WriteLine();
        Console.WriteLine("  Nombre de colonnes de pions (2-8):");
        Console.WriteLine();
        
        // Afficher les options
        Console.Write("  ");
        for (int i = 2; i <= 8; i++)
        {
            if (i == NumberOfColumns)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{i}] ");
                Console.ResetColor();
            }
            else
            {
                Console.Write($" {i}  ");
            }
        }
        Console.WriteLine();
        Console.WriteLine();
        
        // Aperçu de l'échiquier
        Console.WriteLine("  Aperçu de l'échiquier:");
        Console.WriteLine();
        RenderChessboardPreview(NumberOfColumns);
        
        Console.WriteLine();
        Console.WriteLine("  Contrôles:");
        Console.WriteLine("  ← → : Changer le nombre de colonnes");
        Console.WriteLine("  [ENTRÉE] : Lancer la partie");
        Console.WriteLine();
    }
    
    private void RenderChessboardPreview(int columns)
    {
        string[] backPieces = { "♜", "♞", "♝", "♛", "♚", "♝", "♞", "♜" };
        string pawn = "♟";
        
        // Rangée arrière
        Console.Write("  Rangée 2: ");
        for (int i = 0; i < columns; i++)
        {
            Console.Write($"{backPieces[i]} ");
        }
        Console.WriteLine();
        
        // Rangée de pions
        Console.Write("  Rangée 1: ");
        for (int i = 0; i < columns; i++)
        {
            Console.Write($"{pawn} ");
        }
        Console.WriteLine();
    }
    
    /// <summary>
    /// Traite l'entrée utilisateur sur la page de configuration
    /// </summary>
    public void HandleConfigInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.LeftArrow:
                NumberOfColumns = Math.Max(2, NumberOfColumns - 1);
                RenderGameConfigPage();
                break;
            case ConsoleKey.RightArrow:
                NumberOfColumns = Math.Min(8, NumberOfColumns + 1);
                RenderGameConfigPage();
                break;
            case ConsoleKey.Enter:
                OnConfigSubmitted?.Invoke(NumberOfColumns);
                break;
        }
    }
    
    /// <summary>
    /// Affiche un message d'attente
    /// </summary>
    public void ShowWaitingMessage(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⏳ {message}");
        Console.ResetColor();
    }
    
    /// <summary>
    /// Affiche un message d'erreur
    /// </summary>
    public void ShowError(string error)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ❌ {error}");
        Console.ResetColor();
    }
    
    /// <summary>
    /// Affiche un message de succès
    /// </summary>
    public void ShowSuccess(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✅ {message}");
        Console.ResetColor();
    }
}
