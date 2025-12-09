using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSolution.Models;

[Table("collision_balle_piece")]
public class CollisionBallePiece
{
    [Key]
    [Column("id_collision")]
    public int IdCollision { get; set; }
    
    [Column("id_match")]
    public int IdMatch { get; set; }
    
    [Column("id_balle")]
    public int IdBalle { get; set; }
    
    [Column("id_piece")]
    public int IdPiece { get; set; }
    
    [Column("id_joueur_proprietaire")]
    public int IdJoueurProprietaire { get; set; }
    
    [Column("point_impact_x")]
    public float? PointImpactX { get; set; }
    
    [Column("point_impact_y")]
    public float? PointImpactY { get; set; }
    
    [Column("force_impact")]
    public float ForceImpact { get; set; }
    
    [Column("degats_infliges")]
    public int DegatsInfliges { get; set; } = 1;
    
    [Column("vies_piece_avant")]
    public int? ViesPieceAvant { get; set; }
    
    [Column("vies_piece_apres")]
    public int? ViesPieceApres { get; set; }
    
    [Column("piece_eliminee")]
    public bool PieceEliminee { get; set; }
    
    [Column("roi_touche")]
    public bool RoiTouche { get; set; }
    
    [Column("moment")]
    public DateTime Moment { get; set; } = DateTime.Now;
    
    // Navigation properties
    [ForeignKey("IdMatch")]
    public virtual MatchHybride? Match { get; set; }
    
    [ForeignKey("IdBalle")]
    public virtual Balle? Balle { get; set; }
    
    [ForeignKey("IdPiece")]
    public virtual PieceEchecs? Piece { get; set; }
    
    [ForeignKey("IdJoueurProprietaire")]
    public virtual Joueur? JoueurProprietaire { get; set; }
}