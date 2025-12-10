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
    public string Username { get; set; } = string.Empty;
    
    [Column("date_inscription")]
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    
    [Column("classement")]
    public int Rating { get; set; } = 1000;
    
    [Column("parties_jouees")]
    public int GamesPlayed { get; set; } = 0;
    
    [Column("parties_gagnees")]
    public int GamesWon { get; set; } = 0;
    
    [Column("total_points")]
    public int TotalPoints { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<DbMatch> MatchesAsNorth { get; set; } = new List<DbMatch>();
    public virtual ICollection<DbMatch> MatchesAsSouth { get; set; } = new List<DbMatch>();
}