using System.Text.Json;
using System.Text.Json.Serialization;

namespace PingPongChess.Network;

public abstract class GameMessage
{
    public string MessageType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });
    }
    
    public static GameMessage? FromJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("MessageType", out var typeProperty))
            {
                var type = typeProperty.GetString();
                return type switch
                {
                    "JoinRequest" => JsonSerializer.Deserialize<JoinRequestMessage>(json),
                    "JoinResponse" => JsonSerializer.Deserialize<JoinResponseMessage>(json),
                    "PlayerMove" => JsonSerializer.Deserialize<PlayerMoveMessage>(json),
                    "BallHit" => JsonSerializer.Deserialize<BallHitMessage>(json),
                    "GameStateUpdate" => JsonSerializer.Deserialize<GameStateUpdateMessage>(json),
                    "PieceDamaged" => JsonSerializer.Deserialize<PieceDamagedMessage>(json),
                    "MatchEnd" => JsonSerializer.Deserialize<MatchEndMessage>(json),
                    "ChatMessage" => JsonSerializer.Deserialize<ChatMessage>(json),
                    "Ping" => JsonSerializer.Deserialize<PingMessage>(json),
                    _ => null
                };
            }
        }
        catch { }
        return null;
    }
}

// Messages spécifiques
public class JoinRequestMessage : GameMessage
{
    public string PlayerName { get; set; } = string.Empty;
    
    public JoinRequestMessage()
    {
        MessageType = "JoinRequest";
    }
}

public class JoinResponseMessage : GameMessage
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int PlayerId { get; set; }
    public string? Side { get; set; } // "north" or "south"
    
    public JoinResponseMessage()
    {
        MessageType = "JoinResponse";
    }
}

public class PlayerMoveMessage : GameMessage
{
    public int PlayerId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    
    public PlayerMoveMessage()
    {
        MessageType = "PlayerMove";
    }
}

public class BallHitMessage : GameMessage
{
    public int PlayerId { get; set; }
    public float HitPower { get; set; } = 1.0f;
    public float HitAngle { get; set; }
    public float HitPositionX { get; set; }
    public float HitPositionY { get; set; }
    
    public BallHitMessage()
    {
        MessageType = "BallHit";
    }
}

public class GameStateUpdateMessage : GameMessage
{
    public GameState GameState { get; set; } = new();
    
    public GameStateUpdateMessage()
    {
        MessageType = "GameStateUpdate";
    }
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
    
    public PieceDamagedMessage()
    {
        MessageType = "PieceDamaged";
    }
}

public class MatchEndMessage : GameMessage
{
    public int WinnerPlayerId { get; set; }
    public string WinReason { get; set; } = string.Empty; // "king_captured", "score_reached", etc.
    
    public MatchEndMessage()
    {
        MessageType = "MatchEnd";
    }
}

public class ChatMessage : GameMessage
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    
    public ChatMessage()
    {
        MessageType = "ChatMessage";
    }
}

public class PingMessage : GameMessage
{
    public long TimestampTicks { get; set; }
    
    public PingMessage()
    {
        MessageType = "Ping";
        TimestampTicks = DateTime.UtcNow.Ticks;
    }
}