using ServerApp.Models;
using ServerApp.Network;

namespace ServerApp.GameLogic;

public class GameEngine
{
    private GameState _gameState = new();
    private PhysicsEngine _physics = new();
    private readonly GameRepository _repository;
    
    public GameEngine(GameRepository repository)
    {
        _repository = repository;
    }
    
    public void InitializeMatch(int playerNorthId, int playerSouthId)
    {
        _gameState = new GameState
        {
            Players = new List<PlayerState>
            {
                new() { Id = playerNorthId, Name = "Nord", Side = "north", PositionX = 0.5f },
                new() { Id = playerSouthId, Name = "Sud", Side = "south", PositionX = 0.5f }
            },
            Ball = new BallState { PositionX = 0.5f, PositionY = 0.5f, State = "idle" },
            Match = new MatchInfo { Status = "playing", CurrentPhase = "pingpong" }
        };
        
        // Initialiser les pièces
        InitializePieces();
    }
    
    private void InitializePieces()
    {
        _gameState.PiecesNorth = CreatePiecesForSide("north");
        _gameState.PiecesSouth = CreatePiecesForSide("south");
    }
    
    private List<PieceState> CreatePiecesForSide(string side)
    {
        var pieces = new List<PieceState>();
        int pieceId = 1;
        
        // Rangée arrière (roi, reine, etc.)
        string[] backRowTypes = { "rook", "knight", "bishop", "queen", "king", "bishop", "knight", "rook" };
        int[] backRowHealth = { 2, 1, 1, 2, 3, 1, 1, 2 };
        
        for (int col = 0; col < 8; col++)
        {
            pieces.Add(new PieceState
            {
                Id = pieceId++,
                Type = backRowTypes[col],
                Column = col,
                Row = side == "north" ? 1 : 0, // Nord rangée 1, Sud rangée 0
                MaxHealth = backRowHealth[col],
                CurrentHealth = backRowHealth[col],
                IsAlive = true
            });
        }
        
        // Pions
        for (int col = 0; col < 8; col++)
        {
            pieces.Add(new PieceState
            {
                Id = pieceId++,
                Type = "pawn",
                Column = col,
                Row = side == "north" ? 0 : 1, // Nord rangée 0, Sud rangée 1
                MaxHealth = 1,
                CurrentHealth = 1,
                IsAlive = true
            });
        }
        
        return pieces;
    }
    
    public GameStateUpdateMessage ProcessPlayerMove(int playerId, float positionX)
    {
        var player = _gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player != null)
        {
            player.PositionX = Math.Clamp(positionX, 0f, 1f);
        }
        
        return new GameStateUpdateMessage { GameState = _gameState };
    }
    
    public GameStateUpdateMessage ProcessBallHit(int playerId, float power, float angle)
    {
        var player = _gameState.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null || _gameState.Ball.State != "idle")
            return new GameStateUpdateMessage { GameState = _gameState };
        
        // Déterminer la direction
        bool isNorth = player.Side == "north";
        float direction = isNorth ? 1f : -1f; // Nord → Sud, Sud → Nord
        
        // Lancer la balle
        _gameState.Ball.State = "moving";
        _gameState.Ball.VelocityX = MathF.Cos(angle * MathF.PI / 180f) * power * direction;
        _gameState.Ball.VelocityY = MathF.Sin(angle * MathF.PI / 180f) * power;
        _gameState.Ball.LastPlayerId = playerId;
        _gameState.Ball.Angle = angle;
        _gameState.Ball.Speed = power;
        
        // Prédire la colonne de sortie (mais ne pas toucher immédiatement)
        // La balle doit d'abord sortir du terrain avant de toucher une pièce
        int predictedExitColumn = PredictExitColumn(_gameState.Ball);
        _gameState.Ball.PositionX = player.PositionX; // Position initiale à la raquette
        
        return new GameStateUpdateMessage { GameState = _gameState };
    }
    
    public GameStateUpdateMessage UpdatePhysics(float deltaTime)
    {
        if (_gameState.Ball.State != "moving")
            return new GameStateUpdateMessage { GameState = _gameState };
        
        // Utiliser le moteur physique
        var (newX, newY, newVX, newVY) = _physics.UpdateBallPosition(
            _gameState.Ball.PositionX,
            _gameState.Ball.PositionY,
            _gameState.Ball.VelocityX,
            _gameState.Ball.VelocityY,
            deltaTime
        );
        
        _gameState.Ball.PositionX = newX;
        _gameState.Ball.PositionY = newY;
        _gameState.Ball.VelocityX = newVX;
        _gameState.Ball.VelocityY = newVY;
        
        // Vérifier si la balle sort du terrain (collision avec les bords latéraux)
        bool ballExitedLeft = _gameState.Ball.PositionX <= 0 && _gameState.Ball.VelocityX < 0;
        bool ballExitedRight = _gameState.Ball.PositionX >= 1 && _gameState.Ball.VelocityX > 0;
        
        if (ballExitedLeft || ballExitedRight)
        {
            // La balle sort du terrain - maintenant elle peut toucher une pièce
            int exitColumn = _physics.CalculateImpactColumn(_gameState.Ball.PositionX, _gameState.Ball.VelocityX);
            exitColumn = Math.Clamp(exitColumn, 0, 7);
            
            // Déterminer quel joueur a tiré et quel joueur est ciblé
            var lastPlayer = _gameState.Players.FirstOrDefault(p => p.Id == _gameState.Ball.LastPlayerId);
            if (lastPlayer != null)
            {
                bool isNorthShot = lastPlayer.Side == "north";
                var opponent = _gameState.Players.First(p => p.Side != lastPlayer.Side);
                
                // Vérifier si l'adversaire peut défendre cette colonne
                bool canDefend = CanPlayerDefendColumn(opponent, exitColumn);
                
                if (!canDefend)
                {
                    // La balle touche une pièce dans cette colonne
                    var targetPieces = isNorthShot ? _gameState.PiecesSouth : _gameState.PiecesNorth;
                    var targetPiece = GetPieceAtColumn(targetPieces, exitColumn);
                    
                    if (targetPiece != null && targetPiece.IsAlive)
                    {
                        // Appliquer les dégâts
                        ApplyDamageToPiece(targetPiece, 1);
                        
                        // Vérifier si c'est le roi
                        if (targetPiece.Type == "king" && targetPiece.CurrentHealth <= 0)
                        {
                            _gameState.Match.Status = "finished";
                            _gameState.Match.WinnerId = lastPlayer.Id;
                            _gameState.Match.WinReason = "king_captured";
                            if (isNorthShot)
                                _gameState.Match.KingSouthAlive = false;
                            else
                                _gameState.Match.KingNorthAlive = false;
                        }
                    }
                }
                else
                {
                    // Défense réussie - la balle est renvoyée
                    _gameState.Ball.VelocityX = -_gameState.Ball.VelocityX * 0.8f; // Rebond avec perte d'énergie
                    _gameState.Ball.LastPlayerId = opponent.Id;
                    return new GameStateUpdateMessage { GameState = _gameState };
                }
            }
            
            // Réinitialiser la balle après la collision ou la sortie
            _gameState.Ball.State = "idle";
            _gameState.Ball.PositionX = 0.5f;
            _gameState.Ball.PositionY = 0.5f;
            _gameState.Ball.VelocityX = 0;
            _gameState.Ball.VelocityY = 0;
            
            // Si la balle est sortie sans être défendue, marquer un point
            if (lastPlayer != null && canDefend == false)
            {
                if (lastPlayer.Side == "north")
                    _gameState.Match.ScoreNorth++;
                else
                    _gameState.Match.ScoreSouth++;
                
                // Vérifier victoire par score
                CheckScoreVictory();
            }
        }
        
        return new GameStateUpdateMessage { GameState = _gameState };
    }
    
    private int PredictExitColumn(BallState ball)
    {
        // Prédire où la balle va sortir en fonction de sa trajectoire
        // Si elle va vers la droite (vx > 0), elle sort par la droite
        // Si elle va vers la gauche (vx < 0), elle sort par la gauche
        if (ball.VelocityX > 0)
        {
            // Sort par la droite - colonne basée sur position Y actuelle
            return Math.Clamp((int)(ball.PositionX * 8), 0, 7);
        }
        else
        {
            // Sort par la gauche - colonne inversée
            return Math.Clamp((int)((1 - ball.PositionX) * 8), 0, 7);
        }
    }
    
    private bool CanPlayerDefendColumn(PlayerState player, int column)
    {
        int playerColumn = (int)(player.PositionX * 8);
        return playerColumn == column;
    }
    
    private PieceState? GetPieceAtColumn(List<PieceState> pieces, int column)
    {
        // Chercher d'abord dans la rangée avant (pions), puis dans la rangée arrière
        var frontRowPiece = pieces.FirstOrDefault(p => 
            p.IsAlive && p.Column == column && p.Row == 0);
        if (frontRowPiece != null)
            return frontRowPiece;
            
        // Si pas de pièce en avant, chercher en arrière
        return pieces.FirstOrDefault(p => 
            p.IsAlive && p.Column == column && p.Row == 1);
    }
    
    private void ApplyDamageToPiece(PieceState piece, int damage)
    {
        piece.CurrentHealth -= damage;
        if (piece.CurrentHealth <= 0)
        {
            piece.IsAlive = false;
            piece.CurrentHealth = 0;
        }
    }
    
    private void CheckScoreVictory()
    {
        if (_gameState.Match.ScoreNorth >= 11 && _gameState.Match.ScoreNorth - _gameState.Match.ScoreSouth >= 2)
        {
            _gameState.Match.Status = "finished";
            _gameState.Match.WinnerId = _gameState.Players.First(p => p.Side == "north").Id;
            _gameState.Match.WinReason = "score_reached";
        }
        else if (_gameState.Match.ScoreSouth >= 11 && _gameState.Match.ScoreSouth - _gameState.Match.ScoreNorth >= 2)
        {
            _gameState.Match.Status = "finished";
            _gameState.Match.WinnerId = _gameState.Players.First(p => p.Side == "south").Id;
            _gameState.Match.WinReason = "score_reached";
        }
    }
}