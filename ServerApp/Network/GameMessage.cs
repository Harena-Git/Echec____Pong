using System.Text.Json;
using System.Text.Json.Serialization;
using ServerApp.Network;

namespace ServerApp.Network;

public abstract class GameMessage
{
    [JsonPropertyName("messageType")]
    public string MessageType { get; set; } = string.Empty;
    
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, GetType());
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
                    "gameConfig" => JsonSerializer.Deserialize<GameConfigMessage>(json),
                    "playerMove" => JsonSerializer.Deserialize<PlayerMoveMessage>(json),
                    "ballHit" => JsonSerializer.Deserialize<BallHitMessage>(json),
                    "gameStateUpdate" => JsonSerializer.Deserialize<GameStateUpdateMessage>(json),
                    "pieceDamaged" => JsonSerializer.Deserialize<PieceDamagedMessage>(json),
                    "matchEnd" => JsonSerializer.Deserialize<MatchEndMessage>(json),
                    "chatMessage" => JsonSerializer.Deserialize<ChatMessage>(json),
                    "ping" => JsonSerializer.Deserialize<PingMessage>(json),
                    "targetingUpdate" => JsonSerializer.Deserialize<TargetingUpdateMessage>(json),
                    _ => null
                };
            }
        }
        catch (JsonException) { }
        return null;
    }
}

public class JoinRequestMessage : GameMessage
{
    public JoinRequestMessage() => MessageType = "joinRequest";
    
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
}

public class JoinResponseMessage : GameMessage
{
    public JoinResponseMessage() => MessageType = "joinResponse";
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }
    
    [JsonPropertyName("playerName")]
    public string? PlayerName { get; set; }
    
    [JsonPropertyName("side")]
    public string? Side { get; set; }
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}

public class GameConfigMessage : GameMessage
{
    public GameConfigMessage() => MessageType = "gameConfig";
    
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }
    
    [JsonPropertyName("numberOfColumns")]
    public int NumberOfColumns { get; set; } = 8;
}

public class PlayerMoveMessage : GameMessage
{
    public PlayerMoveMessage() => MessageType = "playerMove";
    
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }
    
    [JsonPropertyName("positionX")]
    public float PositionX { get; set; }
}

public class BallHitMessage : GameMessage
{
    public BallHitMessage() => MessageType = "ballHit";
    
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }
    
    [JsonPropertyName("hitPower")]
    public float HitPower { get; set; }
    
    [JsonPropertyName("hitAngle")]
    public float HitAngle { get; set; }
    
    [JsonPropertyName("hitPositionX")]
    public float HitPositionX { get; set; }
}

public class GameStateUpdateMessage : GameMessage
{
    public GameStateUpdateMessage() => MessageType = "gameStateUpdate";
    
    [JsonPropertyName("gameState")]
    public GameState GameState { get; set; } = new();
}

public class PieceDamagedMessage : GameMessage
{
    public PieceDamagedMessage() => MessageType = "pieceDamaged";
    
    [JsonPropertyName("pieceId")]
    public int PieceId { get; set; }
    
    [JsonPropertyName("ownerPlayerId")]
    public int OwnerPlayerId { get; set; }
    
    [JsonPropertyName("pieceType")]
    public string PieceType { get; set; } = string.Empty;
    
    [JsonPropertyName("damage")]
    public int Damage { get; set; }
    
    [JsonPropertyName("remainingHealth")]
    public int RemainingHealth { get; set; }
    
    [JsonPropertyName("isDestroyed")]
    public bool IsDestroyed { get; set; }
    
    [JsonPropertyName("isKing")]
    public bool IsKing { get; set; }
}

public class MatchEndMessage : GameMessage
{
    public MatchEndMessage() => MessageType = "matchEnd";
    
    [JsonPropertyName("winnerPlayerId")]
    public int WinnerPlayerId { get; set; }
    
    [JsonPropertyName("winReason")]
    public string WinReason { get; set; } = string.Empty;
}

public class ChatMessage : GameMessage
{
    public ChatMessage() => MessageType = "chatMessage";
    
    [JsonPropertyName("playerId")]
    public int PlayerId { get; set; }
    
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class PingMessage : GameMessage
{
    public PingMessage() => MessageType = "ping";
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class TargetingUpdateMessage : GameMessage
{
    public TargetingUpdateMessage() => MessageType = "targetingUpdate";
    
    [JsonPropertyName("targetColumn")]
    public int TargetColumn { get; set; }
    
    [JsonPropertyName("targetSide")]
    public string TargetSide { get; set; } = string.Empty;
    
    [JsonPropertyName("targetPieceType")]
    public string TargetPieceType { get; set; } = string.Empty;
    
    [JsonPropertyName("canDefend")]
    public bool CanDefend { get; set; }
}
