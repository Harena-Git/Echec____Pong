namespace Shared.Protocol;

/// <summary>
/// Protocole de communication entre serveur et clients
/// </summary>
public class MessageProtocol
{
    public enum MessageType
    {
        CONNECTION,
        INPUT,
        GAME_STATE,
        MOVE,
        ERROR
    }
    
    /// <summary>
    /// Sérialise un message en JSON
    /// </summary>
    public static string SerializeMessage(MessageType type, object data)
    {
        // TODO: Sérialiser en JSON avec Newtonsoft.Json
        return "{}";
    }
    
    /// <summary>
    /// Désérialise un message JSON
    /// </summary>
    public static (MessageType type, object? data) DeserializeMessage(string json)
    {
        // TODO: Désérialiser depuis JSON
        return (MessageType.ERROR, null);
    }
}

