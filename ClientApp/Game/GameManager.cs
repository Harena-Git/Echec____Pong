using ClientApp.Network;

namespace ClientApp.Game;

public class GameManager
{
    private GameState _currentState = new();
    private LocalPlayer? _localPlayer;
    private GameClient? _gameClient;
    
    public GameState CurrentState => _currentState;
    public LocalPlayer? LocalPlayer => _localPlayer;
    public bool IsConnected => _gameClient?.IsConnected ?? false;
    
    public event Action<GameState>? OnGameStateUpdated;
    public event Action<string>? OnChatMessage;
    public event Action<string>? OnGameEvent;
    
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
    
    public void UpdatePlayerPosition(float x, float y)
    {
        if (_localPlayer == null || _gameClient == null) return;
        
        _localPlayer.PositionX = x;
        _localPlayer.PositionY = y;
        
        var message = new PlayerMoveMessage
        {
            PlayerId = _localPlayer.Id,
            PositionX = x,
            PositionY = y
        };
        
        _gameClient.SendMessageAsync(message);
    }
    
    public void HitBall(float power, float angle, float x, float y)
    {
        if (_localPlayer == null || _gameClient == null) return;
        
        var message = new BallHitMessage
        {
            PlayerId = _localPlayer.Id,
            HitPower = power,
            HitAngle = angle,
            HitPositionX = x,
            HitPositionY = y
        };
        
        _gameClient.SendMessageAsync(message);
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