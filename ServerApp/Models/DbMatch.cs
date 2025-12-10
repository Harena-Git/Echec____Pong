using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("joueurs")]
public class DbPlayer
{
    [Key]
    [Column("id_joueur")]
    public int Id { get; set; }
    
    [Column("pseudo")]
    public string Name { get; set; } = string.Empty;
    
    [Column("date_inscription")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("classement")]
    public int Rating { get; set; } = 1000;
    
    public virtual ICollection<DbMatch> MatchesAsNorth { get; set; } = new List<DbMatch>();
    public virtual ICollection<DbMatch> MatchesAsSouth { get; set; } = new List<DbMatch>();
}