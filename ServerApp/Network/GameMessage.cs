// Ajouter cette nouvelle classe de message
public class TargetingUpdateMessage : GameMessage
{
    public int TargetColumn { get; set; }
    public string TargetSide { get; set; } = string.Empty;
    public string TargetPieceType { get; set; } = string.Empty;
    public bool CanDefend { get; set; }
    
    public TargetingUpdateMessage() => MessageType = "targetingUpdate";
}

// Modifier la méthode FromJson pour inclure le nouveau message
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
                "targetingUpdate" => JsonSerializer.Deserialize<TargetingUpdateMessage>(json), // NOUVEAU
                _ => null
            };
        }
    }
    catch (JsonException) { }
    return null;
}