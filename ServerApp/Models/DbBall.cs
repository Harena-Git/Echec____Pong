using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("balle")]
public class DbBall
{
    [Key]
    [Column("id_balle")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    // Position
    [Column("position_x")]
    public float PositionX { get; set; }
    
    [Column("position_y")]
    public float PositionY { get; set; }
    
    [Column("position_z")]
    public float PositionZ { get; set; } = 0f;
    
    // Vitesse
    [Column("vitesse_x")]
    public float VelocityX { get; set; }
    
    [Column("vitesse_y")]
    public float VelocityY { get; set; }
    
    [Column("vitesse_z")]
    public float VelocityZ { get; set; }
    
    // Propriétés
    [Column("etat")]
    public string State { get; set; } = "en_jeu";
    
    [Column("dernier_touche_par")]
    public int? LastTouchedBy { get; set; }
    
    [Column("force_impact")]
    public float ImpactForce { get; set; } = 1.0f;
    
    // NOUVELLES COLONNES POUR LA CONCEPTION PARALLÈLE
    [Column("colonne_sortie_predite")]
    public int? PredictedExitColumn { get; set; }
    
    [Column("angle_tir")]
    public float? ShotAngle { get; set; }
    
    [Column("puissance_tir")]
    public float ShotPower { get; set; } = 1.0f;
    
    // Timestamps
    [Column("date_creation")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("date_dernier_rebond")]
    public DateTime? LastBounceTime { get; set; }
    
    // Navigation properties
    [ForeignKey("MatchId")]
    public virtual DbMatch? Match { get; set; }
    
    [ForeignKey("LastTouchedBy")]
    public virtual DbPlayer? LastToucher { get; set; }
    
    // Méthodes
    public int CalculateExitColumn()
    {
        if (VelocityX > 0) // Va vers la droite
            return (int)(PositionX * 8);
        else // Va vers la gauche
            return (int)((1 - PositionX) * 8);
    }
    
    public void PredictExit()
    {
        PredictedExitColumn = CalculateExitColumn();
    }
    
    public bool WillExitThroughColumn(int column)
    {
        int exitCol = CalculateExitColumn();
        return exitCol == column;
    }
}