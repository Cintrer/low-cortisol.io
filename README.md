# low-cortisol.io

Structure propre :
- Program.cs = boucle principale
- core = logique générale
- player = code du joueur
- maps = code des maps
- traps = code des pièges
- render = affichage
- assets/player = seulement pour les images du perso

Important :
- les maps sont codées
- les pièges sont codés
- on ne met pas d'images dans les maps
- les images servent seulement au perso si tu veux les brancher après

Contrôles :
- A / D ou flèches : bouger
- Espace / W / flèche haut : sauter
- R : recommencer le niveau

Lancer :
dotnet restore
dotnet run
