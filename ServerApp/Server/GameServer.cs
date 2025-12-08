using System.Net;
using System.Net.Sockets;

namespace ServerApp.Server;

/// <summary>
/// Serveur TCP principal qui accepte les connexions clients
/// et orchestre le jeu
/// </summary>
public class GameServer
{
    private TcpListener? _tcpListener;
    private List<ClientHandler> _clients;
    // private GameEngine _gameEngine;
    
    public GameServer()
    {
        _clients = new List<ClientHandler>();
    }
    
    /// <summary>
    /// Démarre le serveur et commence à accepter les connexions
    /// </summary>
    public void Start(int port)
    {
        // TODO: Initialiser TcpListener
        // TODO: Démarrer l'écoute
        // TODO: Accepter les clients de manière asynchrone
    }
    
    /// <summary>
    /// Accepte les nouvelles connexions clients
    /// </summary>
    private async void AcceptClients()
    {
        // TODO: Boucle d'acceptation des clients
        // TODO: Créer ClientHandler pour chaque client
    }
    
    /// <summary>
    /// Envoie un message à tous les clients connectés
    /// </summary>
    public void BroadcastMessage(string message)
    {
        // TODO: Parcourir tous les clients et envoyer le message
    }
    
    /// <summary>
    /// Arrête le serveur
    /// </summary>
    public void Stop()
    {
        // TODO: Fermer toutes les connexions
        // TODO: Arrêter le TcpListener
    }
}

