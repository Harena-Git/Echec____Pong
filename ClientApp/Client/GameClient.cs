using System.Net.Sockets;
using System.Text;

namespace ClientApp.Client;

/// <summary>
/// Client TCP qui se connecte au serveur
/// </summary>
public class GameClient
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    // private GameState _gameState;
    
    /// <summary>
    /// Se connecte au serveur
    /// </summary>
    public bool Connect(string serverIp, int port)
    {
        // TODO: Créer TcpClient et se connecter
        // TODO: Obtenir le NetworkStream
        return false;
    }
    
    /// <summary>
    /// Envoie une entrée clavier au serveur
    /// </summary>
    public void SendInput(string input)
    {
        // TODO: Sérialiser et envoyer l'input
    }
    
    /// <summary>
    /// Reçoit une mise à jour du jeu depuis le serveur
    /// </summary>
    public async Task<string?> ReceiveUpdate()
    {
        // TODO: Lire les données du stream
        // TODO: Désérialiser le message
        return null;
    }
    
    /// <summary>
    /// Démarre l'écoute des mises à jour du serveur
    /// </summary>
    public async void StartListening()
    {
        // TODO: Boucle de réception des mises à jour
        // TODO: Mettre à jour l'état local du jeu
    }
    
    /// <summary>
    /// Déconnecte le client
    /// </summary>
    public void Disconnect()
    {
        // TODO: Fermer le stream et la connexion
    }
}

