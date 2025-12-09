using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSolution.Shared;

namespace GameSolution.Models;

[Table("piece_echecs")]
public class PieceEchecs
{
    [Key]
    [Column("id_piece")]
    public int IdPiece { get; set; }
    
    [Column("id_joueur")]
    public int IdJoueur { get; set; }
    
    [Column("id_match")]
    public int IdMatch { get; set; }
    
    [Column("type_piece")]
    public TypePiece TypePiece { get; set; }
    
    [Column("colonne")]
    public int Colonne { get; set; }
    
    [Column("rangee")]
    public int Rangee { get; set; }
    
    [Column("nombre_vies")]
    public int NombreVies { get; set; } = 1;
    
    [Column("vies_restantes")]
    public int ViesRestantes { get; set; }
    
    [Column("valeur")]
    public int Valeur { get; set; }
    
    [Column("statut")]
    public StatutPiece Statut { get; set; } = StatutPiece.Vivant;
    
    [Column("date_creation")]
    public DateTime DateCreation { get; set; } = DateTime.Now;
    
    // Navigation properties
    [ForeignKey("IdJoueur")]
    public virtual Joueur? Joueur { get; set; }
    
    [ForeignKey("IdMatch")]
    public virtual MatchHybride? Match { get; set; }
    
    // Méthodes
    public void SubirDegats(int degats)
    {
        ViesRestantes -= degats;
        if (ViesRestantes <= 0)
        {
            ViesRestantes = 0;
            Statut = StatutPiece.Mort;
        }
        else if (ViesRestantes == 1)
        {
            Statut = StatutPiece.Blesse;
        }
    }
    
    public bool EstVivante => Statut != StatutPiece.Mort;
}