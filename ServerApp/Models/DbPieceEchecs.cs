using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("piece_echecs")]
public class DbPieceEchecs
{
    [Key]
    [Column("id_piece")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    [Column("id_joueur")]
    public int PlayerId { get; set; }
    
    [Column("type_piece")]
    public string Type { get; set; } = string.Empty;
    
    [Column("colonne")]
    public int Column { get; set; }
    
    [Column("rangee")]
    public int Row { get; set; }
    
    [Column("nombre_vies")]
    public int MaxHealth { get; set; } = 1;
    
    [Column("vies_restantes")]
    public int CurrentHealth { get; set; } = 1;
    
    [Column("valeur")]
    public int Value { get; set; } = 1;
    
    [Column("statut")]
    public string Status { get; set; } = "vivant";
    
    // NOUVELLES COLONNES POUR LA CONCEPTION PARALLÈLE
    [Column("colonne_protegee")]
    public bool IsColumnProtected { get; set; }
    
    [Column("derniere_colonne_touchee")]
    public int? LastColumnHit { get; set; }
    
    [Column("precision_position")]
    public float PositionAccuracy { get; set; } = 1.0f;
    
    [Column("date_creation")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("date_mort")]
    public DateTime? DestroyedAt { get; set; }
    
    // Navigation properties
    [ForeignKey("MatchId")]
    public virtual DbMatch? Match { get; set; }
    
    [ForeignKey("PlayerId")]
    public virtual DbPlayer? Player { get; set; }
    
    // Méthodes utilitaires
    public bool IsInColumn(int column) => Column == column;
    
    public bool IsInFrontRow() => (PlayerId == Match?.PlayerNorthId && Row == 0) || 
                                 (PlayerId == Match?.PlayerSouthId && Row == 1);
    
    public bool IsVulnerable() => IsInFrontRow() && !IsColumnProtected;
    
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Status = "mort";
            DestroyedAt = DateTime.UtcNow;
        }
        else
        {
            Status = "blesse";
        }
    }
}