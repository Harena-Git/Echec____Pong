using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ServerApp.Database;
using ServerApp.Server;

namespace ServerApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        // Configurer la base de données
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        // Tester la connexion
        using (var dbContext = new DatabaseContext(optionsBuilder.Options))
        {
            try
            {
                await dbContext.Database.OpenConnectionAsync();
                Console.WriteLine("✅ Connexion à PostgreSQL réussie!");
                
                // Vérifier si les tables existent
                var canConnect = await dbContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    Console.WriteLine("✅ Tables de la base de données existantes");
                    
                    // Compter les joueurs
                    var playerCount = await dbContext.Players.CountAsync();
                    Console.WriteLine($"📊 Joueurs dans la base: {playerCount}");
                }
                else
                {
                    Console.WriteLine("⚠️  Tables non trouvées, création...");
                    await dbContext.Database.EnsureCreatedAsync();
                    Console.WriteLine("✅ Tables créées avec succès!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur de connexion: {ex.Message}");
                Console.WriteLine("Vérifiez que:");
                Console.WriteLine("1. PostgreSQL est démarré");
                Console.WriteLine("2. La base de données existe");
                Console.WriteLine("3. Les identifiants sont corrects dans appsettings.json");
                return;
            }
        }
        
        // Démarrer le serveur de jeu
        int port = configuration.GetValue<int>("Server:Port", 7777);
        var gameServer = new GameServer(port);
        gameServer.Start();
        
        Console.WriteLine($"🎮 Serveur de jeu démarré sur le port {port}");
        Console.WriteLine("Appuyez sur 'Q' pour quitter...");
        
        while (Console.ReadKey(true).Key != ConsoleKey.Q)
        {
            await Task.Delay(100);
        }
        
        gameServer.Stop();
        Console.WriteLine("👋 Serveur arrêté");
    }
}