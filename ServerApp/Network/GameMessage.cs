using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerApp.Network;

public abstract class GameMessage
{
    public string MessageType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });
    }
    
    public static GameMessage? FromJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("messageType", out var typeProperty))
            {
                var type = typeProperty.GetString();
                return type switch
                {
                    "joinRequest" => JsonSerializer.Deserialize<JoinRequestMessage>(json),
                    "joinResponse" => JsonSerializer.Deserialize<JoinResponseMessage>(json),
                    "playerMove" => JsonSerializer.Deserialize<PlayerMoveMessage>(json),
                    "ballHit" => JsonSerializer.Deserialize<BallHitMessage>(json),
                    "gameStateUpdate" => JsonSerializer.Deserialize<GameStateUpdateMessage>(json),
                    "pieceDamaged" => JsonSerializer.Deserialize<PieceDamagedMessage>(json),
                    "matchEnd" => JsonSerializer.Deserialize<MatchEndMessage>(json),
                    "chatMessage" => JsonSerializer.Deserialize<ChatMessage>(json),
                    "ping" => JsonSerializer.Deserialize<PingMessage>(json),
                    _ => null
                };
            }
        }
        catch (JsonException) { }
        return null;
    }
}

// Messages clients → serveur
public class JoinRequestMessage : GameMessage
{
    public string PlayerName { get; set; } = string.Empty;
    
    public JoinRequestMessage() => MessageType = "joinRequest";
}

public class PlayerMoveMessage : GameMessage
{
    public int PlayerId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    
    public PlayerMoveMessage() => MessageType = "playerMove";
}

public class BallHitMessage : GameMessage
{
    public int PlayerId { get; set; }
    public float HitPower { get; set; } = 1.0f;
    public float HitAngle { get; set; }
    public float HitPositionX { get; set; }
    public float HitPositionY { get; set; }
    
    public BallHitMessage() => MessageType = "ballHit";
}

public class ChatMessage : GameMessage
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    
    public ChatMessage() => MessageType = "chatMessage";
}

// Messages serveur → clients
public class JoinResponseMessage : GameMessage
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int PlayerId { get; set; }
    public string? Side { get; set; } // "north" or "south"
    
    public JoinResponseMessage() => MessageType = "joinResponse";
}

public class GameStateUpdateMessage : GameMessage
{
    public GameState GameState { get; set; } = new();
    
    public GameStateUpdateMessage() => MessageType = "gameStateUpdate";
}

public class PieceDamagedMessage : GameMessage
{
    public int PieceId { get; set; }
    public int OwnerPlayerId { get; set; }
    public string PieceType { get; set; } = string.Empty;
    public int Damage { get; set; }
    public int RemainingHealth { get; set; }
    public bool IsDestroyed { get; set; }
    public bool IsKing { get; set; }
    
    public PieceDamagedMessage() => MessageType = "pieceDamaged";
}

public class MatchEndMessage : GameMessage
{
    public int WinnerPlayerId { get; set; }
    public string WinReason { get; set; } = string.Empty;
    
    public MatchEndMessage() => MessageType = "matchEnd";
}

public class PingMessage : GameMessage
{
    public long TimestampTicks { get; set; }
    
    public PingMessage()
    {
        MessageType = "ping";
        TimestampTicks = DateTime.UtcNow.Ticks;
    }
}