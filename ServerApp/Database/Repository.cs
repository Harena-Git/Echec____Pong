using Microsoft.EntityFrameworkCore;
using PingPongChess.Server.Models;

namespace PingPongChess.Server.Database;

public class Repository
{
    private readonly DatabaseContext _context;
    
    public Repository(DatabaseContext context)
    {
        _context = context;
    }
    
    // Méthodes pour accéder aux données...
    public async Task<DbPlayer?> GetPlayerByIdAsync(int id)
    {
        return await _context.Players.FindAsync(id);
    }
    
    public async Task<DbMatch?> GetActiveMatchAsync()
    {
        return await _context.Matches
            .Include(m => m.PlayerNorth)
            .Include(m => m.PlayerSouth)
            .Include(m => m.Pieces)
            .FirstOrDefaultAsync(m => m.Status == "playing");
    }
    
    public async Task SaveMatchAsync(DbMatch match)
    {
        if (match.Id == 0)
            _context.Matches.Add(match);
        else
            _context.Matches.Update(match);
        
        await _context.SaveChangesAsync();
    }
}