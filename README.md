# Base C# raylib-cs - style plateforme / Level Devil

## Installation
```bash
dotnet restore
dotnet run
```

## Dossiers importants
- `assets/player/idle` : images du perso à l'arrêt
- `assets/player/run` : images du perso qui court
- `assets/player/jump` : images du saut
- `assets/maps` : décors / tiles / fond
- `assets/ui` : boutons / icônes

## Règles pour les sprites
- Mets tes images en `.png`
- Nomme-les dans l'ordre, par exemple :
  - `run_01.png`
  - `run_02.png`
  - `run_03.png`
- Toutes les images d'une même animation doivent avoir à peu près la même taille

## Contrôles
- A / D ou flèches : bouger
- SPACE : sauter
- SHIFT : courir
