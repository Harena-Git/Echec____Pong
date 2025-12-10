# Compiler le client (pour les joueurs)
cd ClientApp
dotnet publish -c Release -o ../Distrib/Client --self-contained true

# Compiler le serveur (pour l'hôte)
cd ../ServerApp  
dotnet publish -c Release -o ../Distrib/Server --self-contained true