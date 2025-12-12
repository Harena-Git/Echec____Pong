# Script de lancement rapide du jeu en mode local
Write-Host "🎮 Lancement du jeu Echec-Pong en MODE LOCAL..." -ForegroundColor Green
Write-Host ""

# Naviguer vers le dossier ClientApp
Set-Location -Path "$PSScriptRoot\ClientApp"

# Compiler et exécuter
Write-Host "📦 Compilation..." -ForegroundColor Yellow
dotnet build --nologo --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilation réussie!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Lancement de l'application..." -ForegroundColor Cyan
    Write-Host "👉 Cliquez sur 'MODE LOCAL (DÉMO)' pour jouer immédiatement!" -ForegroundColor Yellow
    Write-Host ""
    
    dotnet run --no-build
} else {
    Write-Host "❌ Erreur de compilation!" -ForegroundColor Red
    pause
}
