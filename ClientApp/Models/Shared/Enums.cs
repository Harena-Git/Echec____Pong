namespace GameSolution.Shared;

public enum StatutJoueur
{
    Actif,
    Elimine,
    EnAttente
}

public enum TypePiece
{
    Roi,
    Reine,
    Tour,
    Fou,
    Cavalier,
    Pion
}

public enum StatutPiece
{
    Vivant,
    Blesse,
    Mort,
    Protege
}

public enum TypeCoup
{
    Service,
    Drive,
    Topspin,
    Contre,
    Lob,
    Amorti,
    Smash
}

public enum ResultatCoup
{
    Bon,
    Faute,
    Filet,
    HorsTable,
    CollisionPiece
}

public enum StatutMatch
{
    EnAttente,
    EnCours,
    Termine,
    Abandonne
}

public enum TourActuel
{
    PingPong,
    DeplacementPions
}

public enum RaisonVictoire
{
    RoiCapture,
    ScorePingPong,
    Abandon,
    TempsEcoule,
    PionsElimines
}

public enum TypeDeplacement
{
    Normal,
    Capture,
    Roque,
    Promotion
}

public enum EtatBalle
{
    EnJeu,
    HorsJeu,
    Collision,
    Perdue
}