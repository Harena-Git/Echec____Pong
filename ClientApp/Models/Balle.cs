namespace GameSolution.Models;

public class Balle
{
    public int IdBalle { get; set; }
    public int IdMatch { get; set; }
    
    public float PositionX { get; set; } // 0-1 (0 = côté sud, 1 = côté nord)
    public float PositionY { get; set; } // 0-1 (largeur)
    public float PositionZ { get; set; } = 0; // hauteur
    
    public float VitesseX { get; set; } = 0;
    public float VitesseY { get; set; } = 0;
    public float VitesseZ { get; set; } = 0;
    
    public EtatBalle Etat { get; set; } = EtatBalle.EnJeu;
    public int? DernierTouchePar { get; set; }
    public float ForceImpact { get; set; } = 1.0f;
    
    public DateTime MomentCreation { get; set; } = DateTime.Now;
    public DateTime? MomentDernierRebond { get; set; }
    
    // Pour la simulation physique
    public void AppliquerPhysique(float deltaTime)
    {
        PositionX += VitesseX * deltaTime;
        PositionY += VitesseY * deltaTime;
        PositionZ += VitesseZ * deltaTime;
        
        // Gravité
        VitesseZ -= 9.81f * deltaTime;
    }
    
    public Balle Clone()
    {
        return new Balle
        {
            IdBalle = IdBalle,
            IdMatch = IdMatch,
            PositionX = PositionX,
            PositionY = PositionY,
            PositionZ = PositionZ,
            VitesseX = VitesseX,
            VitesseY = VitesseY,
            VitesseZ = VitesseZ,
            Etat = Etat,
            DernierTouchePar = DernierTouchePar,
            ForceImpact = ForceImpact,
            MomentCreation = MomentCreation,
            MomentDernierRebond = MomentDernierRebond
        };
    }
}