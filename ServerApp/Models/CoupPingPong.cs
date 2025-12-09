using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSolution.Shared;

namespace GameSolution.Models;

[Table("coup_pingpong")]
public class CoupPingPong
{
    [Key]
    [Column("id_coup")]
    public int IdCoup { get; set; }
    
    [Column("id_match")]
    public int IdMatch { get; set; }
    
    [Column("id_joueur")]
    public int IdJoueur { get; set; }
    
    [Column("id_balle")]
    public int? IdBalle { get; set; }
    
    [Column("type_coup")]
    public TypeCoup TypeCoup { get; set; }
    
    [Column("position_frappe_x")]
    public float? PositionFrappeX { get; set; }
    
    [Column("position_frappe_y")]
    public float? PositionFrappeY { get; set; }
    
    [Column("puissance")]
    public float Puissance { get; set; } = 1.0f;
    
    [Column("precision")]
    public float Precision { get; set; } = 1.0f;
    
    [Column("angle")]
    public float? Angle { get; set; }
    
    [Column("reussi")]
    public bool Reussi { get; set; } = true;
    
    [Column("resultat")]
    public ResultatCoup? Resultat { get; set; }
    
    [Column("moment")]
    public DateTime Moment { get; set; } = DateTime.Now;
    
    // Navigation properties
    [ForeignKey("IdMatch")]
    public virtual MatchHybride? Match { get; set; }
    
    [ForeignKey("IdJoueur")]
    public virtual Joueur? Joueur { get; set; }
    
    [ForeignKey("IdBalle")]
    public virtual Balle? Balle { get; set; }
}