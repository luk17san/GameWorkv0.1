# Linie celowania — 2026-10-11
Na obiekcie z ShipCombatController otwórz Fire Range Visualizer:
- Desktop Width Pixels: 1.25; Mobile Width Pixels: 2 (zakres 0.5–6).
- Glow Pixels: 2 (0–8).
- Indicator Camera: kamera rozgrywki; puste pole korzysta z MainCamera.
- Show Sectors: aktywny sektor złoty, pozostałe gotowe baterie bardzo subtelne podczas celowania. Tory tylko wybranej gotowej baterii.
- Surface Offset nadal ustala wysokość obrysu nad wodą; promień moździerza zachowuje rzeczywisty rozmiar świata.

Canvas overlay tworzony automatycznie, bez raycastów i CanvasScaler, pod HUD-em (sortingOrder=-10). Grubość rdzenia, 1 px miękkiej krawędzi i poświata są niezależne od zoomu. Gęstsze próbkowanie łuków (258 punktów).
Nakładka celowo rysuje się nad geometrią sceny: wyspy i kadłuby nie zasłaniają linii. Indicator Render Queue dotyczy obecnie wyłącznie ukrytego nośnika punktów LineRenderer.
Bez zmian inputu, balistyki, zasięgów, przeładowania, scen i prefabów. Dotykowe sterowanie i większy obszar przycisku nie należą do tego pakietu.

Weryfikacja: pełna kontrolna kompilacja źródeł Framework/ThePirate z lokalnymi bibliotekami Unity i URP PASS. Nie wykonano Unity import ani Play Mode.
Test ShipSandbox: najbliższy/najdalszy zoom, obie burty, PPM/LPM moździerza, reload, cel nieważny, przełączenie kamery, wyłączenie komponentu i zmiana sceny. Sprawdź 1920x1080, małe okno oraz telefon; oceń wygładzenie, glow, koszt Canvas i czytelność nad wyspami.
