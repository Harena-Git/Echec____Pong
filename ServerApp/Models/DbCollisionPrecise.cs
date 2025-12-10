using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("collision_precise")]
public class DbCollisionPrecise
{
    [Key]
    [Column("id_collision")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    [Column("id_balle")]
    public int BallId { get; set; }
    
    [Column("id_piece")]
    public int PieceId { get; set; }
    
    [Column("colonne_touchee")]
    public int HitColumn { get; set; }
    
    [Column("rangee_touchee")]
    public int HitRow { get; set; }
    
    [Column("precision_tir")]
    public float? ShotAccuracy { get; set; }
    
    [Column("defense_tentee")]
    public bool DefenseAttempted { get; set; }
    
    [Column("defense_reussie")]
    public bool DefenseSuccessful { get; set; }
    
    [Column("degats_infliges")]
    public int DamageDealt { get; set; } = 1;
    
    [Column("piece_eliminee")]
    public bool PieceEliminated { get; set; }
    
    [Column("moment_impact")]
    public DateTime ImpactTime { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("MatchId")]
    public virtual DbMatch? Match { get; set; }
    
    [ForeignKey("BallId")]
    public virtual DbBall? Ball { get; set; }
    
    [ForeignKey("PieceId")]
    public virtual DbPieceEchecs? Piece { get; set; }
    
    // Méthodes
    public bool WasCriticalHit() => ShotAccuracy.HasValue && ShotAccuracy.Value >= 0.9f;
    
    public bool WasDefended() => DefenseAttempted && DefenseSuccessful;
}