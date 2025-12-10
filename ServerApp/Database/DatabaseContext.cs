using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Database;

public class DatabaseContext : DbContext
{
    public DbSet<DbMatch> Matches { get; set; }
    public DbSet<DbPlayer> Players { get; set; }
    public DbSet<DbPiece> Pieces { get; set; }
    public DbSet<DbBall> Balls { get; set; }
    public DbSet<DbTerrain> Terrains { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // À configurer via appsettings.json
            optionsBuilder.UseNpgsql("Host=localhost;Database=pingpong_chess;Username=postgres;Password=votre_mot_de_passe");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DbMatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasDefaultValue("waiting");
            entity.Property(e => e.KingNorthAlive).HasDefaultValue(true);
            entity.Property(e => e.KingSouthAlive).HasDefaultValue(true);
            
            entity.HasOne(e => e.PlayerNorth)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerNorthId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.PlayerSouth)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerSouthId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}