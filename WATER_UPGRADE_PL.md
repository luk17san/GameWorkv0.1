# Aktualizacja oceanu 0.2 — Unity 6 URP

## Co było przyczyną różnicy

Poprzedni materiał rysował niemal wyłącznie sinusoidalne pasy koloru.
Nie miał map szczegółów fal i piany ani refleksów zależnych od światła.
Dodatkowo współrzędne ekranowe głębokości powstawały w vertex shaderze w niewłaściwej przestrzeni,
a odczyt głębokości nie uwzględniał poprawnie kamery ortograficznej.
Obraz pokazany w rozmowie był generowaną koncepcją, nie renderem tej paczki.

## Podmiana w istniejącym projekcie

1. Rozpakuj archiwum. Otwórz znajdujący się w nim folder
   `Assets/GameWork/ArchipelagoKit`.
2. Skopiuj **zawartość tego folderu** do swojej istniejącej lokalizacji pakietu.
   W Twoim projekcie jest to `Assets/Models/ArchipelagoKit`.
   Nadpisz pliki o tych samych nazwach i dodaj nowe. Zachowaj istniejące pliki `.meta`.
   Nie umieszczaj drugiej kopii tych samych skryptów w `Assets/GameWork`.
3. Poczekaj, aż Unity zakończy import i kompilację.
4. Zaznacz istniejący `Ocean_Stylized` w Hierarchy (komponent `StylizedOceanSurface`).
5. Wybierz **GameWork → Archipelago → Water → Apply Ocean Look To Selected**.
6. Zapisz scenę. Sprawdź efekt w Game View, najlepiej po wejściu w Play Mode.

Polecenie tworzy osobny materiał, przypisuje trzy dostarczone tekstury danych,
ustawia ich import na liniowy z mipmapami i powtarzaniem, włącza Depth Texture oraz
Opaque Texture w aktywnym URP Asset i odpowiednie opcje głównej kamery, jeśli taka istnieje.
Nie przestawia kamery i nie tworzy nowej powierzchni wody.
Przy niestandardowej kamerze bez tagu MainCamera sprawdź samodzielnie jej opcje
Depth Texture i Opaque Texture — powinny korzystać z Pipeline Settings lub być włączone.

Dla nowego oceanu przycisk **Create Ocean** również używa nowego materiału.
Dla telefonów jest **Apply Mobile Look To Selected**: pomija drugą warstwę normalnych,
drugą próbkę piany i refrakcję. Nie wyłącza globalnego Opaque Texture używanego przez inne efekty.

## Co zawiera nowy shader

- Dwie przesuwane warstwy szczegółów fal; oświetlenie i połysk zależą od normalnych.
- Trzy zapętlające się tekstury 512×512: WaveData, FoamPattern i MacroVariation.
- Nierównomierną pianę na grzbietach fal, zniekształcone pasma fal i wolne zmiany koloru.
- Rekonstrukcję głębokości obsługującą kamerę perspektywiczną i ortograficzną.
- Turkusowe płycizny, pianę przy przecięciach i uproszczony wzór światła pod wodą.
- Widoczność nieprzezroczystego dna w płytkiej wodzie, z niewielką refrakcją (wariant PC).
- Wodę na pustym oceanie bez wymogu tworzenia ogromnego dna.

To pozioma powierzchnia XZ. Fale zmieniają normalne i wygląd powierzchni, nie fizyczną wysokość wody.
Shader nie zapewnia symulacji wyporności. Od wersji 0.3 osobny komponent ShipWakeEmitter tworzy kilwater — patrz WAVES_AND_WAKE_PL.md.
Przezroczyste obiekty pod wodą nie są zawarte w Opaque Texture.
Kolor odbicia nieba jest artystycznym przybliżeniem, a nie odbiciem sceny w czasie rzeczywistym.

## Ustawienia startowe dla widoku pionowo z góry

Przy skali 1 jednostka = 1 metr, statku około 10 m i poziomie morza Y=0:

| Element | Wartość początkowa |
|---|---|
| Kamera | Orthographic, Rotation X=90, Y=0, Z=0 |
| Wysokość kamery | Y=160 |
| Orthographic Size | 90 |
| Far Clip Plane | 2000 |
| Directional Light | Rotation X=50, Y=-30, Z=0; Intensity=1 |
| Wave Texture Tile | 96 |
| Foam Texture Tile | 80 |
| Swell Band Spacing | 32 |
| Wave Normal Strength | 0.55 |
| Whitecap Coverage | 0.42 |
| Whitecap Brightness | 0.85 |
| Sun Glints | 0.75 |
| Shallow Colour Depth | 14 |
| Shore Foam Depth | 1.2 |

To punkt startowy, nie gwarancja identycznego kadru. Materiał dostraja się do skali świata,
oświetlenia, ustawień koloru i odległości kamery. Podane ustawienia kamery nie nadpisują jej kontrolera.

## Szybkie dostrojenie

- Zbyt gęste wzorki: zwiększ Wave Texture Tile i Foam Texture Tile.
- Zbyt szerokie pasma: zmniejsz Swell Band Spacing.
- Za dużo piany: zmniejsz Whitecap Coverage / Whitecap Brightness.
- Zbyt mocne błyski: zmniejsz Sun Glints; zwiększ Glint Sharpness, by zwęzić refleksy.
- Chcesz więcej granatu: przyciemnij Deep Ocean albo zmniejsz jasność Wave Turquoise.
- Brak refleksów: sprawdź Directional Light i włączone Scene Lighting w Scene View.
- Dziwne nakładające się wzory: zostaw tylko jedną aktywną powierzchnię oceanu w tym miejscu
  (sprawdź stare obiekty Water_Test / RealisticOcean).
- Brak animacji w Scene View: sprawdź efekt w Play Mode; edytor może nie odświeżać czasu shaderów.
- Brak płycizn: dno musi mieć renderer nieprzezroczysty i znajdować się pod powierzchnią wody.
- Jednolita woda po podmianie pliku: uruchom Apply Ocean Look To Selected, żeby przypisać tekstury.

WaveData to spakowane dane nachylenia powierzchni, nie standardowa mapa Normal Map Unity.
Narzędzie ustawia odpowiedni import automatycznie. Nie zmieniaj jej Texture Type na Normal Map.

## Weryfikacja i granice

Sprawdzono wymiary i ciągłość powtarzania tekstur, spójność nazw shader/materiał i kompletność archiwum.
Numeryczny podgląd wzoru służył kontroli danych, nie jest renderem Unity.
Nie wykonano kompilacji C#/HLSL ani testu wizualnego/wydajnościowego w Unity — edytor nie jest dostępny
w środowisku przygotowującym paczkę. Porównanie z koncepcją trzeba wykonać na rzeczywistym kadrze Game View.

Zmiana w narzędziu terenu: poprawiono normalizację wag piasku, trawy i skał w nowo generowanych wyspach.
Istniejący teren nie jest automatycznie przemalowywany.

Dokumentacja użyta przy korekcie odczytu głębokości:
https://docs.unity3d.com/6000.0/Documentation/Manual/urp/writing-shaders-urp-reconstruct-world-position.html
