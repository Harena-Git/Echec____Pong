using System.Text.Json.Serialization;

namespace ServerApp.Network;

public class GameState
{
    public List<PlayerState> Players { get; set; } = new();
    public BallState? Ball { get; set; }
    public List<PieceState> PiecesNorth { get; set; } = new();
    public List<PieceState> PiecesSouth { get; set; } = new();
    public MatchInfo Match { get; set; } = new();
    public int NumberOfColumns { get; set; } = 8; // Configuration du nombre de colonnes
    
    // NOUVEAU : Information de ciblage
    public TargetingInfo? Targeting { get; set; }
}

public class PlayerState
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float PositionX { get; set; } = 0.5f;
    public float PaddleWidth { get; set; } = 0.15f; // Largeur de la raquette (rectangle)
    public string Side { get; set; } = "north";
    public int Score { get; set; }
    public bool IsServing { get; set; }
    public bool IsReady { get; set; }
    
    // NOUVEAU : Colonne actuelle (0-7)
    [JsonIgnore]
    public int CurrentColumn => (int)(PositionX * 7);
    
    // NOUVEAU : Limites de la raquette (pour collision)
    [JsonIgnore]
    public float PaddleLeft => Math.Max(0, PositionX - PaddleWidth / 2);
    [JsonIgnore]
    public float PaddleRight => Math.Min(1, PositionX + PaddleWidth / 2);
}

public class BallState
{
    public float PositionX { get; set; } = 0.5f;
    public float PositionY { get; set; } = 0.5f;
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public string State { get; set; } = "idle";
    public int? LastPlayerId { get; set; }
    
    // NOUVEAU : Information de trajectoire
    public float Angle { get; set; }
    public float Speed { get; set; }
    
    // NOUVEAU : Colonne ciblée (si la balle sort)
    [JsonIgnore]
    public int? TargetColumn => State == "moving" ? CalculateTargetColumn() : null;
    
    private int CalculateTargetColumn()
    {
        if (VelocityX > 0) // Va vers la droite
            return (int)(PositionX * 8);
        else // Va vers la gauche
            return (int)((1 - PositionX) * 8);
    }
}

public class PieceState
{
    public int Id { get; set; }
    public string Type { get; set; } = "pawn";
    public int Column { get; set; }
    public int Row { get; set; }
    public int MaxHealth { get; set; } = 1;
    public int CurrentHealth { get; set; } = 1;
    public bool IsAlive { get; set; } = true;
    
    // NOUVEAU : Est protégée par la raquette?
    public bool IsProtected { get; set; }
}

public class MatchInfo
{
    public string Status { get; set; } = "waiting";
    public int ScoreNorth { get; set; }
    public int ScoreSouth { get; set; }
    public int PointsToWin { get; set; } = 11;
    public bool KingNorthAlive { get; set; } = true;
    public bool KingSouthAlive { get; set; } = true;
    public string CurrentPhase { get; set; } = "pingpong";
    public int? CurrentPlayerTurn { get; set; }
    public DateTime? StartTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? WinnerId { get; set; }
    public string? WinReason { get; set; }
}

// NOUVELLE CLASSE : Information de ciblage
public class TargetingInfo
{
    public int TargetColumn { get; set; }
    public string TargetSide { get; set; } = string.Empty; // "north" or "south"
    public string TargetPieceType { get; set; } = string.Empty;
    public int TargetPieceHealth { get; set; }
    public bool CanDefend { get; set; }
    public int DefenderColumn { get; set; }
}