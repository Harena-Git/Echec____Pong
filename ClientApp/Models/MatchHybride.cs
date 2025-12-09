namespace GameSolution.Models;

public class MatchHybrideClient
{
    public int IdMatch { get; set; }
    public Terrain? Terrain { get; set; }
    public Joueur? JoueurNord { get; set; }
    public Joueur? JoueurSud { get; set; }
    public Joueur? JoueurActuel { get; set; }
    
    // Scores
    public int ScoreJoueurNord { get; set; }
    public int ScoreJoueurSud { get; set; }
    public int PointsPourGagner { get; set; } = 11;
    
    // État des pièces
    public bool RoiNordVivant { get; set; } = true;
    public bool RoiSudVivant { get; set; } = true;
    public int NombrePionsNord { get; set; } = 16;
    public int NombrePionsSud { get; set; } = 16;
    
    // Informations match
    public StatutMatch Statut { get; set; } = StatutMatch.EnAttente;
    public TourActuel TourActuel { get; set; } = TourActuel.PingPong;
    public Joueur? JoueurAuService { get; set; }
    
    // Listes dynamiques (pour l'affichage)
    public List<PieceEchecs> PiecesNord { get; set; } = new();
    public List<PieceEchecs> PiecesSud { get; set; } = new();
    public List<Balle> Balles { get; set; } = new();
    
    public DateTime DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    
    // Méthodes utilitaires pour le client
    public Joueur? GetAdversaire(Joueur joueur)
    {
        if (JoueurNord?.IdJoueur == joueur.IdJoueur) return JoueurSud;
        if (JoueurSud?.IdJoueur == joueur.IdJoueur) return JoueurNord;
        return null;
    }
    
    public List<PieceEchecs> GetPiecesJoueur(Joueur joueur)
    {
        if (JoueurNord?.IdJoueur == joueur.IdJoueur) return PiecesNord;
        if (JoueurSud?.IdJoueur == joueur.IdJoueur) return PiecesSud;
        return new List<PieceEchecs>();
    }
}