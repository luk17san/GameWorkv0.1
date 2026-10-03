# Architektura i ustalenia

## Podział projektu

| Miejsce | Odpowiedzialność |
| --- | --- |
| Assets/Framework | Uniwersalne moduły niezależne od gry pirackiej; pierwszym zaimplementowanym modułem jest zdrowie |
| Assets/ThePirate | Sceny i zachowania właściwe tej grze: statek, kamera, później broń i AI |
| Dokumenty w głównym folderze | Plan, status, architektura i instrukcje pracy |

ThePirate może korzystać z Framework. Framework nie może odwoływać się do ThePirate. Na obecnym etapie jest to zasada organizacji kodu, a nie granica wymuszana osobnymi zestawami kompilacji.

Element wydzielamy do Framework, kiedy ma konkretne zastosowanie i nie wymaga wiedzy o statkach. Przykład przyszły: zdrowie zgłasza wyczerpanie, a zachowanie statku odpowiada za zatonięcie. Moduł zdrowia nie powinien sam znać animacji statku ani reguł zwycięstwa.

## Istniejące komponenty

| Komponent | Plik | Co robi |
| --- | --- | --- |
| ShipPlayerInput | [kod](Assets/ThePirate/Scripts/Ship/ShipPlayerInput.cs) | Odczytuje klawiaturę przez Input System i przekazuje polecenia do ruchu |
| ShipMovement | [kod](Assets/ThePirate/Scripts/Ship/ShipMovement.cs) | Przechowuje prędkość zadaną i steruje Rigidbody w FixedUpdate |
| TopDownCamera | [kod](Assets/ThePirate/Scripts/Camera/TopDownCamera.cs) | Śledzi wskazany Transform w LateUpdate |

Przepływ sterowania: `Keyboard.current → ShipPlayerInput → ShipMovement.SetInput → Rigidbody`.

Kamera odczytuje położenie wskazanego celu. Nie odczytuje klawiatury ani parametrów ShipMovement. Cel przypisujemy w Inspectorze, przeciągając PlayerShip z Hierarchy.

## Ustalone zachowanie ruchu

- W zwiększa zadaną prędkość; S ją zmniejsza do zera. Brak pływania wstecz.
- Po puszczeniu W/S zadana prędkość zostaje zapamiętana. Statek stopniowo do niej dochodzi.
- A/D obraca statek podczas trzymania klawisza. W prototypie można obracać się również w miejscu.
- W+S oraz A+D znoszą odpowiednie polecenia.
- Dziób wskazuje lokalną oś +Z. Ruch odbywa się na stałej wysokości, z obrotem wokół Y.
- Rigidbody nie używa grawitacji; blokujemy pozycję Y oraz obroty X/Z.
- Kamera płynnie podąża ze stałym przesunięciem w świecie i nie obraca się ze statkiem.

Parametry i testy opisuje [instrukcja konfiguracji](SETUP_SHIP_MOVEMENT.md). Bieżące wartości konkretnego obiektu mogą różnić się od domyślnych w kodzie.

## Ograniczenia obecnego prototypu

Input jest odczytywany bezpośrednio z Keyboard.current. Nie korzystamy jeszcze z map akcji do sterowania statkiem. Zmiana klawiszy, kontroler i ekran dotykowy wymagają późniejszego rozszerzenia wejścia.

ShipMovement nadaje prędkość w każdym kroku fizyki. Nie modeluje dryfu, wyporności, wiatru ani realistycznej reakcji na kolizje. CurrentSpeed oznacza prędkość wyznaczoną przez sterownik, nie pomiar faktycznego przemieszczania podczas zderzenia.

Utrata fokusu zeruje polecenia klawiszy, ale nie jest systemem pauzy i nie kasuje zadanej prędkości. Wyłączenie ShipMovement zatrzymuje sterowany Rigidbody i zeruje stan ruchu. Wyłączenie samego ShipPlayerInput zeruje tylko polecenia.

## Kierunek dalszej rozbudowy

- Gracz i przyszłe AI powinny używać tych samych mechanizmów ruchu i broni. Zmieniamy źródło poleceń, zamiast kopiować cały system statku.
- Na danym statku tylko jedno źródło powinno wydawać polecenia ruchu; przy sterowaniu AI komponent gracza będzie wyłączony lub nieobecny.
- Model wizualny pozostaje dzieckiem głównego obiektu statku. Można go wymienić bez zmiany sterownika.
- Docelowo LPM obsługuje burty; pociski jednej salwy lecą równolegle. PPM pozostaje przeznaczony na przyszłą broń specjalną.
- Zdrowie przeszło test użytkownika. Użytkownik potwierdził działanie pojedynczego działa 2026-09-27. Burty, AI i wynik starcia pozostają planowane.
- Chodzenie postacią jest opcją na przyszłość. Nie tworzymy teraz systemu przełączania postaci i statku.

## Aktualizowanie dokumentacji

Zmiana zachowania lub zależności wymaga aktualizacji tego pliku. Szczegóły konfiguracji aktualizujemy w SETUP_SHIP_MOVEMENT.md, kolejność prac w PLAN.md, a faktycznie wykonane zadania i wyniki testów w STATUS.md. README.md pozostaje krótkim punktem wejścia.

## Moduł zdrowia — etap 2

[Health.cs](Assets/Framework/Health/Health.cs) przechowuje zdrowie i przyjmuje dodatnie obrażenia przez TakeDamage(int). Inicjalizuje pełne zdrowie w Awake; po inicjalizacji maksimum jest stałe. CurrentHealth, MaxHealth i IsDepleted są publicznie dostępne do odczytu. Zero i ujemne obrażenia oraz trafienia po wyczerpaniu są ignorowane.

Changed przekazuje aktualne i maksymalne zdrowie. Depleted jest wywoływane tylko przy pierwszym osiągnięciu zera. Moduł nie usuwa obiektów i nie zna statków, zatonięcia ani sterowania. Nie obsługuje jeszcze leczenia, wskrzeszania i zapisu. Wyłączenie i włączenie komponentu nie odnawia zdrowia.

[HealthDebugTester.cs](Assets/ThePirate/Scripts/Debug/HealthDebugTester.cs) zależy od Health. Subskrybuje zdarzenia w OnEnable i usuwa subskrypcje w OnDisable; menu komponentu służy do zadawania obrażeń w Play Mode. Nie ma zależności Framework od ThePirate. Testujemy osobny HealthTarget, bez automatycznych zmian statku.

Konfiguracja, ograniczenia inicjalizacji i test: [SETUP_HEALTH.md](SETUP_HEALTH.md).

## Pojedyncze działo — etap 3

[CannonPlayerInput.cs](Assets/ThePirate/Scripts/Combat/CannonPlayerInput.cs) odczytuje pojedyncze kliknięcie LPM w Update i wywołuje TryFire. [Cannon.cs](Assets/ThePirate/Scripts/Combat/Cannon.cs) sprawdza referencje i czas przeładowania, po czym tworzy kulę w FirePoint. Nie odczytuje myszy, więc później może być wywoływany również przez AI lub kontroler burty.

[CannonProjectile.cs](Assets/ThePirate/Scripts/Combat/CannonProjectile.cs) ustawia prędkość Rigidbody wzdłuż +Z punktu wylotu. Przy wystrzale ignoruje collidery w hierarchii Owner. Używa ContinuousDynamic; przy pierwszej kolizji blokuje dalsze trafienia, szuka Health na obiekcie collidera lub rodzicu i zadaje obrażenia. Znika także po trafieniu w przeszkodę bez zdrowia albo po upływie czasu życia.

Domyślnie: 25 obrażeń, prędkość 20, czas życia 5 sekund, przeładowanie 1.5 sekundy. Kierunek zależy od FirePoint, nie od kursora. Jedno kliknięcie to jedna próba strzału; kliknięcia podczas przeładowania nie są kolejkowane. Brak automatycznego ognia przy przytrzymaniu. Nie ma jeszcze celowania, wyboru burty i obsługi kliknięć nad UI.

Skrypty broni są w ThePirate i korzystają z uniwersalnego Health. Nie dodajemy jeszcze interfejsów broni, konfiguracji ScriptableObject ani systemu ponownego używania pocisków. Konfigurację i testy fizyki opisuje [SETUP_CANNON.md](SETUP_CANNON.md).

## Podstawowe menu i przepływ gry

`Assets/Framework/Menu` zawiera `PauseService` (czas, dźwięk, kursor i blokada wejścia), `GameFlow` (ładowanie scen/wyjście), `MainMenuView` i `PauseMenuView` (obsługa przycisków i Esc). Framework nie odwołuje się do ThePirate.

`PirateMenuBootstrap` zna ścieżki scen i ładuje prefab menu pauzy tylko w `ShipSandbox`. `MenuSetupBuilder` tworzy scenę menu, prefab uGUI, powiązania przycisków i listę scen z menu na pierwszej pozycji. Nie modyfikuje zapisanej sceny statku. Odrębne elementy Canvas można edytować w Inspectorze.

Dotychczasowe źródła wejścia (`ShipPlayerInput`, `ShipMovementInput`, `ShipMovementController`, `CannonPlayerInput`, `TopDownCamera`) pytają `PauseService.GameplayInputBlocked`. Dzięki temu pauza, utrata fokusu oraz klatka po wznowieniu nie wywołują poleceń gry. Samo zatrzymanie `Time.timeScale` nie wystarcza do zablokowania `Update`.

Nie ma jeszcze serializacji stanu ani ustawień. `NewGame` ładuje świeżą scenę i nie korzysta z pliku zapisu. Powrót do menu i wyjście potwierdzają utratę bieżącego postępu.

## Zapis i wczytywanie — jeden slot

`Framework.Save.SingleSlotSaveStore` zapisuje i odczytuje JSON w `Application.persistentDataPath`, używając pliku tymczasowego oraz kopii zapasowej. Przy odczycie walidacja odrzuca dane niezgodne z aktualnym formatem. Framework nie zna statków.

`ThePirate.Save.VoyageSaveRuntime` zbiera stan gracza (`CombatShip` drużyny 1) oraz opcjonalnego przeciwnika (drużyna 2), jeśli jest obecny w scenie. Waliduje plik i mapuje dane na komponenty po ponownym otwarciu `ShipSandbox`. Przed zastosowaniem sprawdza liczbę statków oraz ścieżki modułów i baterii, by nie przywracać częściowego stanu. Ścieżki zawierają nazwy i indeksy dzieci; po zmianie struktury prefabów starszy zapis może wymagać migracji.

`SaveMenuActions` obsługuje przyciski w istniejącej scenie i prefabie, a `SaveMenuSetupBuilder` dodaje je w edytorze bez przebudowy menu od początku. `GameFlow` udostępnia ładowanie sceny rozgrywki dla wczytania. Komponenty zdrowia, ruchu i uzbrojenia udostępniają ograniczone metody przywracania stanu. Dane pojedynczych pocisków nie są serializowane.

## Warstwa danych HUD

`ThePirate.UI.HUD` w `Assets/ThePirate/UI/HUD/Runtime` zależy od istniejącego zdrowia,
ruchu i walki. Framework nie zależy od HUD. `ShipHudAdapter` odczytuje systemy statku,
`PlayerHudController` odświeża co 0.1 s, a `PlayerHudStateStore` udostępnia niezmienne
snapshoty i oddzielne zdarzenia zmian. Widoki będą subskrybowały magazyn, bez znajomości walki.
`PlayerHudBootstrap` podłącza gracza drużyny 1 w ShipSandbox podczas uruchomienia sceny.
Wytrzymałość HUD pochodzi tylko z Health, przeładowanie jest osobne dla każdego WeaponBattery.
Nie dodano Canvas ani nowego systemu wiatru/questów/wyboru celu. Szczegóły: SETUP_HUD.md.

## Widok HUD uGUI
PlayerHudView subskrybuje magazyn HUD. Dziewięć prefabów sekcji jest zagnieżdżonych w Resources/GameWork/PlayerHud. Osobne Text, Image i HudShapeGraphic umożliwiają edycję. HudMinimapView tworzy lokalną kamerę i RenderTexture tylko na czas działania widoku. UI nie blokuje kliknięć. Szczegóły: SETUP_HUD_UI.md.
