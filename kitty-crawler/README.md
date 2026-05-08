# KittyCrawler, a roguelite dungeon crawler
Dette er repo for spillet Kitty Crawler. Inneholder også frontend og backend arkitektur.

## Teknologier
- Godot 4 med C# (.NET)
- ASP.NET Core Web API
- Blazor
- PostgreSQL / Supabase
- Docker
- Azure

### Klon prosjektet
git clone https://github.com/SimonOygard/KittyCrawler.git

## Mappestruktur
- GodotGame/ – Selve spillet
- Backend/   – ASP.NET Core API
- Frontend/  – Blazor
- Database/  – SQL-migrasjoner


## Kjøring av spill:
Last ned ZIP fra https://kittycrawlerweb.azurewebsites.net/ og kjøre KittyCrawler.exe
(OBS, EXE er ikke signert så må kjøre tiltross for Microsoft defender pop-up)

## Kjøring via Godot:
https://downloads.godotengine.org/?version=4.6.2&flavor=stable&slug=mono_win64.zip&platform=windows.64 versjon

åpne prosjekt og trykk på play knappen øverst til høyre i editoren vedsiden av hammer-symbolet.

## Kjøring av frontend:
git bash:
cd blazor\KittyCrawler.Web\KittyCrawler.Web
dotnet run

## Kjøring av API:
git bash:
cd blazor\KittyCrawlerApi
dotnet run


## Lisens

Prosjektet er utviklet som studentprosjekt for utdanningsformål.

Assets og tredjepartsressurser tilhører sine respektive eiere.

  /\_/\
 ( o.o )  - meow
  > ^ <
 /|   |\
(_|   |_)
