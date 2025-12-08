namespace ClientApp.Input;

/// <summary>
/// Gère les entrées clavier du joueur
/// </summary>
public class KeyboardHandler
{
    private Dictionary<ConsoleKey, Action> _keyBindings;
    
    public KeyboardHandler()
    {
        _keyBindings = new Dictionary<ConsoleKey, Action>();
        // TODO: Initialiser les bindings de touches
    }
    
    /// <summary>
    /// Écoute les entrées clavier
    /// </summary>
    public void ListenInput()
    {
        // TODO: Boucle d'écoute des touches
        // TODO: Appeler les actions correspondantes
    }
    
    /// <summary>
    /// Traite une pression de touche
    /// </summary>
    public void ProcessKeyPress(ConsoleKey key)
    {
        // TODO: Vérifier si la touche est bindée
        // TODO: Exécuter l'action correspondante
    }
    
    /// <summary>
    /// Enregistre un binding de touche
    /// </summary>
    public void RegisterKeyBinding(ConsoleKey key, Action action)
    {
        _keyBindings[key] = action;
    }
}

