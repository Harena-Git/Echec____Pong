# Script pour lancer le jeu en local (2 joueurs sur le même PC)
# Utilisation : .\start-local-game.ps1

Write-Host "🎮 ÉCHEC-PONG - Démarrage en Mode Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier que nous sommes dans le bon répertoire
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

Write-Host "📁 Répertoire du projet : $scriptPath" -ForegroundColor Green
Write-Host ""

# Fonction pour lancer un processus dans une nouvelle fenêtre
function Start-GameProcess {
    param(
        [string]$Title,
        [string]$WorkingDirectory,
        [string]$Command
    )
    
    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = "powershell.exe"
    $startInfo.Arguments = "-NoExit -Command `"cd '$WorkingDirectory'; Write-Host '$Title' -ForegroundColor Yellow; $Command`""
    $startInfo.UseShellExecute = $true
    $startInfo.CreateNoWindow = $false
    
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null
    
    return $process
}

Write-Host "1️⃣  Démarrage du serveur..." -ForegroundColor Yellow
$serverPath = Join-Path $scriptPath "ServerApp"
$serverProcess = Start-GameProcess -Title "🖥️  SERVEUR - ÉCHEC-PONG" -WorkingDirectory $serverPath -Command "dotnet run"

Write-Host "   ✅ Serveur démarré (nouvelle fenêtre)" -ForegroundColor Green
Write-Host ""
Write-Host "⏳ Attendre 5 secondes que le serveur soit prêt..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "2️⃣  Démarrage du Joueur 1 (Alice - Nord)..." -ForegroundColor Yellow
$client1Path = Join-Path $scriptPath "ClientApp"
$client1Process = Start-GameProcess -Title "👤 JOUEUR 1 - ALICE (NORD)" -WorkingDirectory $client1Path -Command "dotnet run"

Write-Host "   ✅ Client 1 démarré (nouvelle fenêtre)" -ForegroundColor Green
Write-Host "   👉 Dans la fenêtre qui s'ouvre :" -ForegroundColor Cyan
Write-Host "      - Choisir l'option 2 (localhost)" -ForegroundColor Cyan
Write-Host "      - Entrer le nom : Alice" -ForegroundColor Cyan
Write-Host ""
Write-Host "⏳ Attendre 3 secondes..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

Write-Host ""
Write-Host "3️⃣  Démarrage du Joueur 2 (Bob - Sud)..." -ForegroundColor Yellow
$client2Path = Join-Path $scriptPath "ClientApp"
$client2Process = Start-GameProcess -Title "👤 JOUEUR 2 - BOB (SUD)" -WorkingDirectory $client2Path -Command "dotnet run"

Write-Host "   ✅ Client 2 démarré (nouvelle fenêtre)" -ForegroundColor Green
Write-Host "   👉 Dans la fenêtre qui s'ouvre :" -ForegroundColor Cyan
Write-Host "      - Choisir l'option 2 (localhost)" -ForegroundColor Cyan
Write-Host "      - Entrer le nom : Bob" -ForegroundColor Cyan
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ TOUS LES PROCESSUS SONT DÉMARRÉS !" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 3 fenêtres PowerShell ont été ouvertes :" -ForegroundColor Cyan
Write-Host "   1. Serveur (écoute sur port 7777)" -ForegroundColor White
Write-Host "   2. Client 1 - Alice (Joueur Nord)" -ForegroundColor White
Write-Host "   3. Client 2 - Bob (Joueur Sud)" -ForegroundColor White
Write-Host ""
Write-Host "🎮 COMMANDES DE JEU :" -ForegroundColor Yellow
Write-Host "   ←/→     : Déplacer la raquette" -ForegroundColor White
Write-Host "   ESPACE  : Frapper la balle" -ForegroundColor White
Write-Host "   A/Z     : Ajuster l'angle" -ForegroundColor White
Write-Host "   E/R     : Ajuster la puissance" -ForegroundColor White
Write-Host "   C       : Chat" -ForegroundColor White
Write-Host "   Q       : Quitter" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Pour arrêter tous les processus :" -ForegroundColor Red
Write-Host "   - Fermer cette fenêtre" -ForegroundColor White
Write-Host "   - OU appuyer sur 'Q' dans chaque fenêtre de client" -ForegroundColor White
Write-Host "   - OU appuyer sur 'Q' dans la fenêtre du serveur" -ForegroundColor White
Write-Host ""
Write-Host "📖 Voir NETWORK_SETUP.md pour plus d'informations" -ForegroundColor Cyan
Write-Host ""
Write-Host "Appuyez sur une touche pour quitter ce script..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host ""
Write-Host "✋ Note : Les 3 fenêtres de jeu restent ouvertes" -ForegroundColor Yellow
Write-Host "   Vous pouvez maintenant fermer cette fenêtre." -ForegroundColor Yellow
