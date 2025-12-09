using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSolution.Models;

[Table("statistiques_joueur_match")]
public class StatistiquesJoueurMatch
{
    [Key]
    [Column("id_statistique")]
    public int IdStatistique { get; set; }
    
    [Column("id_match")]
    public int IdMatch { get; set; }
    
    [Column("id_joueur")]
    public int IdJoueur { get; set; }
    
    // Ping-pong
    [Column("coups_reussis")]
    public int CoupsReussis { get; set; }
    
    [Column("coups_rates")]
    public int CoupsRates { get; set; }
    
    [Column("services")]
    public int Services { get; set; }
    
    [Column("aces")]
    public int Aces { get; set; }
    
    [Column("fautes")]
    public int Fautes { get; set; }
    
    // Échecs
    [Column("pieces_protegees")]
    public int PiecesProtegees { get; set; }
    
    [Column("pieces_perdues")]
    public int PiecesPerdues { get; set; }
    
    [Column("degats_infliges")]
    public int DegatsInfliges { get; set; }
    
    [Column("degats_subis")]
    public int DegatsSubis { get; set; }
    
    // Hybrides
    [Column("pieces_sauvees")]
    public int PiecesSauvees { get; set; }
    
    [Column("collisions_evitees")]
    public int CollisionsEvitees { get; set; }
    
    [Column("date_maj")]
    public DateTime DateMaj { get; set; } = DateTime.Now;
    
    // Navigation properties
    [ForeignKey("IdMatch")]
    public virtual MatchHybride? Match { get; set; }
    
    [ForeignKey("IdJoueur")]
    public virtual Joueur? Joueur { get; set; }
    
    // Méthodes
    public void IncrementerCoupsReussis()
    {
        CoupsReussis++;
        DateMaj = DateTime.Now;
    }
    
    public void IncrementerPiecesPerdues()
    {
        PiecesPerdues++;
        DateMaj = DateTime.Now;
    }
}