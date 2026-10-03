# Edytowalny HUD Unity

## Gdzie są elementy

- Cały HUD: `Assets/ThePirate/Resources/GameWork/PlayerHud.prefab`.
- Osobne prefaby sekcji: `Assets/ThePirate/UI/HUD/Prefabs/`.
- Osobne grafiki PNG z alfa: `Assets/ThePirate/UI/HUD/Art/ShipAvatar.png` i `SpeedWheelFrame.png`.
- Kontrolki uGUI i połączenie ze stanem: `Assets/ThePirate/UI/HUD/Runtime/`.
- Narzędzie budowy i weryfikacji: `Assets/ThePirate/Editor/HUD/`.
- Obraz podglądu wygenerowany z prefabu: `HUD-Preview/PlayerHUD.png` (wartości demonstracyjne).

Prefaby tworzy Unity przez `GameWork > HUD > Utwórz edytowalny HUD`.
Ponowne użycie tego polecenia nie nadpisuje istniejącego HUD — sprawdza go i odświeża podgląd.
Skrypty i PNG nie mają ręcznie tworzonych GUID; pliki .meta i prefaby generuje edytor.

## Hierarchia

```text
PlayerHUD  [Canvas, CanvasScaler, CanvasGroup, PlayerHudView]
  SafeArea
    Durability
      Background / Frame / SlavicCorner_0..3
      Avatar / ShipPortrait
      Title / DurabilityBar (Track, Fill, Outline) / ValueBelowBar
    SpeedWheel
      DialBackground / WheelFrame / SpeedTitle / SpeedValue / DriveBelowSpeed
    Cargo
      Background / Frame / CrateOutline / CrateLid / CrateCenter
      CargoTitle / CargoValue / CargoBar
    ReloadShip
      HullBackground / HullOutline / Keel / DeckLine_0..5
      Bow / Left / Right / Stern
        Background / Rim / ReloadProgress / Selected / Countdown / ReadyCheck
    Minimap
      Background / CircularMask (Terrain, grid, Markers, MarkerTemplate)
      GoldRim / North / East / South / West
    Wind
      Background / Frame / Title / Value / Direction
    Objective
      Background / Frame / CompassIcon / Title / Description
    Target
      Background / Frame / Name / TargetBar / HealthValue / Distance
    Message
      Background / Frame / Text
```

Dziewięć sekcji w SafeArea to zagnieżdżone instancje osobnych prefabów. Można otworzyć
np. `ReloadShip.prefab` bez reszty HUD. Każdy napis, pasek, obrys, znacznik i ozdobnik
jest osobnym GameObject. Awatar i obręcz steru są rastrowymi sprite'ami; ich wewnętrznych
pikseli nie edytuje się jako obiektów Unity. Żaden tekst nie jest wypalony w obraz.

## Jak edytować

1. Otwórz PlayerHud.prefab w Prefab Mode. Rozwiń SafeArea.
2. Przesuwaj/skaluj całą sekcję przez jej RectTransform. Kotwice przywiązują ją do rogu ekranu.
3. Teksty: komponent Text — Font, Font Size, Color, Alignment i rozmiar RectTransform.
4. Grafiki: komponent Image — Source Image i Color. Awatar można podmienić na dowolny Sprite.
5. Ramki/ornamenty: HudShapeGraphic — Color, Thickness, Points (współrzędne 0..1), Closed.
6. Wypełnienia pasków i pierścieni: pole Fill (0..1). Podczas gry kontroler aktualizuje je z danych.
7. Przeładowanie: każdy punkt ma HudBatteryWidget z Side oraz Mount Index. Domyślnie po jednym
   na kierunek. Aby dodać punkt: zduplikuj kontrolkę, ustaw indeks, przesuń i dopisz ją do
   tablicy Batteries komponentu PlayerHudView. Nie ma ikon ani liczników dział.
8. Nazwa napędu jest konfiguracją w PlayerHudView > Propulsion Name, domyślnie ŻAGLE.
   Aktualny model ruchu nie ma systemu zmiany rodzaju napędu. Prędkość to jednostki Unity/s.
9. Minimap View: World Radius, Camera Height, Terrain Layers, Texture Size oraz kolory znaczników.
   Północ to +Z świata. Kamera minimapy nie obraca się razem ze statkiem. Strzałka wiatru
   wskazuje kierunek wektora prędkości wiatru (dokąd wieje), a nie kierunek meteorologiczny „skąd”.

Domyślna rozdzielczość odniesienia to 1672×941. CanvasScaler skaluje całość, HudSafeArea
pilnuje bezpiecznego obszaru. UI nie przechwytuje kliknięć (raycastTarget=false,
CanvasGroup.blocksRaycasts=false); menu pauzy ma wyższy sorting order.

## Podłączenie i dane

W ShipSandbox PlayerHudBootstrap dodaje jedną instancję HUD i przypisuje kontroler gracza.
Jeżeli umieścisz prefab HUD w scenie ręcznie, bootstrap wykorzysta tę instancję.
W innych scenach przypisz PlayerHudController do Source na PlayerHudView.
Nie dodawaj kilku HUD na jednym ekranie. Canvas HUD nie jest utrzymywany przez DontDestroyOnLoad.

Wytrzymałość, ruch, ładownia, baterie i znaczniki korzystają z obecnego PlayerHudStateStore.
Minimapa renderuje scenę z góry do małej tekstury 256×256; nie ma mgły wojny ani wykrywania radarem.
Terrain Layers pozwala wykluczyć zbędne obiekty z renderowania. Warstwa UI zawsze jest wykluczona.
Kamera/RenderTexture powstają tylko podczas gry i są usuwane po wyłączeniu minimapy.

Wiatr, cel i zadanie czekają na istniejące API magazynu (patrz SETUP_HUD.md).
Nieobecne dane pokazują „—” lub „Brak aktywnego zadania”; panel celu chowa się.
Wartości 420/500, 8,4, 320/1000 i timery są wyłącznie przykładem w Prefab Mode/podglądzie,
a po podłączeniu w grze zastępują je rzeczywiste dane. Komunikaty korzystają z MessageRaised.

## Sprawdzenie w Unity

- Poczekaj na import i sprawdź Console.
- Otwórz prefab: napisy, grafiki i punkty powinny dać się zaznaczać oddzielnie.
- Uruchom ShipSandbox: wytrzymałość i prędkość powinny odzwierciedlać statek, przeładowanie
  powinno odliczać po wystrzale, a minimapa pokazywać świat i aktywne statki.
- Sprawdź LPM przez HUD, pauzę, wczytanie zapisu, zmianę sceny i brak drugiej instancji HUD.
- Sprawdź Game View w 16:9, 16:10 i 4:3. Małe rozdzielczości mogą wymagać większych fontów.

Kompilacja kontrolna C# przeszła. Testy importu/prefabów i podglądu są raportowane osobno
w STATUS.md. Odczyt prefabu i render w Edit Mode nie zastępują testu Play Mode.

## Źródło grafiki

Awatar i obręcz koła: wbudowane imagegen, osobne generacje z zatwierdzoną wizualizacją HUD
jako referencją. Prompty: wyodrębniony kremowożaglowy statek bez ramki/tła/tekstów;
oddzielna złoto-mosiężna obręcz steru z ośmioma uchwytami, pustym przezroczystym środkiem,
bez tarczy i napisów. Pozostałe dekoracje są edytowalną geometrią uGUI.
