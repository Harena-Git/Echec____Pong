using ClientApp.Network;
using ClientApp.Client;

namespace ClientApp.Game;

public class GameManager
{
    private GameState _currentState = new();
    private LocalPlayer? _localPlayer;
    private GameClient? _gameClient;
    
    // Ciblage
    private int _predictedTargetColumn = -1;
    private bool _canDefendTarget;
    private string _targetPieceType = string.Empty;
    
    public GameState CurrentState => _currentState;
    public LocalPlayer? LocalPlayer => _localPlayer;
    public bool IsConnected => _gameClient?.IsConnected ?? false;
    public int PredictedTargetColumn => _predictedTargetColumn;
    public bool CanDefendTarget => _canDefendTarget;
    public string TargetPieceType => _targetPieceType;
    
    public event Action<GameState>? OnGameStateUpdated;
    public event Action<string>? OnChatMessage;
    public event Action<string>? OnGameEvent;
    public event Action<int, bool>? OnTargetingUpdated;
    
    public void Initialize(GameClient gameClient)
    {
        _gameClient = gameClient;
        _gameClient.OnMessageReceived += HandleNetworkMessage;
    }
    
    public async Task<bool> ConnectToServer(string ipAddress, int port, string playerName)
    {
        if (_gameClient == null) return false;
        
        if (await _gameClient.ConnectAsync(ipAddress, port))
        {
            var joinRequest = new JoinRequestMessage { PlayerName = playerName };
            await _gameClient.SendMessageAsync(joinRequest);
            return true;
        }
        
        return false;
    }
    
    public void Disconnect()
    {
        _gameClient?.Disconnect();
        _localPlayer = null;
    }
    
    public void UpdatePlayerPosition(float x, float y = 0f)
    {
        if (_localPlayer == null || _gameClient == null) return;
        
        _localPlayer.PositionX = Math.Clamp(x, 0f, 1f);
        _localPlayer.PositionY = y;
        
        var message = new PlayerMoveMessage
        {
            PlayerId = _localPlayer.Id,
            PositionX = _localPlayer.PositionX
        };
        
        _ = _gameClient.SendMessageAsync(message);
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
        
        _ = _gameClient.SendMessageAsync(message);
    }
    
    public void SendChat(string text)
    {
        if (_localPlayer == null || _gameClient == null) return;
        
        var message = new ChatMessage
        {
            PlayerId = _localPlayer.Id,
            PlayerName = _localPlayer.Name,
            Text = text
        };
        
        _gameClient.SendMessageAsync(message);
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
                UpdateTargetingPrediction();
                OnGameStateUpdated?.Invoke(_currentState);
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
                
            case PingMessage ping:
                // Répondre au ping automatiquement
                if (_gameClient != null)
                {
                    _gameClient.SendMessageAsync(new PingMessage());
                }
                break;
        }
    }
    
    /// <summary>
    /// Calcule la colonne ciblée par la balle et si elle est défendable.
    /// </summary>
    private void UpdateTargetingPrediction()
    {
        if (_currentState.Ball?.State != "moving" || _localPlayer == null)
        {
            _predictedTargetColumn = -1;
            _canDefendTarget = false;
            _targetPieceType = string.Empty;
            return;
        }
        
        var ball = _currentState.Ball;
        int targetColumn = ball.VelocityX > 0
            ? (int)(ball.PositionX * 8)
            : (int)((1 - ball.PositionX) * 8);
        
        _predictedTargetColumn = Math.Clamp(targetColumn, 0, 7);
        
        // Joueur qui peut défendre : l'adversaire
        var opponent = _currentState.Players.FirstOrDefault(p => p.Side != _localPlayer.Side);
        _canDefendTarget = opponent != null && opponent.CurrentColumn == _predictedTargetColumn;
        
        // Pièce visée : rangée avant prioritaire
        var targetPieces = _localPlayer.Side == "north"
            ? _currentState.PiecesSouth
            : _currentState.PiecesNorth;
        
        var targetPiece = targetPieces.FirstOrDefault(p =>
            p.IsAlive && p.Column == _predictedTargetColumn && p.Row == 0)
            ?? targetPieces.FirstOrDefault(p =>
            p.IsAlive && p.Column == _predictedTargetColumn && p.Row == 1);
        
        _targetPieceType = targetPiece?.Type ?? "none";
        
        OnTargetingUpdated?.Invoke(_predictedTargetColumn, _canDefendTarget);
    }
    
    private void HandleJoinResponse(JoinResponseMessage response)
    {
        if (response.Success)
        {
            _localPlayer = new LocalPlayer
            {
                Id = response.PlayerId,
                Name = response.PlayerName ?? "Player",
                Side = response.Side ?? "north"
            };
            
            OnGameEvent?.Invoke($"Connected successfully as {_localPlayer.Name}");
        }
        else
        {
            OnGameEvent?.Invoke($"Connection failed: {response.ErrorMessage}");
        }
    }
}

public class LocalPlayer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Side { get; set; } = "north";
    public float PositionX { get; set; } = 0.5f;
    public float PositionY { get; set; } = 0.5f;
    public bool IsReady { get; set; }
}