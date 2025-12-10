using PingPongChess.Network;

namespace PingPongChess.Client.Game;

public class GameManager
{
    private GameState _currentState = new();
    private Player? _localPlayer;
    private NetworkClient? _networkClient;
    
    public GameState CurrentState => _currentState;
    public Player? LocalPlayer => _localPlayer;
    public bool IsConnected => _networkClient?.IsConnected ?? false;
    
    public event Action<GameState>? OnGameStateUpdated;
    public event Action<string>? OnChatMessage;
    public event Action<string>? OnGameEvent;
    
    public async Task<bool> ConnectToServer(string ipAddress, int port, string playerName)
    {
        _networkClient = new NetworkClient();
        _networkClient.OnMessageReceived += HandleNetworkMessage;
        
        if (await _networkClient.ConnectAsync(ipAddress, port))
        {
            var joinRequest = new JoinRequestMessage { PlayerName = playerName };
            await _networkClient.SendMessageAsync(joinRequest);
            return true;
        }
        
        return false;
    }
    
    public void Disconnect()
    {
        _networkClient?.Disconnect();
    }
    
    public void SendPlayerMove(float x, float y)
    {
        if (_localPlayer == null || _networkClient == null) return;
        
        var message = new PlayerMoveMessage
        {
            PlayerId = _localPlayer.Id,
            PositionX = x,
            PositionY = y
        };
        
        _networkClient.SendMessageAsync(message);
    }
    
    public void SendBallHit(float power, float angle, float x, float y)
    {
        if (_localPlayer == null || _networkClient == null) return;
        
        var message = new BallHitMessage
        {
            PlayerId = _localPlayer.Id,
            HitPower = power,
            HitAngle = angle,
            HitPositionX = x,
            HitPositionY = y
        };
        
        _networkClient.SendMessageAsync(message);
    }
    
    public void SendChatMessage(string text)
    {
        if (_localPlayer == null || _networkClient == null) return;
        
        var message = new ChatMessage
        {
            PlayerId = _localPlayer.Id,
            PlayerName = _localPlayer.Name,
            Text = text
        };
        
        _networkClient.SendMessageAsync(message);
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
                OnGameEvent?.Invoke($"Piece {pieceDamaged.PieceType} damaged!");
                break;
                
            case MatchEndMessage matchEnd:
                OnGameEvent?.Invoke($"Match finished! Winner: Player {matchEnd.WinnerPlayerId}");
                break;
                
            case ChatMessage chat:
                OnChatMessage?.Invoke($"[{chat.PlayerName}]: {chat.Text}");
                break;
        }
    }
    
    private void HandleJoinResponse(JoinResponseMessage response)
    {
        if (response.Success)
        {
            _localPlayer = new Player
            {
                Id = response.PlayerId,
                Name = response.PlayerName ?? "Player",
                Side = response.Side ?? "north"
            };
            
            OnGameEvent?.Invoke($"Connected as {_localPlayer.Name} on {_localPlayer.Side} side");
        }
        else
        {
            OnGameEvent?.Invoke($"Connection failed: {response.ErrorMessage}");
        }
    }
}