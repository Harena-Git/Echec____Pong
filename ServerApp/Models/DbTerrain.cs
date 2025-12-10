using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("terrains")]
public class DbTerrain
{
    [Key]
    [Column("id_terrain")]
    public int Id { get; set; }
    
    [Column("nom_terrain")]
    public string Name { get; set; } = string.Empty;
    
    [Column("largeur_pingpong")]
    public float PingPongWidth { get; set; } = 2.74f;
    
    [Column("longueur_pingpong")]
    public float PingPongLength { get; set; } = 1.525f;
    
    [Column("largeur_zone_echecs")]
    public float ChessZoneWidth { get; set; } = 2.74f;
    
    [Column("profondeur_zone_echecs")]
    public float ChessZoneDepth { get; set; } = 1.0f;
    
    [Column("nombre_colonnes_pions")]
    public int NumberOfColumns { get; set; } = 8;
    
    [Column("nombre_rangees_pions")]
    public int NumberOfRows { get; set; } = 2;
    
    [Column("couleur_surface")]
    public string SurfaceColor { get; set; } = "vert";
    
    [Column("date_creation")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<DbMatch> Matches { get; set; } = new List<DbMatch>();
}

