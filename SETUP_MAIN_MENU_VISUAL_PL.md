# Ilustrowane menu główne

Scena: `Assets/ThePirate/Scenes/MainMenu.unity`. Otwórz ją w Unity i uruchom Play. Nowa gra, Wczytaj grę, Ustawienia i Wyjście używają osobnych przycisków. Twórcy pominięci zgodnie z poleceniem.

## Edycja wyglądu

- Prefaby: `Assets/ThePirate/UI/MainMenuVisualKit/Prefabs/`. Każdy ma osobne obiekty `Icon` i `Label`. Akcje są przypisane instancjom w scenie, a szablony prefabów nie odwołują się do obiektów sceny.
- Grafiki: `Assets/ThePirate/UI/MainMenuVisualKit/Sprites/`. Tło jest osobną ilustracją. Logo, powierzchnia przycisku, panel ustawień i ikony mają prawdziwą przezroczystość. Pliki `.sprite.asset` wybierają obszar grafiki bez zewnętrznego marginesu, bez zmiany źródłowego PNG.
- Tekst: osobne komponenty uGUI Text z fontem Cinzel, obsługującym polskie znaki. Font i licencja OFL pochodzą z [Google Fonts](https://github.com/google/fonts/tree/main/ofl/cinzel).
- Canvas używa odniesienia 1600 × 900. `PirateMainMenuLayout` dostosowuje lewą kolumnę i panel ustawień do wysokości, proporcji i `Screen.safeArea`. Tło wypełnia ekran i jest kadrowane niezależnie od UI. Przyciski zachowują proporcje bez rozciągania ornamentów.
- Kolory stanów normalnego, zaznaczonego, wciśniętego i wyłączonego edytuje się w komponencie Button. Wczytaj jest automatycznie wyłączane przy braku poprawnego zapisu. Wyjście ukrywa się w runtime Android/iOS.

## Podłączenie do gry

Zachowano MainMenuView, GameFlow, SaveMenuActions, sceny w Build Settings i GUID MainMenu. Nowa gra przechodzi do ShipSandbox, Wczytaj uruchamia dotychczasową obsługę zapisu. Nie zmieniono formatu ani zasad zapisu. Nie zmieniono pauzy i sceny ShipSandbox.

Ustawienia zawierają głośność ogólną i wybór istniejącego poziomu QualitySettings. Zmiany głośności działają od razu, zapisują się po zamknięciu panelu. Przycisk DŹWIĘK przełącza wyciszenie; zapamiętana głośność pozostaje zachowana. Ustawienia przechowuje PlayerPrefs pod prefiksem `GameWork.Audio` / `GameWork.Graphics`; nie są częścią zapisu podróży.

Modal blokuje główne przyciski i przyjmuje kliknięcia na swoim tle. Powrót lub Escape zamyka panel. PL jest oznaczeniem obecnego języka, bez obietnicy przełączania. Osobnego przełącznika muzyki i karty aktualizacji nie dodano: brak ścieżki muzycznej, lokalizacji i usługi aktualizacji do podłączenia. Nie ma pustych przycisków tych funkcji.

## Narzędzia Unity

`Tools > GameWork > Menu > Apply illustrated main menu` ponownie tworzy cztery szablony i migruje zawartość MenuCanvas. Przed zapisem wykonuje kopię MainMenu w `Documentation/MenuVisualBackups/<data>/`. Zachowuje GUID sceny. **To polecenie przebudowuje UI i zastąpi ręczne poprawki w MenuCanvas oraz szablonach przycisków.** Nie uruchamia się automatycznie podczas importu. Nie używaj dawnych generatorów menu do zmiany pozycji tego nowego układu.

`Start menu preview` uruchamia MainMenu bez zamiany aktualnie edytowanej sceny. Odmawia przy niezapisanych zmianach scen. Po wyjściu z Play Mode przywraca wcześniejszą scenę startową i ustawienie rozmiaru Game View. Podczas podglądu pozwala Unity działać w tle, aby przechwytywane klatki były aktualne.

`Preview 16x9`, `Preview mobile 20x9`, `Preview 4x3` służą do kontroli proporcji w Game View. `Check menu in Play Mode` sprawdza połączenia ustawień, dostępność zapisu, polskie znaki i granice przycisków. Testy chwilowo zmieniają głośność/wyciszenie i przywracają wcześniejsze wartości. `Check New Game action` uruchamia rzeczywiste przejście do rozgrywki.

## Weryfikacja

Kontrolna kompilacja pełnych źródeł Framework/ThePirate z bibliotekami Unity 6000.6.3f1: PASS, 0 błędów. Unity zaimportowało grafiki, font i skrypty, wygenerowało meta oraz cztery prefaby; kontrola referencji, akcji i CanvasRenderer przeszła. Podglądy i testy menu w Play Mode wykonano dla 1600 × 900, 2000 × 900 i 1200 × 900. Sprawdzono otwarcie i zamknięcie ustawień, blokadę wejścia pod modalem, suwak głośności, wyciszenie, dostępność Wczytaj, polskie znaki i położenie przycisków. Podgląd panelu ustawień sprawdzono wizualnie.

Przycisk Nowa gra również sprawdzono w Play Mode: wywołanie jego rzeczywistego zdarzenia uruchomiło aktywną scenę ShipSandbox. Po testach zakończono Play Mode i przywrócono ustawienia podglądu. Kontrola Console nie pokazała błędów menu; historia Pipeline zawiera wcześniejszy timeout polecenia diagnostycznego eval, który ominięto przez zarejestrowane polecenia edytora.

Testy wywołują rzeczywiste zdarzenia Button w Play Mode; nie są potwierdzeniem fizycznego kliknięcia myszą ani dotyku na telefonie. Test na urządzeniu, wycięcia ekranu, instalacja Android/iOS, działanie wczytywania pełnej podróży oraz odsłuch rzeczywistych nagrań pozostają do sprawdzenia. Menu nie dodaje muzyki ani nagrań dźwiękowych.

Podglądy są w `Documentation/MenuVisualPreviews/`; kopia sceny sprzed instalacji jest dodatkowo w workspace ChatGPT `menu-implementation/backup-*`. Bez commit/push.
