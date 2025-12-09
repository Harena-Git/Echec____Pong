namespace GameSolution.Models;

public class Joueur
{
    public int IdJoueur { get; set; }
    public string Pseudo { get; set; } = string.Empty;
    public float PositionX { get; set; } = 0.5f;
    public float PositionY { get; set; } = 0.5f;
    public StatutJoueur Statut { get; set; } = StatutJoueur.Actif;
    public string? CoteTerrain { get; set; } // "nord" ou "sud"
    
    // Pour la sérialisation réseau
    public Joueur Clone()
    {
        return new Joueur
        {
            IdJoueur = IdJoueur,
            Pseudo = Pseudo,
            PositionX = PositionX,
            PositionY = PositionY,
            Statut = Statut,
            CoteTerrain = CoteTerrain
        };
    }
}