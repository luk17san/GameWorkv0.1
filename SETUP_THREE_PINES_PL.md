# Wybrzeże Trzech Sosen — pierwszy etap

Otwórz `Assets/ThePirate/Scenes/WybrzezeTrzechSosen.unity`.

Scena odwzorowuje interpretowane linie brzegowe mapy `art/Mapa na jasnym pergaminie.png`: dwie duże wyspy na północy, ląd na południu i wschodzie, zatoki, cieśninę oraz mniejsze wyspy i skały. Obszar ma 3000 × 2000 metrów. Północ to +Z, wschód +X, morze Y=0. Środek obszaru jest w (0,0,0). Ramka, napisy, róża wiatrów i ozdobne symbole nie stają się obiektami świata.

Mapa jest ilustracją perspektywiczną. Kontury zinterpretowano ręcznie; wysokości, szerokości plaż i podwodne zbocza są przybliżeniem. Ląd urwany ramką kończy się na granicy obszaru. To nie jest jeszcze granica blokująca gracza.

## Hierarchy i Inspector

- `Lad_i_wyspy` → Terrain → Paint Terrain: standardowe pędzle Unity do podnoszenia, obniżania i wygładzania wysokości oraz malowania warstw. Dane wysokości znajdują się w `Assets/ThePirate/World/ThreePines/Generated/ThreePines_Terrain.asset`. Heightmap 1025², teren Y=-35, rozmiar (3000,220,2000). Nie zmieniaj skali Transform; rozmiar ustawia TerrainData. TerrainCollider używa tych samych danych.
- `Lad_i_wyspy` → Terrain Layers: piasek, trawa i skała z istniejącego ArchipelagoKit. Te warstwy są współdzielone z projektem; skopiuj warstwę przed modyfikowaniem jej tekstury lub parametrów tylko dla tej mapy.
- `Ocean_Y0` → Mesh Renderer → Material: osobny `Assets/ThePirate/World/ThreePines/Ocean.mat`. Shader obsługuje poprawnie głębokość dla kamer perspektywicznych i ortograficznych. Ocean nie ma collidera; dno ma collider Terrain.
- `Main Camera` → Camera: kamera śledząca statek gracza. Po dodaniu gracza kamera działa w Perspective i ma TopDownCamera z przypisanym PlayerShip; offset (0,50,-35), maksymalna odległość 120 m. Kółko myszy zmienia przybliżenie.
- `Punkt_startowy_statku`: znacznik startu gracza obok portu, około (-667.26,0.5,399).

## Zmiana konturów

`Assets/ThePirate/World/ThreePines/Coastlines.json` przechowuje 40 zamkniętych wielokątów. Współrzędne x/y są pikselami obrazu referencyjnego 1536 × 1024; y rośnie w dół. Obszar ramki: x=40..1495, y=40..970. Pola width/depth ustawiają docelowy rozmiar w metrach.

Po zmianie JSON uruchom `GameWork → Map → Wybrzeze Trzech Sosen → Generate new scene`. Generator tworzy nową scenę i nowe TerrainData z unikatową nazwą, zachowując wcześniejsze ręczne rzeźbienie. Nie aktualizuje już wygenerowanego terenu na miejscu. Nie uruchamiaj generowania w Play Mode. Jeśli chcesz zachować rzeźbienie, pracuj pędzlami na istniejącym Terrain zamiast ponownie generować.

## Weryfikacja

Unity 6000.6.3f1 wygenerowało scenę i pliki .meta. Kontrolne raycasty TerrainCollider trafiły ponad wodą w trzech głównych obszarach lądu (3/3), w edytorze. Kompilacja i podgląd kamery sprawdzone osobno. Dokumentacyjny zrzut: `Documentation/ThreePines/Overview.png`.

Play Mode: sprawdzono podejście, zacumowanie, dostępność PortRepairService, odcumowanie, przywrócenie uzbrojenia oraz przypisanie HUD. Użyto bezpośrednich metod dokowania; fizyczne E, pełne pływanie, pociski i telefon pozostają do sprawdzenia. Nie zmieniono menu ani Build Settings.

## Statek i port — 2026-10-10
- PlayerShip: kopia aktualnego obiektu gracza z ShipSandbox, z jego modelem słowiańskim, uzbrojeniem i ruchem; pozycja około (-667.26,0.5,399), kierunek +X. To edytowalna kopia sceny, nie nowy wspólny prefab.
- Main Camera → TopDownCamera → Target: PlayerShip. PlayerShip → ShipCombatController → Aim Camera: Main Camera, Water Height: 0.
- PlayerHUD → PlayerHudView → Source: PlayerShip/PlayerHudController. HUD ma jawne przypisanie, ponieważ bootstrap projektu obsługuje wyłącznie ShipSandbox.
- Port_Trzech_Sosen: instancja PortDock.prefab w zatoce północno-zachodniej wyspy, pozycja około (-675.26,0,399), obrót Y=90. PortDock → Port Id: three-pines-northwest-port; Display Name: Port Trzech Sosen; Berth: dziecko Berth.
- Port_Trzech_Sosen/Land pełni funkcję wąskiego drewnianego dojścia do plaży, Pier jest pomostem poprzecznym, MooringPost to słupki. Materiały Wood_Warm/Shadow pochodzą z ArchipelagoKit. Pola nazw prefabowych zachowano jako nadpisania instancji sceny.
- Dokowanie: zwolnij do maksymalnie 1 m/s, znajdź się do 12 m od Berth, naciśnij E. PortMenu i usługi portowe są tworzone przez istniejący CombatShip. E odcumowuje; stan walki blokuje dokowanie.
- Otwórz scenę i uruchom Play: sprawdź W/S/A/D, przybliżenie kółkiem, E przy pomoście, menu usług i ponowne E. Scena startuje ze statkiem swobodnie pływającym obok stanowiska.
- Podgląd kamery: Documentation/ThreePines/PlayerPort.png; zrzut renderu kamery nie zawiera nakładki Canvas HUD.
