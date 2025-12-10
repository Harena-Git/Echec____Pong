using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("preferences_joueur")]
public class DbPlayerPreference
{
    [Key]
    [Column("id_preference")]
    public int Id { get; set; }
    
    [Column("id_joueur")]
    public int PlayerId { get; set; }
    
    [Column("colonne_preferee")]
    public int PreferredColumn { get; set; } = 4;
    
    [Column("strategie_defense")]
    public string DefenseStrategy { get; set; } = "mixte";
    
    [Column("sensibilite_controle")]
    public float ControlSensitivity { get; set; } = 1.0f;
    
    [Column("afficher_predictions")]
    public bool ShowPredictions { get; set; } = true;
    
    [Column("volume_effets")]
    public float EffectsVolume { get; set; } = 0.8f;
    
    [Column("couleur_raquette")]
    public string PaddleColor { get; set; } = "blanc";
    
    [Column("theme_interface")]
    public string InterfaceTheme { get; set; } = "classique";
    
    // Navigation property
    [ForeignKey("PlayerId")]
    public virtual DbPlayer? Player { get; set; }
    
    // Méthodes
    public float GetAdjustedSensitivity()
    {
        return ControlSensitivity * (DefenseStrategy == "agressive" ? 1.2f : 0.8f);
    }
}