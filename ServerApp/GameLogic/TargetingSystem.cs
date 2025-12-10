using ServerApp.Network;

namespace ServerApp.GameLogic;

public class TargetingSystem
{
    public class TargetingResult
    {
        public int TargetColumn { get; set; }
        public string TargetSide { get; set; } = string.Empty;
        public string TargetPieceType { get; set; } = string.Empty;
        public bool CanDefend { get; set; }
        public int DefenderColumn { get; set; }
        public float HitAccuracy { get; set; }
    }
    
    public TargetingResult CalculateTarget(
        float ballX, float ballVX, 
        PlayerState attacker, PlayerState defender,
        List<PieceState> targetPieces)
    {
        var result = new TargetingResult();
        
        // 1. Calculer la colonne cible
        if (ballVX > 0) // Vers la droite
        {
            result.TargetColumn = (int)(ballX * 8);
            result.TargetSide = defender.Side;
        }
        else // Vers la gauche
        {
            result.TargetColumn = (int)((1 - ballX) * 8);
            result.TargetSide = defender.Side;
        }
        
        result.TargetColumn = Math.Clamp(result.TargetColumn, 0, 7);
        
        // 2. Vérifier la défense
        result.DefenderColumn = (int)(defender.PositionX * 8);
        result.CanDefend = result.DefenderColumn == result.TargetColumn;
        
        // 3. Trouver la pièce ciblée
        var targetPiece = targetPieces.FirstOrDefault(p => 
            p.Column == result.TargetColumn && p.Row == 0 && p.IsAlive);
            
        result.TargetPieceType = targetPiece?.Type ?? "none";
        
        // 4. Calculer la précision du tir
        result.HitAccuracy = CalculateHitAccuracy(ballX, result.TargetColumn);
        
        return result;
    }
    
    private float CalculateHitAccuracy(float ballX, int targetColumn)
    {
        float targetCenter = (targetColumn + 0.5f) / 8f;
        float distance = Math.Abs(ballX - targetCenter);
        
        // Plus la distance est petite, plus la précision est élevée
        return Math.Max(0, 1 - distance * 2);
    }
}