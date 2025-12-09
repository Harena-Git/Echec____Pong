using Microsoft.EntityFrameworkCore;
using GameSolution.Models;

namespace GameSolution.Database;

public class DatabaseContext : DbContext
{
    public DbSet<Terrain> Terrains { get; set; }
    public DbSet<Joueur> Joueurs { get; set; }
    public DbSet<PieceEchecs> PieceEchecs { get; set; }
    public DbSet<Balle> Balles { get; set; }
    public DbSet<CoupPingPong> CoupsPingPong { get; set; }
    public DbSet<CollisionBallePiece> CollisionsBallePiece { get; set; }
    public DbSet<MatchHybride> MatchsHybrides { get; set; }
    public DbSet<StatistiquesJoueurMatch> StatistiquesJoueurMatch { get; set; }
    public DbSet<DeplacementPiece> DeplacementsPiece { get; set; }
    public DbSet<ConfigurationPieces> ConfigurationsPieces { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration des tables
        modelBuilder.Entity<MatchHybride>()
            .HasMany(m => m.Pieces)
            .WithOne(p => p.Match)
            .HasForeignKey(p => p.IdMatch)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<MatchHybride>()
            .HasMany(m => m.Balles)
            .WithOne(b => b.Match)
            .HasForeignKey(b => b.IdMatch)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<MatchHybride>()
            .HasMany(m => m.Coups)
            .WithOne(c => c.Match)
            .HasForeignKey(c => c.IdMatch)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index pour les performances
        modelBuilder.Entity<PieceEchecs>()
            .HasIndex(p => new { p.IdMatch, p.IdJoueur });
            
        modelBuilder.Entity<Balle>()
            .HasIndex(b => b.IdMatch);
            
        modelBuilder.Entity<CoupPingPong>()
            .HasIndex(c => new { c.IdMatch, c.IdJoueur });
            
        modelBuilder.Entity<MatchHybride>()
            .HasIndex(m => m.Statut);
            
        // Valeurs par défaut
        modelBuilder.Entity<Joueur>()
            .Property(j => j.Statut)
            .HasDefaultValue(StatutJoueur.Actif);
            
        modelBuilder.Entity<PieceEchecs>()
            .Property(p => p.Statut)
            .HasDefaultValue(StatutPiece.Vivant);
            
        modelBuilder.Entity<MatchHybride>()
            .Property(m => m.Statut)
            .HasDefaultValue(StatutMatch.EnAttente);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // À configurer dans appsettings.json
            optionsBuilder.UseNpgsql("Host=localhost;Database=pingpongechecs;Username=postgres;Password=password");
        }
    }
}