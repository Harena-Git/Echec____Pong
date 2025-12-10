using System.Text.Json.Serialization;

namespace ClientApp.Network;

public class GameState
{
    public List<PlayerState> Players { get; set; } = new();
    public BallState? Ball { get; set; }
    public List<PieceState> PiecesNorth { get; set; } = new();
    public List<PieceState> PiecesSouth { get; set; } = new();
    public MatchInfo Match { get; set; } = new();
}

public class PlayerState
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string Side { get; set; } = "north"; // "north" or "south"
    public int Score { get; set; }
    public bool IsServing { get; set; }
    public bool IsReady { get; set; }
    
    // NOUVEAU : Colonne actuelle (0-7)
    [JsonIgnore]
    public int CurrentColumn => (int)(PositionX * 7);
}

public class BallState
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float VelocityZ { get; set; }
    public string State { get; set; } = "idle"; // "idle", "moving", "colliding"
    public int? LastPlayerId { get; set; }
}

public class PieceState
{
    public int Id { get; set; }
    public string Type { get; set; } = "pawn"; // "king", "queen", "rook", "bishop", "knight", "pawn"
    public int Column { get; set; } // 0-7
    public int Row { get; set; } // 0-1 (0 = front, 1 = back)
    public int MaxHealth { get; set; } = 1;
    public int CurrentHealth { get; set; } = 1;
    public bool IsAlive { get; set; } = true;
}

public class MatchInfo
{
    public string Status { get; set; } = "waiting"; // "waiting", "playing", "finished"
    public int ScoreNorth { get; set; }
    public int ScoreSouth { get; set; }
    public int PointsToWin { get; set; } = 11;
    public bool KingNorthAlive { get; set; } = true;
    public bool KingSouthAlive { get; set; } = true;
    public string CurrentPhase { get; set; } = "pingpong"; // "pingpong" or "chess_move"
    public int? CurrentPlayerTurn { get; set; }
    public DateTime? StartTime { get; set; }
    public TimeSpan? Duration { get; set; }
}