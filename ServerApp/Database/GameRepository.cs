using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Database;

public class GameRepository
{
    private readonly DatabaseContext _context;
    
    public GameRepository(DatabaseContext context)
    {
        _context = context;
    }
    
    // Joueurs
    public async Task<DbPlayer?> GetPlayerByIdAsync(int id)
    {
        return await _context.Players.FindAsync(id);
    }
    
    public async Task<DbPlayer> CreatePlayerAsync(string name)
    {
        var player = new DbPlayer
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }
    
    // Matchs
    public async Task<DbMatch?> GetActiveMatchAsync()
    {
        return await _context.Matches
            .Include(m => m.PlayerNorth)
            .Include(m => m.PlayerSouth)
            .Include(m => m.Pieces)
            .FirstOrDefaultAsync(m => m.Status == "playing");
    }
    
    public async Task<DbMatch> CreateMatchAsync(int playerNorthId, int playerSouthId)
    {
        var match = new DbMatch
        {
            PlayerNorthId = playerNorthId,
            PlayerSouthId = playerSouthId,
            Status = "waiting",
            StartTime = null,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();
        return match;
    }
    
    public async Task UpdateMatchAsync(DbMatch match)
    {
        _context.Matches.Update(match);
        await _context.SaveChangesAsync();
    }
    
    // Pièces
    public async Task InitializePiecesForMatchAsync(int matchId, int playerNorthId, int playerSouthId)
    {
        var pieces = new List<DbPiece>();
        
        // Pièces pour le joueur Nord (rangée 0-1)
        pieces.AddRange(CreatePiecesForPlayer(matchId, playerNorthId, 0));
        // Pièces pour le joueur Sud (rangée 1-0 inversé)
        pieces.AddRange(CreatePiecesForPlayer(matchId, playerSouthId, 1));
        
        _context.Pieces.AddRange(pieces);
        await _context.SaveChangesAsync();
    }
    
    private List<DbPiece> CreatePiecesForPlayer(int matchId, int playerId, int baseRow)
    {
        var pieces = new List<DbPiece>();
        
        // Configuration standard
        string[] backRowTypes = { "rook", "knight", "bishop", "queen", "king", "bishop", "knight", "rook" };
        
        // Rangée arrière
        for (int col = 0; col < 8; col++)
        {
            pieces.Add(new DbPiece
            {
                MatchId = matchId,
                PlayerId = playerId,
                Type = backRowTypes[col],
                Column = col,
                Row = baseRow == 0 ? 0 : 1, // Inverse pour Sud
                Health = GetHealthForType(backRowTypes[col]),
                CurrentHealth = GetHealthForType(backRowTypes[col]),
                IsAlive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Pions
        for (int col = 0; col < 8; col++)
        {
            pieces.Add(new DbPiece
            {
                MatchId = matchId,
                PlayerId = playerId,
                Type = "pawn",
                Column = col,
                Row = baseRow == 0 ? 1 : 0, // Inverse pour Sud
                Health = 1,
                CurrentHealth = 1,
                IsAlive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        return pieces;
    }
    
    private int GetHealthForType(string type)
    {
        return type switch
        {
            "king" => 3,
            "queen" => 2,
            "rook" => 2,
            "bishop" => 1,
            "knight" => 1,
            "pawn" => 1,
            _ => 1
        };
    }
}