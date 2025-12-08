using System.Net.Sockets;
using System.Text;

namespace ServerApp.Server;

/// <summary>
/// Gère la communication avec un client spécifique
/// </summary>
public class ClientHandler
{
    private TcpClient _tcpClient;
    private NetworkStream? _stream;
    // private Player _player;
    
    public ClientHandler(TcpClient client)
    {
        _tcpClient = client;
        _stream = client.GetStream();
    }
    
    /// <summary>
    /// Gère la communication avec le client
    /// </summary>
    public async void HandleClient()
    {
        // TODO: Boucle de réception des messages
        // TODO: Traiter les messages reçus
        // TODO: Envoyer les réponses
    }
    
    /// <summary>
    /// Envoie un message au client
    /// </summary>
    public void SendMessage(string message)
    {
        // TODO: Sérialiser et envoyer le message
    }
    
    /// <summary>
    /// Reçoit un message du client
    /// </summary>
    public async Task<string?> ReceiveMessage()
    {
        // TODO: Lire les données du stream
        // TODO: Désérialiser le message
        return null;
    }
    
    /// <summary>
    /// Ferme la connexion avec le client
    /// </summary>
    public void Disconnect()
    {
        // TODO: Fermer le stream et la connexion
    }
}

