using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSolution.Shared;

namespace GameSolution.Models;

[Table("match_hybride")]
public class MatchHybride
{
    [Key]
    [Column("id_match")]
    public int IdMatch { get; set; }
    
    [Column("id_terrain")]
    public int IdTerrain { get; set; }
    
    [Column("id_joueur_nord")]
    public int IdJoueurNord { get; set; }
    
    [Column("id_joueur_sud")]
    public int IdJoueurSud { get; set; }
    
    // Scores ping-pong
    [Column("score_joueur_nord")]
    public int ScoreJoueurNord { get; set; }
    
    [Column("score_joueur_sud")]
    public int ScoreJoueurSud { get; set; }
    
    [Column("points_pour_gagner")]
    public int PointsPourGagner { get; set; } = 11;
    
    // État des pièces
    [Column("roi_nord_vivant")]
    public bool RoiNordVivant { get; set; } = true;
    
    [Column("roi_sud_vivant")]
    public bool RoiSudVivant { get; set; } = true;
    
    [Column("nombre_pions_nord")]
    public int NombrePionsNord { get; set; } = 16;
    
    [Column("nombre_pions_sud")]
    public int NombrePionsSud { get; set; } = 16;
    
    // Informations match
    [Column("statut")]
    public StatutMatch Statut { get; set; } = StatutMatch.EnAttente;
    
    [Column("tour_actuel")]
    public TourActuel TourActuel { get; set; } = TourActuel.PingPong;
    
    [Column("joueur_au_service")]
    public int? JoueurAuService { get; set; }
    
    [Column("date_debut")]
    public DateTime? DateDebut { get; set; }
    
    [Column("date_fin")]
    public DateTime? DateFin { get; set; }
    
    [Column("duree_match")]
    public TimeSpan? DureeMatch { get; set; }
    
    // Conditions de victoire
    [Column("vainqueur")]
    public int? Vainqueur { get; set; }
    
    [Column("raison_victoire")]
    public RaisonVictoire? RaisonVictoire { get; set; }
    
    // Navigation properties
    [ForeignKey("IdTerrain")]
    public virtual Terrain? Terrain { get; set; }
    
    [ForeignKey("IdJoueurNord")]
    public virtual Joueur? JoueurNord { get; set; }
    
    [ForeignKey("IdJoueurSud")]
    public virtual Joueur? JoueurSud { get; set; }
    
    [ForeignKey("JoueurAuService")]
    public virtual Joueur? ServiceJoueur { get; set; }
    
    [ForeignKey("Vainqueur")]
    public virtual Joueur? VainqueurJoueur { get; set; }
    
    // Collections
    public virtual ICollection<PieceEchecs> Pieces { get; set; } = new List<PieceEchecs>();
    public virtual ICollection<Balle> Balles { get; set; } = new List<Balle>();
    public virtual ICollection<CoupPingPong> Coups { get; set; } = new List<CoupPingPong>();
    public virtual ICollection<CollisionBallePiece> Collisions { get; set; } = new List<CollisionBallePiece>();
    
    // Méthodes
    public void DemarrerMatch()
    {
        DateDebut = DateTime.Now;
        Statut = StatutMatch.EnCours;
        TourActuel = TourActuel.PingPong;
    }
    
    public void TerminerMatch(int vainqueurId, RaisonVictoire raison)
    {
        DateFin = DateTime.Now;
        DureeMatch = DateFin - DateDebut;
        Vainqueur = vainqueurId;
        RaisonVictoire = raison;
        Statut = StatutMatch.Termine;
    }
    
    public void IncrementerScore(int joueurId)
    {
        if (joueurId == IdJoueurNord)
            ScoreJoueurNord++;
        else if (joueurId == IdJoueurSud)
            ScoreJoueurSud++;
    }
    
    public bool VerifierVictoirePingPong()
    {
        if (ScoreJoueurNord >= PointsPourGagner && 
            ScoreJoueurNord - ScoreJoueurSud >= 2)
        {
            TerminerMatch(IdJoueurNord, RaisonVictoire.ScorePingPong);
            return true;
        }
        
        if (ScoreJoueurSud >= PointsPourGagner && 
            ScoreJoueurSud - ScoreJoueurNord >= 2)
        {
            TerminerMatch(IdJoueurSud, RaisonVictoire.ScorePingPong);
            return true;
        }
        
        return false;
    }
}