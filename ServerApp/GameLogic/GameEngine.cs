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
        
        // Calculer la cible
        int targetColumn = CalculateTargetColumn(_gameState.Ball);
        
        // Vérifier si l'adversaire peut défendre
        var opponent = _gameState.Players.First(p => p.Side != player.Side);
        bool canDefend = CanPlayerDefendColumn(opponent, targetColumn);
        
        if (!canDefend)
        {
            // La balle va toucher une pièce
            var targetPiece = GetPieceAtColumn(
                isNorth ? _gameState.PiecesSouth : _gameState.PiecesNorth,
                targetColumn
            );
            
            if (targetPiece != null)
            {
                // Créer un message de collision
                var collisionMessage = new PieceDamagedMessage
                {
                    PieceId = targetPiece.Id,
                    OwnerPlayerId = opponent.Id,
                    PieceType = targetPiece.Type,
                    Damage = 1,
                    RemainingHealth = targetPiece.CurrentHealth - 1,
                    IsDestroyed = targetPiece.CurrentHealth <= 1,
                    IsKing = targetPiece.Type == "king"
                };
                
                // Appliquer les dégâts
                ApplyDamageToPiece(targetPiece, 1);
                
                // Vérifier la victoire
                if (targetPiece.Type == "king" && targetPiece.CurrentHealth <= 0)
                {
                    _gameState.Match.Status = "finished";
                    _gameState.Match.WinnerId = playerId;
                    _gameState.Match.WinReason = "king_captured";
                }
            }
        }
        
        return new GameStateUpdateMessage { GameState = _gameState };
    }
    
    public GameStateUpdateMessage UpdatePhysics(float deltaTime)
    {
        if (_gameState.Ball.State != "moving")
            return new GameStateUpdateMessage { GameState = _gameState };
        
        // Mettre à jour la position
        _gameState.Ball.PositionX += _gameState.Ball.VelocityX * deltaTime;
        _gameState.Ball.PositionY += _gameState.Ball.VelocityY * deltaTime;
        
        // Vérifier les collisions avec les bords
        if (_gameState.Ball.PositionX <= 0 || _gameState.Ball.PositionX >= 1)
        {
            // Balle sort du terrain
            _gameState.Ball.State = "idle";
            _gameState.Ball.PositionX = 0.5f;
            _gameState.Ball.PositionY = 0.5f;
            
            // Marquer un point si la balle est sortie
            var lastPlayer = _gameState.Players.First(p => p.Id == _gameState.Ball.LastPlayerId);
            if (lastPlayer != null)
            {
                if (lastPlayer.Side == "north")
                    _gameState.Match.ScoreNorth++;
                else
                    _gameState.Match.ScoreSouth++;
                
                // Vérifier victoire par score
                CheckScoreVictory();
            }
        }
        
        // Gravité
        _gameState.Ball.VelocityY -= 9.81f * deltaTime;
        
        return new GameStateUpdateMessage { GameState = _gameState };
    }
    
    private int CalculateTargetColumn(BallState ball)
    {
        // Position X normalisée (0-1) → colonne (0-7)
        float normalizedX = Math.Clamp(ball.PositionX, 0f, 1f);
        return (int)(normalizedX * 8);
    }
    
    private bool CanPlayerDefendColumn(PlayerState player, int column)
    {
        int playerColumn = (int)(player.PositionX * 8);
        return playerColumn == column;
    }
    
    private PieceState? GetPieceAtColumn(List<PieceState> pieces, int column)
    {
        return pieces.FirstOrDefault(p => 
            p.IsAlive && p.Column == column && p.Row == 0); // Toujours la rangée avant (la plus proche)
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