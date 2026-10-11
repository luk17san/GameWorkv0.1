# Mapa pergaminowa

Otwórz projekt Unity i poczekaj na import. Oczekujące żądanie instaluje prefab i wiąże go ze statkiem w WybrzezeTrzechSosen. Jeśli scena ma niezapisane zmiany, zapisz ją i wybierz GameWork > Map > Wybrzeze Trzech Sosen > Install parchment map. Wynik jest w Temp/ParchmentMap.result. PASS Editor nie oznacza testu Play Mode.

Sterowanie: M otwiera/zamyka mapę, Esc zamyka. Świat nadal działa, sterowanie i broń gracza są blokowane. Czerwony trójkąt wskazuje kierunek dziobu. Poza terenem znacznik znika i pojawia się komunikat. Istniejąca minimapa pozostaje w HUD.

Inspector: obiekt ParchmentMap, komponent ParchmentMapView:
- Player: PlayerShip (po wymianie statku odnajdywany w tej samej scenie).
- World Minimum: (-1500, -1000), World Size: (3000, 2000), osie X/Z.
- Image World Rect: X=40, Y=54, Width=1455, Height=930; piksele od lewego dolnego rogu, zgodne z generatorem terenu.
- Image Size: (1536, 1024). Zmiana kadrowania obrazu wymaga ponownej kalibracji.
- Panel: MapPanel; Parchment: MapPanel/MapViewport/Parchment; Marker: Parchment/ShipMarker; Status: MapPanel/Status.

Prefab: Assets/ThePirate/Resources/GameWork/ParchmentMap.prefab. Kolor i rozmiar znacznika edytujesz w ShipMarker i Heading. Tekstura: Assets/ThePirate/World/ThreePines/ParchmentMap.png. Zachowano oryginalny obraz w art.

Test w Unity: uruchom WybrzezeTrzechSosen, naciśnij M i sprawdź zatokę NW; zamknij mapę, wypłyń i skręć, ponownie otwórz. Znacznik powinien zmienić położenie i kierunek. Sprawdź M/Esc, blokadę strzału i sterowania, różne proporcje Game View oraz wymianę statku. Pozycja początkowa (-667.26, 399 w X/Z) odpowiada około (444, 319) pikseli od lewego górnego rogu ilustracji.

Mapa pokazuje ilustrację terenu, nie odkrywa świata i nie ma funkcji wyznaczania kursu. Kontury terenu są interpretacją obrazu, więc detale rysunku nie stanowią dokładnej mapy każdej skały.

## Wynik wykonanej weryfikacji
Kompilacja kontrolna: 0 błędów. Unity wygenerowało prefab i meta oraz zapisało powiązanie z graczem w scenie. Play Mode: przesunięcie X/Z, kurs 90°, ukrycie poza obszarem, blokada wejścia i Close zaliczone programowo. Fizyczne M/Esc oraz wygląd UI wymagają sprawdzenia. Narzędzie screenshot pomija overlay, kolejne przechwycenie blokował błąd ROSLYN001; podgląd kamery nie potwierdza pergaminu.
