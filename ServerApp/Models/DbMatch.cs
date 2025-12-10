using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("match_hybride")]
public class DbMatch
{
    [Key]
    [Column("id_match")]
    public int Id { get; set; }
    
    [Column("id_terrain")]
    public int? TerrainId { get; set; }
    
    [Column("id_joueur_nord")]
    public int PlayerNorthId { get; set; }
    
    [Column("id_joueur_sud")]
    public int PlayerSouthId { get; set; }
    
    // Scores ping-pong
    [Column("score_joueur_nord")]
    public int ScoreNorth { get; set; }
    
    [Column("score_joueur_sud")]
    public int ScoreSouth { get; set; }
    
    [Column("points_pour_gagner")]
    public int PointsToWin { get; set; } = 11;
    
    // État des pièces
    [Column("roi_nord_vivant")]
    public bool KingNorthAlive { get; set; } = true;
    
    [Column("roi_sud_vivant")]
    public bool KingSouthAlive { get; set; } = true;
    
    [Column("nombre_pions_nord")]
    public int PiecesNorthCount { get; set; } = 16;
    
    [Column("nombre_pions_sud")]
    public int PiecesSouthCount { get; set; } = 16;
    
    // Informations match
    [Column("statut")]
    public string Status { get; set; } = "en_attente";
    
    [Column("tour_actuel")]
    public string CurrentTurn { get; set; } = "pingpong";
    
    [Column("joueur_au_service")]
    public int? ServingPlayerId { get; set; }
    
    // NOUVELLES COLONNES POUR LA CONCEPTION PARALLÈLE
    [Column("colonne_service")]
    public int? ServiceColumn { get; set; }
    
    [Column("dernier_tir_colonne")]
    public int? LastShotColumn { get; set; }
    
    [Column("nombre_defenses")]
    public int DefenseCount { get; set; }
    
    // Conditions de victoire
    [Column("vainqueur")]
    public int? WinnerId { get; set; }
    
    [Column("raison_victoire")]
    public string? WinReason { get; set; }
    
    // Timestamps
    [Column("date_creation")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("date_debut")]
    public DateTime? StartTime { get; set; }
    
    [Column("date_fin")]
    public DateTime? EndTime { get; set; }
    
    [Column("duree_match")]
    public TimeSpan? Duration { get; set; }
    
    // Navigation properties
    [ForeignKey("TerrainId")]
    public virtual DbTerrain? Terrain { get; set; }
    
    [ForeignKey("PlayerNorthId")]
    public virtual DbPlayer? PlayerNorth { get; set; }
    
    [ForeignKey("PlayerSouthId")]
    public virtual DbPlayer? PlayerSouth { get; set; }
    
    [ForeignKey("ServingPlayerId")]
    public virtual DbPlayer? ServingPlayer { get; set; }
    
    [ForeignKey("WinnerId")]
    public virtual DbPlayer? Winner { get; set; }
    
    // Collections
    public virtual ICollection<DbPieceEchecs> Pieces { get; set; } = new List<DbPieceEchecs>();
    public virtual ICollection<DbBall> Balls { get; set; } = new List<DbBall>();
    public virtual ICollection<DbCoupPingPong> Shots { get; set; } = new List<DbCoupPingPong>();
    public virtual ICollection<DbCollisionPrecise> PreciseCollisions { get; set; } = new List<DbCollisionPrecise>();
    public virtual ICollection<DbDefenseStat> DefenseStats { get; set; } = new List<DbDefenseStat>();
    
    // Méthodes
    public bool IsActive => Status == "en_cours";
    public bool IsFinished => Status == "termine";
    
    public void Start()
    {
        StartTime = DateTime.UtcNow;
        Status = "en_cours";
        CurrentTurn = "pingpong";
    }
    
    public void End(int winnerId, string reason)
    {
        EndTime = DateTime.UtcNow;
        Duration = EndTime - StartTime;
        WinnerId = winnerId;
        WinReason = reason;
        Status = "termine";
    }
    
    public void IncrementDefenseCount()
    {
        DefenseCount++;
    }
    
    public void RecordShot(int column)
    {
        LastShotColumn = column;
    }
}