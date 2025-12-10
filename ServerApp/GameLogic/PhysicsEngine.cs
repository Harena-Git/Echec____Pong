namespace ServerApp.GameLogic;

public class PhysicsEngine
{
    public const float TABLE_WIDTH = 1.0f;
    public const float TABLE_HEIGHT = 0.5f;
    public const float GRAVITY = 9.81f;
    public const float BALL_RADIUS = 0.05f;
    
    public (float newX, float newY, float newVX, float newVY) UpdateBallPosition(
        float x, float y, float vx, float vy, float deltaTime)
    {
        // Nouvelle position
        float newX = x + vx * deltaTime;
        float newY = y + vy * deltaTime;
        
        // Appliquer la gravité
        float newVY = vy - GRAVITY * deltaTime;
        
        // Rebond sur le sol (y = 0)
        if (newY <= 0)
        {
            newY = 0;
            newVY = Math.Abs(newVY) * 0.8f; // Coefficient de restitution
        }
        
        // Limites horizontales
        if (newX < 0) newX = 0;
        if (newX > TABLE_WIDTH) newX = TABLE_WIDTH;
        
        return (newX, newY, vx, newVY);
    }
    
    public int CalculateImpactColumn(float ballX, float ballVX)
    {
        // Prédire où la balle va sortir
        // Si elle va vers la droite (vx > 0) → sort par la droite
        // Si elle va vers la gauche (vx < 0) → sort par la gauche
        
        if (ballVX > 0)
        {
            // Sort par la droite, calculer la colonne basée sur la position
            return (int)(ballX * 8);
        }
        else
        {
            // Sort par la gauche
            return (int)((1 - ballX) * 8);
        }
    }
    
    public float CalculateHitPower(float swingDuration, float swingDistance)
    {
        // Puissance = distance * durée
        float power = swingDistance / Math.Max(0.1f, swingDuration);
        return Math.Clamp(power, 0.5f, 3.0f);
    }
}