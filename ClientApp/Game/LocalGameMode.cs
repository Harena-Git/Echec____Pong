using ClientApp.Network;

namespace ClientApp.Game;

/// <summary>
/// Mode de jeu local sans serveur - pour tester l'interface rapidement.
/// Simule deux joueurs localement avec un état de jeu factice.
/// </summary>
public class LocalGameMode
{
    private GameState _currentState;
    private int _frameCounter = 0;
    
    public GameState CurrentState => _currentState;
    
    public event Action<GameState>? OnGameStateUpdated;
    
    public LocalGameMode()
    {
        _currentState = CreateInitialState();
    }
    
    /// <summary>
    /// Initialise un état de jeu de démonstration.
    /// </summary>
    private GameState CreateInitialState()
    {
        var state = new GameState
        {
            NumberOfColumns = 8,
            Match = new MatchInfo
            {
                Status = "playing",
                ScoreNorth = 0,
                ScoreSouth = 0
            },
            Ball = new BallState
            {
                PositionX = 0.5f,
                PositionY = 0.5f,
                VelocityX = 0.02f,
                VelocityY = 0.01f,
                State = "moving"
            },
            Players = new List<PlayerState>
            {
                new PlayerState
                {
                    Id = 1,
                    Name = "Joueur Nord",
                    Side = "north",
                    PositionX = 0.5f,
                    PaddleWidth = 0.15f
                },
                new PlayerState
                {
                    Id = 2,
                    Name = "Joueur Sud",
                    Side = "south",
                    PositionX = 0.5f,
                    PaddleWidth = 0.15f
                }
            }
        };
        
        // Créer les pièces pour chaque joueur
        state.PiecesNorth = CreateChessPieces("north", 8);
        state.PiecesSouth = CreateChessPieces("south", 8);
        
        return state;
    }
    
    /// <summary>
    /// Crée les pièces d'échecs pour un joueur.
    /// </summary>
    private List<PieceState> CreateChessPieces(string side, int columns)
    {
        var pieces = new List<PieceState>();
        string[] backRow = { "rook", "knight", "bishop", "queen", "king", "bishop", "knight", "rook" };
        int[] backRowHealth = { 2, 1, 1, 2, 3, 1, 1, 2 };
        
        // Rangée arrière (row 1)
        for (int i = 0; i < Math.Min(columns, 8); i++)
        {
            pieces.Add(new PieceState
            {
                Type = backRow[i],
                Row = 1,
                Column = i,
                IsAlive = true,
                MaxHealth = backRowHealth[i],
                CurrentHealth = backRowHealth[i]
            });
        }
        
        // Rangée de pions (row 0)
        for (int i = 0; i < columns; i++)
        {
            pieces.Add(new PieceState
            {
                Type = "pawn",
                Row = 0,
                Column = i,
                IsAlive = true,
                MaxHealth = 1,
                CurrentHealth = 1
            });
        }
        
        return pieces;
    }
    
    /// <summary>
    /// Met à jour la position du joueur local (simule le joueur Nord).
    /// </summary>
    public void UpdatePlayerPosition(float x)
    {
        var northPlayer = _currentState.Players.FirstOrDefault(p => p.Side == "north");
        if (northPlayer != null)
        {
            northPlayer.PositionX = Math.Clamp(x, 0f, 1f);
        }
    }
    
    /// <summary>
    /// Met à jour la physique du jeu (mouvement de la balle, collisions).
    /// </summary>
    public void Update()
    {
        if (_currentState.Ball == null || _currentState.Match.Status != "playing")
            return;
        
        var ball = _currentState.Ball;
        
        // Mouvement de la balle
        ball.PositionX += ball.VelocityX;
        ball.PositionY += ball.VelocityY;
        
        // Rebond sur les côtés
        if (ball.PositionX <= 0 || ball.PositionX >= 1)
        {
            ball.VelocityX = -ball.VelocityX;
            ball.PositionX = Math.Clamp(ball.PositionX, 0f, 1f);
        }
        
        // Collision avec raquette Nord (haut)
        if (ball.PositionY <= 0.05f)
        {
            var northPlayer = _currentState.Players.FirstOrDefault(p => p.Side == "north");
            if (northPlayer != null)
            {
                float paddleLeft = northPlayer.PositionX - northPlayer.PaddleWidth / 2;
                float paddleRight = northPlayer.PositionX + northPlayer.PaddleWidth / 2;
                
                if (ball.PositionX >= paddleLeft && ball.PositionX <= paddleRight)
                {
                    // Rebond réussi
                    ball.VelocityY = -ball.VelocityY;
                    ball.PositionY = 0.05f;
                }
                else
                {
                    // Raté - toucher une pièce Sud
                    HitRandomPiece(_currentState.PiecesSouth);
                    ResetBall();
                    _currentState.Match.ScoreSouth++;
                }
            }
        }
        
        // Collision avec raquette Sud (bas)
        if (ball.PositionY >= 0.95f)
        {
            var southPlayer = _currentState.Players.FirstOrDefault(p => p.Side == "south");
            if (southPlayer != null)
            {
                float paddleLeft = southPlayer.PositionX - southPlayer.PaddleWidth / 2;
                float paddleRight = southPlayer.PositionX + southPlayer.PaddleWidth / 2;
                
                if (ball.PositionX >= paddleLeft && ball.PositionX <= paddleRight)
                {
                    // Rebond réussi
                    ball.VelocityY = -ball.VelocityY;
                    ball.PositionY = 0.95f;
                }
                else
                {
                    // Raté - toucher une pièce Nord
                    HitRandomPiece(_currentState.PiecesNorth);
                    ResetBall();
                    _currentState.Match.ScoreNorth++;
                }
            }
        }
        
        // IA simple pour le joueur Sud
        _frameCounter++;
        if (_frameCounter % 5 == 0)
        {
            var southPlayer = _currentState.Players.FirstOrDefault(p => p.Side == "south");
            if (southPlayer != null)
            {
                // Suivre la balle
                float targetX = ball.PositionX;
                float diff = targetX - southPlayer.PositionX;
                southPlayer.PositionX += Math.Clamp(diff * 0.3f, -0.03f, 0.03f);
                southPlayer.PositionX = Math.Clamp(southPlayer.PositionX, 0f, 1f);
            }
        }
        
        // Notifier les observateurs
        OnGameStateUpdated?.Invoke(_currentState);
    }
    
    /// <summary>
    /// Touche une pièce aléatoire vivante.
    /// </summary>
    private void HitRandomPiece(List<PieceState> pieces)
    {
        var alivePieces = pieces.Where(p => p.IsAlive).ToList();
        if (alivePieces.Count > 0)
        {
            var piece = alivePieces[Random.Shared.Next(alivePieces.Count)];
            piece.CurrentHealth--;
            if (piece.CurrentHealth <= 0)
            {
                piece.IsAlive = false;
                
                // Vérifier victoire
                if (piece.Type == "king")
                {
                    _currentState.Match.Status = "finished";
                }
            }
        }
    }
    
    /// <summary>
    /// Réinitialise la balle au centre.
    /// </summary>
    private void ResetBall()
    {
        if (_currentState.Ball != null)
        {
            _currentState.Ball.PositionX = 0.5f;
            _currentState.Ball.PositionY = 0.5f;
            _currentState.Ball.VelocityX = Random.Shared.NextSingle() * 0.04f - 0.02f;
            _currentState.Ball.VelocityY = Random.Shared.Next(0, 2) == 0 ? -0.02f : 0.02f;
        }
    }
}
