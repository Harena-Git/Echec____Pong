using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("coup_pingpong")]
public class DbCoupPingPong
{
    [Key]
    [Column("id_coup")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    [Column("id_joueur")]
    public int PlayerId { get; set; }
    
    [Column("id_balle")]
    public int? BallId { get; set; }
    
    [Column("type_coup")]
    public string ShotType { get; set; } = "drive";
    
    [Column("position_frappe_x")]
    public float? HitPositionX { get; set; }
    
    [Column("position_frappe_y")]
    public float? HitPositionY { get; set; }
    
    [Column("puissance")]
    public float Power { get; set; } = 1.0f;
    
    [Column("precision")]
    public float Accuracy { get; set; } = 1.0f;
    
    [Column("angle")]
    public float? Angle { get; set; }
    
    [Column("reussi")]
    public bool Successful { get; set; } = true;
    
    [Column("resultat")]
    public string? Result { get; set; }
    
    // NOUVELLES COLONNES POUR LA CONCEPTION PARALLÈLE
    [Column("colonne_visee")]
    public int? TargetColumn { get; set; }
    
    [Column("colonne_atteinte")]
    public int? ActualColumn { get; set; }
    
    [Column("precision_tir")]
    public float? ShotAccuracy { get; set; }
    
    [Column("defense_reussie")]
    public bool DefenseSuccessful { get; set; }
    
    [Column("date_coup")]
    public DateTime ShotTime { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("MatchId")]
    public virtual DbMatch? Match { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual DbPlayer? Player { get; set; }
    
    [ForeignKey("BallId")]
    public virtual DbBall? Ball { get; set; }
    
    // Méthodes
    public float CalculateColumnAccuracy()
    {
        if (!TargetColumn.HasValue || !ActualColumn.HasValue)
            return 0f;
            
        int diff = Math.Abs(TargetColumn.Value - ActualColumn.Value);
        return Math.Max(0, 1 - (diff / 7f));
    }
    
    public bool WasPrecise() => ShotAccuracy.HasValue && ShotAccuracy.Value >= 0.7f;
}