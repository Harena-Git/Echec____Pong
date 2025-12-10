using ClientApp.Game;

namespace ClientApp.Input;

public class KeyboardHandler
{
    private readonly GameManager _gameManager;
    
    public event Action<float>? OnMove;
    public event Action<string>? OnChat;
    public event Action? OnQuit;
    
    public KeyboardHandler(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
    
    public void StartListening()
    {
        Task.Run(() =>
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    HandleKey(key.Key);
                }
                
                Thread.Sleep(50);
            }
        });
    }
    
    private void HandleKey(ConsoleKey key)
    {
        switch (key)
        {
            // Déplacement horizontal
            case ConsoleKey.LeftArrow:
                MovePaddle(-0.05f);
                break;
            case ConsoleKey.RightArrow:
                MovePaddle(0.05f);
                break;
                
            // Chat
            case ConsoleKey.C:
                StartChat();
                break;
                
            // Quitter
            case ConsoleKey.Q:
                OnQuit?.Invoke();
                break;
        }
    }
    
    private void MovePaddle(float delta)
    {
        OnMove?.Invoke(delta);
    }
    
    private void StartChat()
    {
        Console.SetCursorPosition(0, 25);
        Console.Write("Message: ");
        string message = Console.ReadLine() ?? string.Empty;
        
        if (!string.IsNullOrWhiteSpace(message))
        {
            OnChat?.Invoke(message);
        }
    }
}