namespace GameSolution.Models;

public class Terrain
{
    public int IdTerrain { get; set; }
    public string NomTerrain { get; set; } = string.Empty;
    
    // Dimensions zone ping-pong
    public float LargeurPingPong { get; set; } = 2.74f;
    public float LongueurPingPong { get; set; } = 1.525f;
    
    // Dimensions zone échecs
    public float LargeurZoneEchecs { get; set; } = 2.74f;
    public float ProfondeurZoneEchecs { get; set; } = 1.0f;
    
    // Configuration des colonnes
    public int NombreColonnesPions { get; set; } = 8;
    public int NombreRangeesPions { get; set; } = 2;
    
    // Méthodes utilitaires
    public float GetLargeurTotale()
    {
        return LargeurPingPong;
    }
    
    public float GetLongueurTotale()
    {
        return LongueurPingPong + (ProfondeurZoneEchecs * 2);
    }
    
    public Terrain Clone()
    {
        return new Terrain
        {
            IdTerrain = IdTerrain,
            NomTerrain = NomTerrain,
            LargeurPingPong = LargeurPingPong,
            LongueurPingPong = LongueurPingPong,
            LargeurZoneEchecs = LargeurZoneEchecs,
            ProfondeurZoneEchecs = ProfondeurZoneEchecs,
            NombreColonnesPions = NombreColonnesPions,
            NombreRangeesPions = NombreRangeesPions
        };
    }
}