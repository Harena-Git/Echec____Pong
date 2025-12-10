using ClientApp.Network;

namespace ClientApp.Game;

public class GameManager
{
    private GameState _currentState = new();
    private LocalPlayer? _localPlayer;
    private GameClient? _gameClient;
    
    // NOUVEAU : Variables pour le ciblage
    private int _predictedTargetColumn = -1;
    private bool _canDefendTarget = false;
    private string _targetPieceType = string.Empty;
    
    public GameState CurrentState => _currentState;
    public LocalPlayer? LocalPlayer => _localPlayer;
    public bool IsConnected => _gameClient?.IsConnected ?? false;
    
    // NOUVEAU : Propriétés pour l'UI
    public int PredictedTargetColumn => _predictedTargetColumn;
    public bool CanDefendTarget => _canDefendTarget;
    public string TargetPieceType => _targetPieceType;
    
    public event Action<GameState>? OnGameStateUpdated;
    public event Action<string>? OnChatMessage;
    public event Action<string>? OnGameEvent;
    public event Action<int, bool>? OnTargetingUpdated; // NOUVEAU : ciblage
    
    public void Initialize(GameClient gameClient)
    {
        _gameClient = gameClient;
        _gameClient.OnMessageReceived += HandleNetworkMessage;
    }
    
    private void HandleNetworkMessage(GameMessage message)
    {
        switch (message)
        {
            case JoinResponseMessage joinResponse:
                HandleJoinResponse(joinResponse);
                break;
                
            case GameStateUpdateMessage stateUpdate:
                _currentState = stateUpdate.GameState;
                UpdateTargetingPrediction(); // NOUVEAU
                OnGameStateUpdated?.Invoke(_currentState);
                break;
                
            case TargetingUpdateMessage targetingUpdate: // NOUVEAU
                _predictedTargetColumn = targetingUpdate.TargetColumn;
                _canDefendTarget = targetingUpdate.CanDefend;
                _targetPieceType = targetingUpdate.TargetPieceType;
                OnTargetingUpdated?.Invoke(_predictedTargetColumn, _canDefendTarget);
                break;
                
            case PieceDamagedMessage pieceDamaged:
                OnGameEvent?.Invoke($"{pieceDamaged.PieceType} damaged! ({pieceDamaged.Damage} damage)");
                break;
                
            case MatchEndMessage matchEnd:
                OnGameEvent?.Invoke($"Game Over! Winner: Player {matchEnd.WinnerPlayerId} - {matchEnd.WinReason}");
                break;
                
            case ChatMessage chat:
                OnChatMessage?.Invoke($"{chat.PlayerName}: {chat.Text}");
                break;
        }
    }
    
    // NOUVELLE MÉTHODE : Mettre à jour la prédiction de ciblage
    private void UpdateTargetingPrediction()
    {
        if (_currentState.Ball?.State != "moving" || _localPlayer == null)
        {
            _predictedTargetColumn = -1;
            _canDefendTarget = false;
            _targetPieceType = string.Empty;
            return;
        }
        
        // Calculer la colonne ciblée
        var ball = _currentState.Ball;
        int targetColumn;
        
        if (ball.VelocityX > 0) // Va vers la droite
        {
            targetColumn = (int)(ball.PositionX * 8);
        }
        else // Va vers la gauche
        {
            targetColumn = (int)((1 - ball.PositionX) * 8);
        }
        
        _predictedTargetColumn = Math.Clamp(targetColumn, 0, 7);
        
        // Vérifier si le joueur local peut défendre
        var opponent = _currentState.Players.First(p => p.Side != _localPlayer.Side);
        _canDefendTarget = opponent.CurrentColumn == _predictedTargetColumn;
        
        // Trouver la pièce ciblée
        var targetPieces = _localPlayer.Side == "north" 
            ? _currentState.PiecesSouth 
            : _currentState.PiecesNorth;
            
        var targetPiece = targetPieces.FirstOrDefault(p => 
            p.Column == _predictedTargetColumn && p.Row == 0 && p.IsAlive);
            
        _targetPieceType = targetPiece?.Type ?? "none";
    }
    
    public void SendBallHit(float power, float angle)
    {
        if (_localPlayer == null || _gameClient == null) return;
        
        var message = new BallHitMessage
        {
            PlayerId = _localPlayer.Id,
            HitPower = power,
            HitAngle = angle,
            HitPositionX = _localPlayer.PositionX
        };
        
        _gameClient.SendMessageAsync(message);
    }
}

// NOUVELLE CLASSE : Pour les prédictions de trajectoire
public class TrajectoryPredictor
{
    public static List<(float x, float y)> PredictBallPath(
        float startX, float startY, float velocityX, float velocityY, int steps = 20)
    {
        var path = new List<(float x, float y)>();
        float x = startX, y = startY;
        float vx = velocityX, vy = velocityY;
        
        for (int i = 0; i < steps; i++)
        {
            x += vx * 0.1f;
            y += vy * 0.1f;
            vy -= 9.81f * 0.1f; // Gravité
            
            // Rebond sur le sol
            if (y < 0)
            {
                y = 0;
                vy = Math.Abs(vy) * 0.8f;
            }
            
            // Sort du terrain?
            if (x < 0 || x > 1)
                break;
                
            path.Add((x, y));
        }
        
        return path;
    }
    
    public static int PredictExitColumn(List<(float x, float y)> path)
    {
        if (path.Count == 0) return -1;
        
        var lastPoint = path.Last();
        
        if (lastPoint.x <= 0) return 0; // Sort gauche
        if (lastPoint.x >= 1) return 7; // Sort droite
        
        // Interpolation linéaire
        return (int)(lastPoint.x * 8);
    }
}