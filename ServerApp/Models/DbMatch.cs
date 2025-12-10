using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PingPongChess.Server.Models;

[Table("match_hybride")]
public class DbMatch
{
    [Key]
    [Column("id_match")]
    public int Id { get; set; }
    
    [Column("id_joueur_nord")]
    public int PlayerNorthId { get; set; }
    
    [Column("id_joueur_sud")]
    public int PlayerSouthId { get; set; }
    
    [Column("score_nord")]
    public int ScoreNorth { get; set; }
    
    [Column("score_sud")]
    public int ScoreSouth { get; set; }
    
    [Column("roi_nord_vivant")]
    public bool KingNorthAlive { get; set; } = true;
    
    [Column("roi_sud_vivant")]
    public bool KingSouthAlive { get; set; } = true;
    
    [Column("statut")]
    public string Status { get; set; } = "waiting";
    
    [Column("date_debut")]
    public DateTime? StartTime { get; set; }
    
    [Column("date_fin")]
    public DateTime? EndTime { get; set; }
    
    [Column("vainqueur")]
    public int? WinnerId { get; set; }
    
    [Column("raison_victoire")]
    public string? WinReason { get; set; }
    
    // Navigation properties
    [ForeignKey("PlayerNorthId")]
    public virtual DbPlayer? PlayerNorth { get; set; }
    
    [ForeignKey("PlayerSouthId")]
    public virtual DbPlayer? PlayerSouth { get; set; }
    
    [ForeignKey("WinnerId")]
    public virtual DbPlayer? Winner { get; set; }
    
    public virtual ICollection<DbPiece> Pieces { get; set; } = new List<DbPiece>();
    public virtual ICollection<DbBall> Balls { get; set; } = new List<DbBall>();
}