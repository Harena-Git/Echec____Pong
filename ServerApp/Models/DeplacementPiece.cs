using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSolution.Shared;

namespace GameSolution.Models;

[Table("deplacement_piece")]
public class DeplacementPiece
{
    [Key]
    [Column("id_deplacement")]
    public int IdDeplacement { get; set; }
    
    [Column("id_match")]
    public int IdMatch { get; set; }
    
    [Column("id_joueur")]
    public int IdJoueur { get; set; }
    
    [Column("id_piece")]
    public int IdPiece { get; set; }
    
    [Column("colonne_depart")]
    public int ColonneDepart { get; set; }
    
    [Column("rangee_depart")]
    public int RangeeDepart { get; set; }
    
    [Column("colonne_arrivee")]
    public int ColonneArrivee { get; set; }
    
    [Column("rangee_arrivee")]
    public int RangeeArrivee { get; set; }
    
    [Column("type_deplacement")]
    public TypeDeplacement TypeDeplacement { get; set; } = TypeDeplacement.Normal;
    
    [Column("moment")]
    public DateTime Moment { get; set; } = DateTime.Now;
    
    // Navigation properties
    [ForeignKey("IdMatch")]
    public virtual MatchHybride? Match { get; set; }
    
    [ForeignKey("IdJoueur")]
    public virtual Joueur? Joueur { get; set; }
    
    [ForeignKey("IdPiece")]
    public virtual PieceEchecs? Piece { get; set; }
}