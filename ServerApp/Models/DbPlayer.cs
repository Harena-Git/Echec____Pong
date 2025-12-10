// ServerApp/Models/DbPlayer.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerApp.Models;

[Table("joueur")]
public class DbPlayer
{
    [Key]
    [Column("id_joueur")]
    public int Id { get; set; }
    
    [Column("pseudo")]
    public string Username { get; set; } = string.Empty;
    
    [Column("date_inscription")]
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    
    [Column("classement")]
    public int Rating { get; set; } = 1000;
}

// ServerApp/Models/DbPiece.cs
[Table("piece_echecs")]
public class DbPiece
{
    [Key]
    [Column("id_piece")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    [Column("id_joueur")]
    public int PlayerId { get; set; }
    
    [Column("type_piece")]
    public string Type { get; set; } = "pawn";
    
    [Column("colonne")]
    public int Column { get; set; }
    
    [Column("rangee")]
    public int Row { get; set; }
    
    [Column("vies_restantes")]
    public int Health { get; set; } = 1;
    
    [Column("statut")]
    public string Status { get; set; } = "alive";
}

// ServerApp/Models/DbBall.cs
[Table("balle")]
public class DbBall
{
    [Key]
    [Column("id_balle")]
    public int Id { get; set; }
    
    [Column("id_match")]
    public int MatchId { get; set; }
    
    [Column("position_x")]
    public float PositionX { get; set; }
    
    [Column("position_y")]
    public float PositionY { get; set; }
    
    [Column("dernier_touche_par")]
    public int? LastTouchedBy { get; set; }
}