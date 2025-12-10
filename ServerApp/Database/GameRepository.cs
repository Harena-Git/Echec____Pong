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
        var pieces = new List<DbPieceEchecs>();
        
        // Pièces pour le joueur Nord (baseRow = 0)
        // - Rangée arrière = 1, Rangée avant (pions) = 0
        pieces.AddRange(CreatePiecesForPlayer(matchId, playerNorthId, 0));
        
        // Pièces pour le joueur Sud (baseRow = 1)
        // - Rangée arrière = 0, Rangée avant (pions) = 1
        pieces.AddRange(CreatePiecesForPlayer(matchId, playerSouthId, 1));
        
        _context.Pieces.AddRange(pieces);
        await _context.SaveChangesAsync();
    }
    
    private List<DbPieceEchecs> CreatePiecesForPlayer(int matchId, int playerId, int baseRow)
    {
        var pieces = new List<DbPieceEchecs>();
        
        // Configuration standard
        string[] backRowTypes = { "tour", "cavalier", "fou", "reine", "roi", "fou", "cavalier", "tour" };
        int[] backRowHealth = { 2, 1, 1, 2, 3, 1, 1, 2 };
        
        // Pour le joueur Nord (baseRow = 0):
        // - Rangée arrière (pièces importantes) = rangée 1
        // - Rangée avant (pions) = rangée 0
        
        // Pour le joueur Sud (baseRow = 1):
        // - Rangée arrière (pièces importantes) = rangée 0
        // - Rangée avant (pions) = rangée 1
        
        int backRow = baseRow == 0 ? 1 : 0;  // Nord: rangée 1, Sud: rangée 0
        int frontRow = baseRow == 0 ? 0 : 1; // Nord: rangée 0, Sud: rangée 1
        
        // Rangée arrière (roi, reine, etc.)
        for (int col = 0; col < 8; col++)
        {
            pieces.Add(new DbPieceEchecs
            {
                MatchId = matchId,
                PlayerId = playerId,
                Type = backRowTypes[col],
                Column = col,
                Row = backRow,
                MaxHealth = backRowHealth[col],
                CurrentHealth = backRowHealth[col],
                Status = "vivant",
                Value = GetValueForType(backRowTypes[col]),
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Pions (rangée avant)
        for (int col = 0; col < 8; col++)
        {
            pieces.Add(new DbPieceEchecs
            {
                MatchId = matchId,
                PlayerId = playerId,
                Type = "pion",
                Column = col,
                Row = frontRow,
                MaxHealth = 1,
                CurrentHealth = 1,
                Status = "vivant",
                Value = 1,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        return pieces;
    }
    
    private int GetValueForType(string type)
    {
        return type switch
        {
            "roi" => 100,
            "reine" => 9,
            "tour" => 5,
            "fou" => 3,
            "cavalier" => 3,
            "pion" => 1,
            _ => 1
        };
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