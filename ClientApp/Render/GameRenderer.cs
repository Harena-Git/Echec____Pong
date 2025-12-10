using ClientApp.Game;
using ClientApp.Network;

namespace ClientApp.Render;

public class GameRenderer
{
    private readonly GameManager _gameManager;
    
    public GameRenderer(GameManager gameManager)
    {
        _gameManager = gameManager;
        _gameManager.OnGameStateUpdated += OnGameStateUpdated;
        _gameManager.OnTargetingUpdated += OnTargetingUpdated;
    }
    
    public void Render()
    {
        Console.Clear();
        
        var state = _gameManager.CurrentState;
        var localPlayer = _gameManager.LocalPlayer;
        
        if (state == null || localPlayer == null)
        {
            RenderConnectionScreen();
            return;
        }
        
        // 1. En-tête
        RenderHeader(state);
        
        // 2. Zone Nord (échiquier + joueur)
        RenderNorthZone(state, localPlayer.Side == "north");
        
        // 3. Zone de jeu (ping-pong aligné)
        RenderGameZone(state, localPlayer);
        
        // 4. Zone Sud (échiquier + joueur)
        RenderSouthZone(state, localPlayer.Side == "south");
        
        // 5. Informations de ciblage
        RenderTargetingInfo();
        
        // 6. Commandes
        RenderControls();
    }
    
    private void RenderHeader(GameState state)
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                   ÉCHEC-PONG HYBRIDE                    │");
        Console.WriteLine("├─────────────────────────────────────────────────────────┤");
        
        var northPlayer = state.Players.FirstOrDefault(p => p.Side == "north");
        var southPlayer = state.Players.FirstOrDefault(p => p.Side == "south");
        
        Console.WriteLine($"│ Nord: {northPlayer?.Name ?? "..."} [{state.Match.ScoreNorth}]    " +
                         $"VS    Sud: {southPlayer?.Name ?? "..."} [{state.Match.ScoreSouth}] │");
        Console.WriteLine("└─────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }
    
    private void RenderNorthZone(GameState state, bool isLocalPlayer)
    {
        Console.WriteLine("┌───────────────────── NORD ─────────────────────┐");
        
        // Échiquier Nord (vue de dessus)
        RenderChessRow(state.PiecesNorth, 1, isLocalPlayer); // Rangée arrière
        RenderChessRow(state.PiecesNorth, 0, isLocalPlayer); // Rangée pions
        
        // Joueur Nord
        var northPlayer = state.Players.FirstOrDefault(p => p.Side == "north");
        if (northPlayer != null)
        {
            string marker = isLocalPlayer ? "[YOU]" : "[CPU]";
            Console.WriteLine($"│ Raquette: Col {northPlayer.CurrentColumn} {marker,-5}           │");
        }
        
        Console.WriteLine("└────────────────────────────────────────────────┘");
        Console.WriteLine();
    }
    
    private void RenderSouthZone(GameState state, bool isLocalPlayer)
    {
        Console.WriteLine();
        Console.WriteLine("┌───────────────────── SUD ──────────────────────┐");
        
        // Joueur Sud
        var southPlayer = state.Players.FirstOrDefault(p => p.Side == "south");
        if (southPlayer != null)
        {
            string marker = isLocalPlayer ? "[YOU]" : "[CPU]";
            Console.WriteLine($"│ Raquette: Col {southPlayer.CurrentColumn} {marker,-5}           │");
        }
        
        // Échiquier Sud (vue de dessus inversée)
        RenderChessRow(state.PiecesSouth, 0, isLocalPlayer); // Rangée pions
        RenderChessRow(state.PiecesSouth, 1, isLocalPlayer); // Rangée arrière
        
        Console.WriteLine("└────────────────────────────────────────────────┘");
    }
    
    private void RenderChessRow(List<PieceState> pieces, int row, bool isLocalPlayer)
    {
        var state = _gameManager.CurrentState;
        int numCols = state?.NumberOfColumns ?? 8;
        
        Console.Write("│ ");
        for (int col = 0; col < numCols; col++)
        {
            var piece = pieces.FirstOrDefault(p => 
                p.Row == row && p.Column == col && p.IsAlive);
                
            if (piece != null)
            {
                string symbol = GetPieceSymbol(piece.Type, row == 1);
                string health = new string('♥', piece.CurrentHealth);
                
                // Mettre en évidence si ciblée
                bool isTargeted = _gameManager.PredictedTargetColumn == col;
                if (isTargeted)
                {
                    Console.ForegroundColor = _gameManager.CanDefendTarget ? 
                        ConsoleColor.Green : ConsoleColor.Red;
                }
                
                Console.Write($"{symbol}{health} ");
                Console.ResetColor();
            }
            else
            {
                Console.Write("[  ] ");
            }
        }
        Console.WriteLine("│");
    }
    
    private void RenderGameZone(GameState state, LocalPlayer localPlayer)
    {
        Console.WriteLine("┌───────────────── ZONE DE JEU ─────────────────┐");
        
        // Ligne 1: Raquette Nord
        Console.Write("│ Nord: ");
        RenderPaddleLine(state.Players.First(p => p.Side == "north"), 
                        localPlayer.Side == "north");
        Console.WriteLine(" │");
        
        // Ligne 2: Filet
        Console.WriteLine("│      ─────────────────────────────────      │");
        
        // Ligne 3: Balle avec trajectoire
        Console.Write("│ Balle: ");
        if (state.Ball?.State == "moving")
        {
            RenderBallWithTrajectory(state.Ball);
        }
        else
        {
            Console.Write("● EN ATTENTE ");
        }
        Console.WriteLine(" │");
        
        // Ligne 4: Filet
        Console.WriteLine("│      ─────────────────────────────────      │");
        
        // Ligne 5: Raquette Sud
        Console.Write("│ Sud:   ");
        RenderPaddleLine(state.Players.First(p => p.Side == "south"), 
                        localPlayer.Side == "south");
        Console.WriteLine(" │");
        
        // Ligne 6: Légende colonnes
        Console.Write("│ Col:   ");
        for (int col = 0; col < 8; col++)
        {
            Console.Write($"{col,-3}");
        }
        Console.WriteLine(" │");
        
        Console.WriteLine("└───────────────────────────────────────────────┘");
    }
    
    private void RenderPaddleLine(PlayerState player, bool isLocalPlayer)
    {
        var state = _gameManager.CurrentState;
        int numCols = state?.NumberOfColumns ?? 8;
        float colWidth = 1.0f / numCols;
        
        for (int col = 0; col < numCols; col++)
        {
            float colStart = col * colWidth;
            float colEnd = (col + 1) * colWidth;
            
            // Vérifier si la raquette chevauche cette colonne
            bool paddleInColumn = player.PaddleLeft < colEnd && player.PaddleRight > colStart;
            
            if (paddleInColumn)
            {
                Console.ForegroundColor = isLocalPlayer ? ConsoleColor.Cyan : ConsoleColor.Yellow;
                Console.Write("[══]");
                Console.ResetColor();
            }
            else
            {
                Console.Write("    ");
            }
        }
    }
    
    private void RenderBallWithTrajectory(BallState ball)
    {
        var state = _gameManager.CurrentState;
        int numCols = state?.NumberOfColumns ?? 8;
        
        // Position actuelle et direction
        int ballCol = (int)(ball.PositionX * (numCols - 1));
        string direction = ball.VelocityX > 0 ? "→" : "←";
        
        Console.Write($"[{ball.PositionX:F2},{ball.PositionY:F2}] {direction} ");
        
        // Afficher la balle à sa position actuelle
        for (int col = 0; col < numCols; col++)
        {
            if (col == ballCol)
            {
                // Déterminer la couleur basée sur la direction
                Console.ForegroundColor = ball.VelocityX > 0 ? 
                    ConsoleColor.Red : ConsoleColor.Blue;
                Console.Write(" ● ");
                Console.ResetColor();
            }
            else
            {
                Console.Write("   ");
            }
        }
        
        // Indicateur de colonne cible
        Console.Write($" → Col{_gameManager.PredictedTargetColumn}");
    }
    
    private void RenderTargetingInfo()
    {
        if (_gameManager.PredictedTargetColumn < 0) return;
        
        Console.WriteLine();
        Console.WriteLine("┌────────────── CIBLAGE ──────────────┐");
        
        string defenseStatus = _gameManager.CanDefendTarget ? 
            "✅ PROTÉGÉ" : "❌ VULNÉRABLE";
            
        string pieceInfo = _gameManager.TargetPieceType != "none" ?
            $"Pièce: {_gameManager.TargetPieceType}" : "Colonne vide";
            
        Console.WriteLine($"│ Colonne {_gameManager.PredictedTargetColumn}: {defenseStatus} │");
        Console.WriteLine($"│ {pieceInfo,-30} │");
        
        if (!_gameManager.CanDefendTarget && _gameManager.TargetPieceType != "none")
        {
            Console.WriteLine("│ ⚠️  La balle va toucher cette pièce! │");
        }
        
        Console.WriteLine("└─────────────────────────────────────┘");
    }
    
    private void RenderControls()
    {
        Console.WriteLine();
        Console.WriteLine("┌────────────── COMMANDES ──────────────┐");
        Console.WriteLine("│ ← → : Déplacer raquette               │");
        Console.WriteLine("│ 🎯 FRAPPE AUTOMATIQUE sur collision   │");
        Console.WriteLine("│ C : Chat | Q : Quitter                │");
        Console.WriteLine("└───────────────────────────────────────┘");
    }
    
    private void RenderConnectionScreen()
    {
        Console.WriteLine("┌─────────────────────────────────────┐");
        Console.WriteLine("│      CONNEXION AU SERVEUR           │");
        Console.WriteLine("├─────────────────────────────────────┤");
        Console.WriteLine("│ En attente de connexion...          │");
        Console.WriteLine("│                                     │");
        Console.WriteLine("│ Vérifiez:                           │");
        Console.WriteLine("│ 1. Le serveur est démarré           │");
        Console.WriteLine("│ 2. L'adresse IP est correcte        │");
        Console.WriteLine("│ 3. Le port 7777 est ouvert          │");
        Console.WriteLine("└─────────────────────────────────────┘");
    }
    
    private string GetPieceSymbol(string type, bool isBackRow)
    {
        return type switch
        {
            "king" => isBackRow ? "♔" : "♚",
            "queen" => isBackRow ? "♕" : "♛",
            "rook" => isBackRow ? "♖" : "♜",
            "bishop" => isBackRow ? "♗" : "♝",
            "knight" => isBackRow ? "♘" : "♞",
            "pawn" => isBackRow ? "♙" : "♟",
            _ => "?"
        };
    }
    
    private void OnGameStateUpdated(GameState state)
    {
        Render();
    }
    
    private void OnTargetingUpdated(int column, bool canDefend)
    {
        // Rafraîchir juste la section ciblage
        Console.SetCursorPosition(0, 20);
        RenderTargetingInfo();
    }
}