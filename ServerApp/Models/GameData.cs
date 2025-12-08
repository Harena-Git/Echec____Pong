namespace ServerApp.Models;

/// <summary>
/// Modèle de données pour une partie
/// </summary>
public class GameData
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "waiting"; // waiting, playing, finished
    public int? WinnerId { get; set; }
    public string GameStateJson { get; set; } = "{}";
}

