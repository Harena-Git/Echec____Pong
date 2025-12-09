using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GameSolution.Models;

[Table("configuration_pieces")]
public class ConfigurationPieces
{
    [Key]
    [Column("id_config")]
    public int IdConfig { get; set; }
    
    [Column("nom_config")]
    public string NomConfig { get; set; } = string.Empty;
    
    [Column("description")]
    public string? Description { get; set; }
    
    // Nombre de vies par type
    [Column("vies_roi")]
    public int ViesRoi { get; set; } = 3;
    
    [Column("vies_reine")]
    public int ViesReine { get; set; } = 2;
    
    [Column("vies_tour")]
    public int ViesTour { get; set; } = 2;
    
    [Column("vies_fou")]
    public int ViesFou { get; set; } = 1;
    
    [Column("vies_cavalier")]
    public int ViesCavalier { get; set; } = 1;
    
    [Column("vies_pion")]
    public int ViesPion { get; set; } = 1;
    
    [Column("position_initiale")]
    public string? PositionInitialeJson { get; set; }
    
    // Propriété pour désérialiser le JSON
    [NotMapped]
    public Dictionary<string, object>? PositionInitiale
    {
        get => PositionInitialeJson != null 
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(PositionInitialeJson) 
            : null;
        set => PositionInitialeJson = value != null 
            ? JsonSerializer.Serialize(value) 
            : null;
    }
    
    // Méthodes utilitaires
    public int GetViesParType(TypePiece type)
    {
        return type switch
        {
            TypePiece.Roi => ViesRoi,
            TypePiece.Reine => ViesReine,
            TypePiece.Tour => ViesTour,
            TypePiece.Fou => ViesFou,
            TypePiece.Cavalier => ViesCavalier,
            TypePiece.Pion => ViesPion,
            _ => 1
        };
    }
}