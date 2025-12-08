namespace ServerApp.Models;

/// <summary>
/// Modèle de données pour un joueur
/// </summary>
public class PlayerData
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime ConnectedAt { get; set; }
    public int? GameId { get; set; }
}

