using Microsoft.EntityFrameworkCore;
using ServerApp.Database;
using ServerApp.Server;

// Configuration
var builder = WebApplication.CreateBuilder(args);

// Configurer la base de données
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
 
// Tester la connexion
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    try
    {
        await dbContext.Database.OpenConnectionAsync();
        Console.WriteLine("✅ Connexion à PostgreSQL réussie!");
        
        // Vérifier si les tables existent
        var tablesExist = await dbContext.Database.CanConnectAsync();
        if (tablesExist)
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
        Console.WriteLine("2. La base 'pingpong_chess' existe");
        Console.WriteLine("3. Les identifiants sont corrects dans appsettings.json");
        return;
    }
}

// Démarrer le serveur de jeu
var gameServer = new GameServer(7777);
gameServer.Start();

Console.WriteLine("🎮 Serveur de jeu démarré sur le port 7777");
Console.WriteLine("Appuyez sur 'Q' pour quitter...");

while (Console.ReadKey().Key != ConsoleKey.Q)
{
    await Task.Delay(100);
}

gameServer.Stop();
Console.WriteLine("👋 Serveur arrêté");