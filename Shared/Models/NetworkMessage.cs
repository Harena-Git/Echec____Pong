namespace Shared.Models;

/// <summary>
/// Modèle de message réseau
/// </summary>
public class NetworkMessage
{
    public string Type { get; set; } = "";
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

