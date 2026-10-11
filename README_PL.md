# PortVisualKit — obiekty UI panelu portu

Zestaw do odtworzenia zaakceptowanego wariantu 3. Grafika powstała przy użyciu wbudowanego imagegen. Wszystkie napisy, wartości, ikony przycisków i paski pozostają osobnymi obiektami Unity uGUI. Tło jest ilustracją, a nie modelem portu 3D.

## Użycie w Unity

1. Zestaw zawiera folder `Assets`. Skopiuj jego zawartość do `Assets` swojego projektu. Dodaje wyłącznie dwa nowe katalogi: `ThePirate/UI/PortVisualKit` oraz `ThePirate/Editor/PortVisualKit`. Nie kopiuj plików kontrolnej kompilacji do projektu.
2. Zaczekaj na zakończenie importu i kompilacji Unity. Wybierz **Tools > GameWork > Ports > Visual kit > Create editable port objects**.
3. Unity utworzy i zaznaczy `Assets/ThePirate/UI/PortVisualKit/Generated/Build-01/PortVisualCanvas.prefab`. Otwórz go dwuklikiem w Prefab Mode. Kolejne uruchomienie tworzy `Build-02`, `Build-03` itd., zachowując poprzednie wersje.

Prefaby i pliki `.meta` powstaną dopiero po wykonaniu punktu 2 w Unity. Zestaw zawiera grafikę i kod generatora, nie ręcznie zapisane pliki prefabów. Generator nie zapisuje sceny i nie zmienia istniejącego `Resources/GameWork/PortMenu.prefab`.

## Grafiki

| PNG w `Assets/ThePirate/UI/PortVisualKit/Sprites` | Przeznaczenie |
| --- | --- |
| `harbor-background.png` | Ilustracja zacumowanego statku i portu, bez interfejsu. |
| `outer-frame.png` | Zewnętrzna rama z drewnem, okuciami, linami i flagami; przezroczysty środek. |
| `button-navy.png` | Zwykły przycisk lub tło ciemnego panelu. |
| `button-red.png` | Przycisk aktywny i główna akcja. |
| `parchment-panel.png` | Pusty pergamin pod kartę szczegółów. |
| `ship-portrait.png` | Osobna ilustracja statku na pergaminie. |
| `icons-atlas.png` | 12 ikon, importowanych jako oddzielne Sprite subassets. |

Ikony: `Ship`, `Anchor`, `Tools`, `Scroll`, `Trade`, `Cargo`, `Tavern`, `Gold`, `Wheel`, `Crew`, `Wood`, `Arrow`. Po generowaniu rozwiń atlas w Project i przeciągaj pojedyncze ikony do pól `Image > Source Image`. Generator znajduje przerwy między rzędami i przycina marginesy każdej ikony; nie wymaga instalowania pakietu 2D Sprite.

Przyciski i pergamin są importowane z granicami 9-slice. Ustawienie `Image > Type = Sliced` zachowuje narożniki podczas zmiany wymiarów. Oryginalne PNG pozostają nienaruszone; przycięcie jest zapisane w ustawieniach Sprite. Zwykłe panele można budować przy użyciu `button-navy`.

## Oddzielne prefaby tworzone przez generator

| Prefab | Obiekty wewnątrz |
| --- | --- |
| `NavButton` | `Button` z tłem, osobną ikoną i edytowalnym napisem. |
| `NavButtonSelected` | Czerwony wariant aktywnej pozycji. |
| `PrimaryButton` | Duży przycisk głównej akcji. |
| `StatusCell` | Tło, ikona, opis i wartość zasobu. |
| `StatRow` | Wiersz parametru statku z opisem, wartością i paskiem. |
| `ProgressBar` | `Slider` 0–1 z osobnym tłem i wypełnieniem. |
| `ShipSummary` | Pergamin, portret, nazwa, parametry, opis i naprawa. |
| `ActiveQuest` | Nazwa zadania, ikona zasobu, postęp i przycisk zadań. |
| `Header` | Tytuł portu oraz złoto, statek, kadłub i ładownia. |
| `Navigation` | Dziewięć przycisków nawigacji. |
| `DockedBadge` | Etykieta zacumowania z ikoną kotwicy. |
| `RepairButton` | Przycisk naprawy statku. |
| `PortVisualCanvas` | Kompletny złożony panel główny portu. |

Elementy są niezależnie edytowalne. Złożony panel zawiera zwykłe obiekty, a prefaby sekcji są wzorcami do ponownego użycia; edycja wzorca nie zmienia automatycznie odpowiadającej mu sekcji w panelu.

## Hierarchy panelu

```text
PortVisualCanvas (Canvas, CanvasScaler, GraphicRaycaster)
  Window
    HarborBackground
    BackgroundShade
    OuterFrame
    Header
      TitlePlate / PanelTitle / PortName
      GoldStatus / ShipStatus / HullStatus / CargoStatus
    Navigation
      Panel
      NavPort / NavEquipment / NavWorkshop / NavShipyard / NavFleet
      NavQuests / NavTrade / NavWarehouse / NavTavern
    SceneView
      DockedBadge
    ShipSummary
      Parchment / ShipPortrait / ShipName / Divider
      Hull / Crew / Cargo / Description / RepairButton
    Footer
      Background
      ActiveQuest
      DepartButton
```

Każdy przycisk ma dzieci `Icon` i `Label`. Wartości liczbowe są obiektami `Text`. Każdy pasek ma `Slider`, `Track` i `FillArea/Fill`.

## Edycja w Inspectorze

- Rozmiar i położenie: `RectTransform`. Bazowy układ: **1920 × 1080**. `CanvasScaler` używa `Expand`, żeby cały układ mieścił się również przy innych proporcjach; na szerszym ekranie zostają dodatkowe marginesy.
- Tekst: `Text > Text`, `Font`, `Font Size`, `Color`. Domyślna czcionka to dostępna w Unity `LegacyRuntime.ttf`, więc typografia nie będzie identyczna z grafiką koncepcyjną. Dla pisma szeryfowego dodaj własną licencjonowaną czcionkę z polskimi znakami jako `Assets/ThePirate/UI/PortVisualKit/Fonts/PortSerif.ttf` przed generowaniem albo przypisz font w obiektach `Text`.
- Postęp: `Slider > Value` od **0 do 1**; np. `0.76` = 76%. Nie zmieniaj ręcznie wymiarów `Fill`, które są sterowane przez Slider. `Fill > Image > Color` zmienia kolor paska.
- Tło lub ikona: `Image > Source Image`. Dekoracje i napisy mają `Raycast Target` wyłączone; aktywne pozostaje na tle przycisku.
- Wybrana pozycja menu: podmień jej `Source Image` na czerwony sprite. Pozostałe pozycje używają granatowego.

## Podłączenie do gry — osobny etap

Generator przygotowuje wygląd głównego ekranu portu. Przykładowe złoto, stan statku i zadanie nie są odczytywane z gry. `Button > On Click()` pozostaje pusty: nawigacja, naprawa i wypłynięcie wymagają podłączenia do istniejących usług. Pozostałe usługi nie mają w tej paczce gotowych paneli treści.

Przy integracji zachowaj istniejącą ścieżkę `PortMenuController → PortMenuView → PortRepairService` oraz układy ulepszeń/stoczni. `PortMenuController` obecnie wymaga komponentu `PortMenuView`; ten wizualny prefab nie jest jego bezpośrednim zamiennikiem. Nie nadpisuj nim `Resources/GameWork/PortMenu.prefab` bez dostosowania i sprawdzenia powiązań.

Źródła do przyszłego podłączenia:

- Nazwa portu: `Window/Header/PortName`.
- Złoto: `Window/Header/GoldStatus/Value`.
- Statek: `Window/Header/ShipStatus/Value` i `Window/ShipSummary/ShipName`.
- Kadłub: wartości `HullStatus/Value`, `ShipSummary/Hull/Value` i ich `Progress/Slider`.
- Ładownia: wartości `CargoStatus/Value`, `ShipSummary/Cargo/Value` i ich `Progress/Slider`.
- Zadanie: `Window/Footer/ActiveQuest/QuestName`, `ProgressValue` i `Progress/Slider`.
- Naprawa: `Window/ShipSummary/RepairButton`.
- Wypłynięcie: `Window/Footer/DepartButton`.

Do testowania kliknięć potrzebny jest działający `EventSystem` z obecnym w grze `InputSystemUIInputModule`. Generator tworzy prefab, nie dodaje EventSystem ani instancji UI do sceny.

## Weryfikacja

Wykonano kontrolną kompilację skryptu z lokalnymi bibliotekami Unity **6000.6.3f1**: 0 błędów. Sprawdzono PNG oraz kanał alfa głównych elementów. Nie uruchomiono importu ani generowania prefabów w edytorze Unity, nie wykonano Play Mode i nie potwierdzono jeszcze wyglądu kompletnego panelu na ekranie Game.

Po imporcie: wygeneruj zestaw, sprawdź Console i Prefab Mode, a następnie obejrzyj panel w testowej scenie przy 1920×1080, 1280×720 i 4:3. Wartości pozostają poglądowe do etapu integracji.
