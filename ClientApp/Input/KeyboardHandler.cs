namespace ClientApp.Input;

public class KeyboardHandler
{
    private readonly GameManager _gameManager;
    private float _currentAngle = 45f; // 0-90 degrés
    private float _currentPower = 1.0f; // 0.5-3.0
    
    public event Action<float>? OnMove;
    public event Action<float, float>? OnHit;
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
                MovePaddle(-0.1f);
                break;
            case ConsoleKey.RightArrow:
                MovePaddle(0.1f);
                break;
                
            // Frappe avec ajustement angle/puissance
            case ConsoleKey.Spacebar:
                HitBall();
                break;
            case ConsoleKey.A: // Augmenter angle
                _currentAngle = Math.Clamp(_currentAngle + 5f, 0f, 90f);
                ShowHitPreview();
                break;
            case ConsoleKey.Z: // Diminuer angle
                _currentAngle = Math.Clamp(_currentAngle - 5f, 0f, 90f);
                ShowHitPreview();
                break;
            case ConsoleKey.E: // Augmenter puissance
                _currentPower = Math.Clamp(_currentPower + 0.2f, 0.5f, 3.0f);
                ShowHitPreview();
                break;
            case ConsoleKey.R: // Diminuer puissance
                _currentPower = Math.Clamp(_currentPower - 0.2f, 0.5f, 3.0f);
                ShowHitPreview();
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
        
        // Afficher la nouvelle position
        Console.SetCursorPosition(0, 22);
        Console.WriteLine($"Position: {(_gameManager.LocalPlayer?.PositionX ?? 0.5f):F2} " +
                         $"Col: {(_gameManager.LocalPlayer?.PositionX ?? 0.5f) * 7:F0}");
    }
    
    private void HitBall()
    {
        OnHit?.Invoke(_currentPower, _currentAngle);
        
        // Réinitialiser les paramètres
        _currentAngle = 45f;
        _currentPower = 1.0f;
    }
    
    private void ShowHitPreview()
    {
        Console.SetCursorPosition(0, 23);
        Console.WriteLine($"Préparation: Angle={_currentAngle}° Puissance={_currentPower:F1}   ");
        
        // Afficher la prédiction de trajectoire
        if (_gameManager.LocalPlayer != null)
        {
            // Simuler la trajectoire
            float simulatedVX = MathF.Cos(_currentAngle * MathF.PI / 180f) * _currentPower;
            float simulatedVY = MathF.Sin(_currentAngle * MathF.PI / 180f) * _currentPower;
            
            // Calculer la colonne cible
            int targetCol = simulatedVX > 0 ? 
                (int)(_gameManager.LocalPlayer.PositionX * 8) :
                (int)((1 - _gameManager.LocalPlayer.PositionX) * 8);
                
            Console.SetCursorPosition(0, 24);
            Console.WriteLine($"Cible prédite: Colonne {targetCol}               ");
        }
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