namespace PingPongChess.Network;

public static class NetworkProtocol
{
    // Port par défaut
    public const int DEFAULT_PORT = 7777;
    
    // Délais et timeouts
    public const int PING_INTERVAL_MS = 1000;
    public const int CONNECTION_TIMEOUT_MS = 10000;
    public const int GAME_UPDATE_INTERVAL_MS = 50; // 20 FPS
    
    // Constantes de jeu
    public const float BALL_SPEED_MAX = 10.0f;
    public const float BALL_GRAVITY = 9.81f;
    public const float PADDLE_SPEED = 5.0f;
    
    // Messages d'erreur
    public const string ERROR_GAME_FULL = "Game is full";
    public const string ERROR_GAME_STARTED = "Game already started";
    public const string ERROR_INVALID_NAME = "Invalid player name";
    public const string ERROR_CONNECTION_LOST = "Connection lost";
    
    // Commandes de contrôle
    public enum ControlCommand
    {
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        HitBall,
        Chat,
        Ready,
        Pause,
        Surrender
    }
}