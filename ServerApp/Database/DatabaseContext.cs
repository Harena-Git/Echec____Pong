using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Database;

public class DatabaseContext : DbContext
{
    public DbSet<DbMatch> Matches { get; set; }
    public DbSet<DbPlayer> Players { get; set; }
    public DbSet<DbPieceEchecs> Pieces { get; set; }
    public DbSet<DbBall> Balls { get; set; }
    public DbSet<DbTerrain> Terrains { get; set; }
    public DbSet<DbCoupPingPong> Shots { get; set; }
    public DbSet<DbCollisionPrecise> Collisions { get; set; }
    public DbSet<DbDefenseStat> DefenseStats { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // À configurer via appsettings.json
            optionsBuilder.UseNpgsql("Host=localhost;Database=pingpong_chess;Username=postgres;Password=harena");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration DbMatch
        modelBuilder.Entity<DbMatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasDefaultValue("en_attente");
            entity.Property(e => e.KingNorthAlive).HasDefaultValue(true);
            entity.Property(e => e.KingSouthAlive).HasDefaultValue(true);
            entity.Property(e => e.PiecesNorthCount).HasDefaultValue(16);
            entity.Property(e => e.PiecesSouthCount).HasDefaultValue(16);
            entity.Property(e => e.PointsToWin).HasDefaultValue(11);
            
            entity.HasOne(e => e.PlayerNorth)
                  .WithMany(p => p.MatchesAsNorth)
                  .HasForeignKey(e => e.PlayerNorthId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.PlayerSouth)
                  .WithMany(p => p.MatchesAsSouth)
                  .HasForeignKey(e => e.PlayerSouthId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Terrain)
                  .WithMany(t => t.Matches)
                  .HasForeignKey(e => e.TerrainId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            // Configuration pour ServingPlayer et Winner (relations supplémentaires)
            entity.HasOne(e => e.ServingPlayer)
                  .WithMany()
                  .HasForeignKey(e => e.ServingPlayerId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Winner)
                  .WithMany()
                  .HasForeignKey(e => e.WinnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuration DbPieceEchecs
        modelBuilder.Entity<DbPieceEchecs>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MatchId, e.PlayerId, e.Column, e.Row })
                  .IsUnique();
        });
        
        // Configuration DbDefenseStat
        modelBuilder.Entity<DbDefenseStat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MatchId, e.PlayerId, e.DefendedColumn })
                  .IsUnique();
        });
    }
}