using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("statistiques_defense")]
public class DbDefenseStat
{
    [Key]
    [Column("id_statistique")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    [Column("id_joueur")]
    public int PlayerId { get; set; }
    
    [Column("colonne_defendue")]
    public int DefendedColumn { get; set; }
    
    [Column("nombre_defenses")]
    public int DefenseCount { get; set; }
    
    [Column("nombre_tirs_subis")]
    public int ShotsFacedCount { get; set; }
    
    [Column("taux_reussite")]
    public float SuccessRate { get; set; }
    
    [Column("date_maj")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("MatchId")]
    public virtual DbMatch? Match { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual DbPlayer? Player { get; set; }
    
    // Méthodes
    public void UpdateSuccessRate()
    {
        if (ShotsFacedCount > 0)
            SuccessRate = (float)DefenseCount / ShotsFacedCount;
        else
            SuccessRate = 0f;
            
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RecordDefense(bool successful)
    {
        ShotsFacedCount++;
        if (successful)
            DefenseCount++;
            
        UpdateSuccessRate();
    }
    
    public bool IsStrongDefense() => SuccessRate >= 0.7f;
}