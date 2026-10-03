# Stan projektu

Data aktualizacji: 2026-09-27

## Obecny etap

**Etap 3 — jedno działo i pocisk: zaakceptowany przez użytkownika 2026-09-27.** Użytkownik potwierdził, że działa. Następny planowany etap to ostrzał burtowy; jego implementacja nie została jeszcze rozpoczęta.

Sterowanie i kamera zostały wcześniej zaakceptowane. PlayerShip.prefab istnieje; odtworzenie nowej instancji prefabu nie zostało osobno potwierdzone.

## Baza projektu

- Repozytorium: https://github.com/luk17san/GameWorkv0.1
- Unity: 6000.6.3f1; URP: 17.6.0; Input System: 1.20.0.
- Wcześniej sprawdzony commit bazowy: 572adad1d7e6ee242a881039f0f665d454534d94. To zapis historyczny, nie oznaczenie aktualnego HEAD.
- Manifest zawiera AI Assistant 2.20.0-pre.1 i Unity Pipeline 0.8.0-exp.1; działania tych integracji nie zweryfikowano.
- Scena testowa: Assets/ThePirate/Scenes/ShipSandbox.unity.

## Co jest wykonane

- Etap 2: użytkownik potwierdził poprawne działanie zdrowia i obrażeń.
- Dodano ThePirate/Scripts/Combat/Cannon.cs, CannonPlayerInput.cs i CannonProjectile.cs: pojedynczy strzał LPM, przeładowanie, pocisk z obrażeniami i czasem życia, ignorowanie własnego kadłuba.
- Dodano SETUP_CANNON.md; uzupełniono README.md i ARCHITECTURE.md. W tym kroku nie zmieniano sceny, prefabów, modułu zdrowia ani sterowania.

- Dodano Framework/Health/Health.cs: dodatnie obrażenia, stan zdrowia, zdarzenia Changed i jednorazowe Depleted.
- Dodano ThePirate/Scripts/Debug/HealthDebugTester.cs: menu testowe i komunikaty w Console.
- Dodano SETUP_HEALTH.md; uaktualniono README.md i ARCHITECTURE.md. Sceny i prefab skonfigurowano później po stronie użytkownika.

- Użytkownik potwierdził uruchomienie początkowej sceny bez błędów Console (etap 0).
- Utworzono ShipPlayerInput, ShipMovement i TopDownCamera oraz instrukcję SETUP_SHIP_MOVEMENT.md.
- Odczyt zapisanej sceny potwierdza PlayerShip z komponentami wejścia i ruchu oraz kamerę z TopDownCamera i przypisanym celem. Użytkownik potwierdził przygotowanie sceny testowej.
- Dokumentacja projektu: README.md wskazuje pozostałe dokumenty, PLAN.md opisuje drogę do pojedynku, ARCHITECTURE.md zapisuje odpowiedzialności i zachowania, a ten plik śledzi postęp.

## Sprawdzenia i ich zakres

| Sprawdzenie | Wynik i ograniczenie |
| --- | --- |
| Początkowa scena w Play Mode | Według użytkownika bez błędów; przed pełnym testem nowego sterowania |
| Kompilacja kontrolna trzech skryptów | W poprzednim kroku Roslyn z bibliotekami projektu: bez błędów; CS0649 dotyczył celu kamery przypisywanego przez Inspector |
| Odczyt zapisanej sceny | Komponenty istnieją, pole celu kamery jest przypisane; nie dowodzi to poprawnego zachowania podczas gry |
| Prefab statku | Istnieje Assets/ThePirate/Prefab/PlayerShip.prefab; odczyt potwierdza komponenty ShipMovement i ShipPlayerInput. Test instancji w Unity niepotwierdzony |
| Nowe sterowanie i kamera | 2026-09-26: użytkownik potwierdził poprawne działanie. Asystent nie uruchamiał edytora; nie zgłoszono osobno aktualnego stanu Console |
| Kod zdrowia | Kompilacja kontrolna z bibliotekami tego projektu: bez błędów |
| Logika zdrowia | 12 asercji poza Unity: wartości niedozwolone, zwykłe trafienia, nadmiarowe obrażenia, granice int i jednorazowe zdarzenie wyczerpania, także przy obrażeniach ze zdarzenia Changed. Użyto rzeczywistego Health.cs i minimalnych zastępników API Unity; cykl życia komponentów nie był testowany |
| Zdrowie w Play Mode | Użytkownik potwierdził działanie i przejście do kolejnego etapu; asystent nie uruchamiał Unity |
| Kod działa | Roslyn: trzy nowe skrypty skompilowane z istniejącymi skryptami Framework i ThePirate oraz bibliotekami projektu, bez błędów. Ostrzeżenia CS0649 dotyczą referencji przypisywanych w Inspectorze |
| Działo i pocisk w Play Mode | 2026-09-27: użytkownik potwierdził działanie. Nie przekazał osobnych wyników każdego przypadku z SETUP_CANNON.md; asystent nie uruchamiał Unity |
| Dokumentacja | Uzupełniona o pojedyncze działo i testy pocisku |

## Zgłoszone problemy

Zgłoszono `Invalid AssetDatabase path` z pełną ścieżką projektu. Log wskazał `UnityEditor.ClipboardContextMenu` przy otwieraniu menu pola Inspectora. Zalecono zastąpienie zawartości schowka zwykłym tekstem i przypisywanie celu kamery z Hierarchy. Przyczyna związana z zawartością schowka pozostaje wnioskiem z logu; nie otrzymano osobnego potwierdzenia ponownego testu tego błędu. Nie zmieniano z tego powodu kodu ruchu.

## Następne zadanie

Zaprojektować etap 4: wybór lewej lub prawej burty kursorem, strzał LPM z przypisanych dział, równoległy lot pocisków oraz przeładowanie. Przed implementacją ustalić sposób celowania i podział komponentów. PPM pozostaje na przyszłą broń specjalną.

## Sposób dopisywania wyników

Po każdym zadaniu aktualizujemy bieżący etap i następny krok. Dla sprawdzeń podajemy: datę, kto wykonał test (użytkownik/asystent), co sprawdzono, wynik i ograniczenia. Rozróżniamy odczyt plików, kompilację i test w Play Mode.

Publikacja: obecna aktualizacja dokumentacji jest lokalna. W tym zadaniu nie wykonano commit ani push; dostęp do zmian na innym urządzeniu wymaga osobnej synchronizacji repozytorium.

## Integracja feature/ship-movement-data — 2026-09-27

- Na polecenie użytkownika włączono lokalnie do main osiem commitów gałęzi, do ebadeb78ca358d892bb12c738d9b2149956fb11c (commit scalający, z zachowaniem dwóch własnych commitów main).
- Dodano pięć skryptów w Assets/FrameWork/Runtime/Ships/Movement: ShipMovementStats, ShipCargoStats, ShipMovementState, ShipMovementInput i ShipMovementController.
- Asystent skompilował pięć nowych skryptów narzędziem Roslyn z bibliotekami Unity 6000.6.3f1 i Input System tego projektu: bez błędów; trzy ostrzeżenia CS0649 dotyczą referencji przypisywanych w Inspectorze. Sprawdzenie różnic Git: bez błędów białych znaków.
- Nie uruchomiono edytora ani Play Mode. Nowy kontroler nie jest podłączony do sceny lub prefabu; dotychczasowe ShipMovement i ShipPlayerInput nadal pozostają w projekcie.
- Następny krok: import w Unity i wygenerowanie .meta, utworzenie zasobów Movement Stats i Cargo Stats, konfiguracja nowego kontrolera na obiekcie testowym oraz test W/S/A/D, hamowania i cofania. Nie należy uruchamiać starego i nowego kontrolera ruchu jednocześnie na tym samym statku.
- Zachowano lokalne zmiany .plastic/plastic.changes i .plastic/plastic.wktree. Nie wykonano push. Niniejsza aktualizacja STATUS.md jest częścią lokalnego commitu scalającego.
- Poniżej zachowano notatki źródłowej gałęzi jako zapis historyczny. Wskazana tam scena SampleScene i etap 1 nie zastępują bieżącego stanu projektu opisanego powyżej.

### Historyczne notatki ze scalonej gałęzi
# Stan projektu

Data: 2026-09-27

## Zweryfikowana baza

- Repozytorium: https://github.com/luk17san/GameWorkv0.1
- Unity: 6000.6.3f1.
- URP: 17.6.0; Input System: 1.20.0.
- Scena: Assets/Scenes/SampleScene.unity.
- Framework pozostaje niezależny od Assets/ThePirate.

## Aktywne zadanie

Etap 1: przygotowanie systemu ruchu statku.

Gałąź do przeglądu: `feature/ship-movement-data`.

## Zmiany przygotowane w chmurze

Dodano pierwszą warstwę danych systemu ruchu:

- `Assets/FrameWork/Runtime/Ships/Movement/ShipMovementStats.cs`
  - prędkość maksymalna, przyspieszenie i mnożnik hamowania;
  - procentowe parametry cofania;
  - konfigurowalne punkty zależności zwrotności od prędkości;
  - statek zachowuje 60% domyślnej sterowności na postoju.
- `Assets/FrameWork/Runtime/Ships/Movement/ShipCargoStats.cs`
  - limit ładowności;
  - blokada przekroczenia pojemności;
  - konfigurowalny próg rozpoczęcia kar, domyślnie 40%;
  - osobne kary prędkości i przyspieszenia przy pełnym załadunku.
- `Assets/FrameWork/Runtime/Ships/Movement/ShipMovementState.cs`
  - stan prędkości zadanej i rzeczywistej;
  - efektywne osiągi;
  - prędkość środowiskowa;
  - stan napędu oraz przygotowanie cofania.

## Weryfikacja i ograniczenia

- Sprawdzono strukturę i składnię plików poza Unity.
- Nie uruchomiono Unity ani kompilacji projektu.
- Nie przetestowano zachowania statku w Play Mode.
- Pliki `.meta` dla nowych folderów i skryptów muszą zostać wygenerowane przez Unity po pobraniu gałęzi, a następnie dodane do repozytorium.
- Dodano kontroler poruszający Rigidbody, ale nie zweryfikowano go jeszcze w scenie.
- Kierunek skrętu podczas cofania nie jest obecnie odwracany.

## Dalsze zmiany przygotowane w chmurze

- `ShipMovementInput.cs`
  - korzysta z Input System i akcji Vector2;
  - jest przeznaczony do podpięcia do istniejącej akcji `Player/Move`;
  - nie używa starego `UnityEngine.Input`.
- `ShipMovementController.cs`
  - zapamiętuje prędkość po puszczeniu W;
  - realizuje hamowanie oraz zabezpieczone cofanie;
  - oblicza kary od ładunku i mnożniki uszkodzeń;
  - zachowuje obracanie na postoju;
  - przyjmuje zewnętrzną prędkość wiatru i prądów;
  - porusza statek przez `Rigidbody.MovePosition` i `MoveRotation`.

## Konfiguracja wymagana w Unity

1. Dodać `ShipMovementInput` oraz `ShipMovementController` do obiektu statku.
2. W polu Movement Action przypisać `Player/Move` z `InputSystem_Actions`.
3. Przypisać utworzone zasoby Movement Stats i Cargo Stats.
4. Rigidbody powinien mieć zablokowany obrót osi X i Z.

## Następny krok

1. Pobrać gałąź `feature/ship-movement-data` na komputer.
2. Otworzyć projekt w Unity i pozwolić edytorowi wygenerować pliki `.meta`.
3. Sprawdzić Console pod kątem błędów kompilacji.
4. Utworzyć zasoby `ShipMovementStats` i `ShipCargoStats` w Inspectorze.
5. Dodać komponenty do prostego obiektu testowego i sprawdzić W/S/A/D, utrzymywanie prędkości, obrót na postoju oraz cofanie.

## Poprawka drzenia statku — 2026-09-27

- Po usunieciu duplikatow komponentow uzytkownik nadal zglaszal drzenie. Zapisana scena dziedziczy jedna pare komponentow z PlayerShip.prefab; Rigidbody jest dynamiczne i ma wlaczona interpolacje.
- ShipMovementController: dynamiczne Rigidbody porusza sie przez linearVelocity zamiast MovePosition. Skret i kierunek predkosci korzystaja ze wspolnej rotacji Rigidbody, zamiast pobierania transform.forward z wygladzanego obrazu. Wyzerowano pozostala predkosc katowa przed kontrolowanym skretem, jak w poprzednim sterowaniu. Dla kinematycznego Rigidbody pozostawiono MovePosition.
- Parametry ladunku, przyspieszania, hamowania i cofania pozostaly bez zmian. Nie zmieniono sceny, prefabu ani kamery.
- Asystent: kontrolna kompilacja poprawionego kontrolera z pozostalymi skryptami ruchu i bibliotekami projektu przeszla bez bledow; trzy ostrzezenia CS0649 dotycza pol przypisywanych w Inspectorze.
- Ograniczenie: nie uruchomiono edytora ani Play Mode. Zmiana usuwa podejrzana niezgodnosc sposobu poruszania z typem Rigidbody; nie potwierdzono jeszcze ustapienia drzenia.
- Nastepny krok: test plyniecia prosto, skretow, hamowania i cofania w ShipSandbox po ponownej kompilacji Unity. Jesli drzenie pozostanie, porownac widok nieruchomej kamery z TopDownCamera i sprawdzic czasy klatek.

## Zoom kamery TopDown — 2026-09-27

- Zmieniono Assets/ThePirate/Scripts/Camera/TopDownCamera.cs: kółko myszy przybliża i oddala płynnie przez Input System. Tryb jest odczytywany z Camera.orthographic: Orthographic zmienia rozmiar, Perspective odległość wzdłuż kierunku offsetu, przy stałym kącie patrzenia i FOV.
- Zachowano śledzenie w LateUpdate. Wygładzanie pozycji celu jest oddzielone od wygładzania zoomu. Kierunek kamery nadal nie zależy od obrotu statku. Brak myszy lub celu jest obsługiwany bez błędu. Nie zmieniano scen, prefabów ani plików .meta.
- Inspector / TopDownCamera: Zoom Speed = 2 jednostki na krok, Zoom Smooth Time = 0.15 s; Min/Max Orthographic Size = 5/40; Min/Max Perspective Distance = 8/80. Smooth Time nadal odpowiada za śledzenie. Start: Camera Size w Orthographic, długość Offset w Perspective, ograniczone do ustawionych limitów. Projection wybiera się w komponencie Camera; przełączenie trybu korzysta z osobno zapamiętanego zoomu danego trybu i nie gwarantuje identycznego kadru.
- Asystent: kontrolna kompilacja skryptu Roslyn z bibliotekami Unity 6000.6.3f1 i Input System projektu zakończona bez błędów. Jedno ostrzeżenie CS0649 dotyczy pola Target przypisywanego w Inspectorze. To sprawdzenie kodu, nie test edytora.
- Nie uruchomiono Unity ani Play Mode. Następny krok: w ShipSandbox przetestować kółko w obie strony i dojście do obu limitów w Orthographic oraz Perspective; podczas zoomu płynąć prosto i skręcać, sprawdzić śledzenie, stały kąt i Console. Sprawdzić również zmianę Projection oraz ponowne włączenie komponentu. Minimalną odległość dopasować do wielkości statku i Near Clipping Plane, jeśli model jest obcinany.
- Zmiany lokalne, bez commit i push. Zachowano istniejące zmiany użytkownika; wpis dopisano do STATUS.md.

## Podstawowe menu i przepływ gry — 2026-09-28

- Przygotowano `GameFlow`, `PauseService`, `MainMenuView` i `PauseMenuView` w Framework oraz `PirateMenuBootstrap` i narzędzie `MenuSetupBuilder` w ThePirate.
- Narzędzie Unity tworzy scenę MainMenu, prefab pauzy i dodaje obie sceny do listy budowania. Zachowuje istniejące zasoby menu i otwartą scenę użytkownika. `ShipSandbox` otrzymuje menu pauzy z prefabu w czasie startu.
- Nowa gra wczytuje czystą scenę. Pauza zatrzymuje czas i dźwięk; sterowanie statkiem, strzał i zoom są blokowane, włącznie z klatką wznowienia. Powrót i wyjście z rozgrywki wymagają potwierdzenia.
- Kontrolna kompilacja wszystkich skryptów menu i istniejących skryptów gry z bibliotekami Unity 6000.6.3f1: bez błędów. Ostrzeżenia CS0649 dotyczą pól przypisywanych przez narzędzie edytora lub Inspector.
- 22 sprawdzenia logiki `PauseService` i `GameFlow` poza Unity przeszły; użyto rzeczywistych plików kodu i zastępników API. Nie potwierdza to integracji scen, uGUI ani Play Mode.
- Instrukcja i test ręczny: `SETUP_MENU.md`. Scena/prefab i Play Mode wymagają potwierdzenia w Unity po imporcie.
- Zapis, wczytywanie i ustawienia nie są częścią tego etapu. Następny krok: sprawdzenie menu w Play Mode, potem ustalenie zakresu danych zapisu i wykonanie zapisu/wczytywania.

- Unity wygenerowało MainMenu.unity, PauseMenu.prefab i pliki .meta. Odczyt YAML potwierdził 7 połączeń przycisków oraz referencje skryptów, a EditorBuildSettings.asset wskazuje MainMenu jako pierwszą scenę. Kod całego etapu skompilowano bez błędów (20 ostrzeżeń o polach ustawianych w Inspectorze). Test Play Mode nadal nie został wykonany.

- 2026-09-28: użytkownik potwierdził, że podstawowe menu działa w Unity. Nie przekazał osobnych wyników każdego przypadku z SETUP_MENU.md ani testu zbudowanej gry.

## System baterii i obrażeń statku — 2026-09-28

- Wdrożono ustalenia rozmowy „Omówienie systemu ataku”: wspólne baterie, emisja równoległych salw, celowanie kursorem, LPM, moździerz PPM/LPM, sektory, zdrowie modułów, automatyczny ostrzał, niezależna broń załogi i obrażenia kolizyjne.
- Zachowano integrację pauzy i Input System. ShipConditionController przekazuje stan żagli/steru do istniejącego kontrolera ruchu. Nie zmieniano kodu ruchu, kamery ani menu.
- Generator Unity przygotowuje edytowalny CombatRig na PlayerShip oraz nieruchomy CombatTestEnemy. Nowe .meta generuje Unity; stare GUID-y skryptów zachowane.
- Kontrolna kompilacja wszystkich skryptów Framework/ThePirate z bibliotekami Unity 6000.6.3f1: bez błędów. Ostrzeżenia CS0649 dotyczą pól Inspector. Generator zawiera walidację prefabów i 40 sprawdzeń geometrii i zdrowia w Edit Mode; ich wynik zależy od uruchomienia generatora i jest zapisany w Temp/GameWorkCombatSetup.result.
- Play Mode, fizyka kolizji, wygląd oraz strojenie balansu nie zostały potwierdzone. Instrukcja konfiguracji, ograniczenia i scenariusze: SETUP_COMBAT.md. Brak publikacji na GitHub.

- Wynik importu: Unity wygenerowało PlayerShip/CombatRig, CombatTestEnemy i pliki .meta; walidacja prefabów oraz 40 sprawdzeń geometrii sektorów i zdrowia modułów w Edit Mode przeszły. Play Mode nadal niepotwierdzony.

## Zapis i wczytywanie — 2026-09-28

- Dodano jeden slot zapisu gry z walidacją wersji, plikiem tymczasowym i kopią `.bak` w `Application.persistentDataPath`.
- Zapis obejmuje statek gracza i opcjonalnie przeciwnika, jeśli został umieszczony w scenie; obejmuje ich pozycję, obrót, zdrowie kadłubów/modułów, prędkość, ładunek i pozostały czas przeładowania baterii oraz moździerza. Pociski i rozpoczęte salwy nie są odtwarzane.
- `MainMenu` otrzymuje Wczytaj, a menu pauzy Zapisz / nadpisz i Wczytaj z potwierdzeniem utraty niezapisanych zmian. Nowa gra nie kasuje pliku zapisu. Przyciski pozostają obiektami uGUI.
- Kontrolna kompilacja całego kodu Framework/ThePirate z bibliotekami Unity 6000.6.3f1: bez błędów. 15 sprawdzeń pliku i odzyskiwania kopii poza Unity przeszło. Import UI, Play Mode i zbudowana gra wymagają osobnego potwierdzenia.
- Test ręczny: SETUP_SAVE.md. Zmiany lokalne, bez commit i push.

- Unity wygenerowało elementy zapisu w MainMenu i PauseMenu (w tym potwierdzenie wczytania podczas pauzy) oraz pliki .meta. Odczyt zapisanych zasobów potwierdził przyciski, połączenia metod i skrypty; MainMenu pozostaje pierwszą sceną. Ostatnia kontrolna kompilacja: 0 błędów. 15 sprawdzeń odczytu, nadpisania i odzyskiwania pliku przeszło poza Unity. Zapis i wczytanie rozgrywki w Play Mode oraz w zbudowanej grze nie zostały jeszcze potwierdzone.

## HUD gracza — modele i adaptery, 2026-09-29

- Dodano Assets/ThePirate/UI/HUD/Runtime: HudModels, PlayerHudStateStore, ShipHudAdapter,
  PlayerHudController i PlayerHudBootstrap. Jeden pasek wytrzymałości; osobne zdarzenia
  ruchu, ładowni, baterii, celu/celowania, minimapy, wiatru, zadania i komunikatów.
- Adapter odczytuje istniejące systemy; bootstrap podłącza jedynego aktywnego gracza drużyny 1
  w ShipSandbox podczas uruchomienia. Nie zmieniono zapisanych scen/prefabów ani reguł walki.
- ShipMovementController udostępnia pojemność; WeaponBattery udostępnia liczbę luf i zapamiętany
  czas bieżącego przeładowania. Po wczytaniu save pasek startuje od 0 na pozostały czas.
- Wiatr i zadania oczekują danych z przyszłych systemów. Wybór celu wymaga SetTarget;
  odczyt punktu celowania i warunków strzału jest już podłączony.
- Asystent: kompilacja statyczna Framework/ThePirate z bibliotekami Unity 6000.6.3f1 bez błędów
  (ostrzeżenia CS0649), 19 asercji modeli i magazynu poza Unity przeszło.
- Import/metadane Unity oraz Play Mode niepotwierdzone. Konfiguracja i test: SETUP_HUD.md.
- Następny krok: test adapterów w Play Mode i osobne widoki uGUI w ustalonym układzie.
  Nie wykonano commit ani push.

## HUD gracza — osobne elementy uGUI, 2026-09-29

- Przygotowano generator PlayerHud.prefab i dziewięciu osobnych prefabów sekcji,
  PNG awatara oraz obręczy steru z alfa. Napisy, paski, kontury, ornamenty i pierścienie
  są osobnymi obiektami. Konfiguracja: SETUP_HUD_UI.md.
- Widok subskrybuje PlayerHudStateStore i odpina zdarzenia po wyłączeniu. Bootstrap
  podłącza jedną instancję w ShipSandbox, również gdy HUD umieszczono w scenie ręcznie.
- Minimapa ma kamerę ortograficzną i osobne znaczniki. Wiatr i zadania oczekują źródeł danych;
  wybór celu nadal używa SetTarget. Rodzaj napędu jest konfigurowalnym podpisem.
- Kompilacja statyczna kodu Framework/ThePirate z bibliotekami Unity 6000.6.3f1 przeszła.
  PNG sprawdzone pod kątem alfa. Import, budowa i podgląd prefabu: wynik zostanie dopisany.
- Play Mode pozostaje do sprawdzenia. Nie wykonano commit/push.

## Poprawka CanvasRenderer HUD — 2026-09-29

- Usunięto przyczynę MissingComponentException w CircularMask: HudShapeGraphic wymaga
  teraz CanvasRenderer, a generator dodaje go przed grafiką. Walidacja sprawdza każdy Graphic.
- Uzupełniono 82 brakujące CanvasRenderer w dziewięciu istniejących prefabach sekcji HUD.
  Zachowano istniejące komponenty, układ, kolory, teksty i GUID; root dziedziczy naprawione sekcje.
- Kompilacja kontrolna bez błędów. Sprawdzenie serializacji: unikalne fileID,
  poprawne powiązania i obecność renderera dla każdej grafiki; istniejące pola bez zmian.
- Po imporcie wymagany ponowny start Play Mode. Instancje uruchomione przed poprawką
  nie są dowodem naprawy. Asystent nie wykonał ponownego testu Play Mode.

## AI przeciwnika — etap 1, 2026-09-30

- Dodano EnemyShipAI: wykrywanie wrogiej drużyny, podejście z hamowaniem, utrzymanie odległości z marginesem, przerwanie pościgu i powrót do rejonu startowego, reakcja na utratę celu i zniszczenie. Obsługuje pauzę; otwarta woda, bez omijania przeszkód.
- ShipMovementController otrzymał opcjonalny tryb External Control i polecenia prędkości/skrętu. Domyślne sterowanie gracza zachowane; AI korzysta ze wspólnej fizyki, ładunku i uszkodzeń. Nieodświeżane polecenia wygasają po 0.25 s.
- Dodano narzędzie Tools > GameWork > AI > Create approach test prefab. Tworzy osobny EnemyAIApproach na podstawie CombatTestEnemy, włącza ruch i wyłącza ostrzał na czas testu. Nie nadpisuje istniejącego wyniku ani nie modyfikuje sceny.
- Kontrolna kompilacja całego kodu Framework/ThePirate z nowymi plikami i bibliotekami Unity: 0 błędów. 14 sprawdzeń matematyki, marginesu przełączania i symulowanego hamowania przeszło poza Unity. Nie jest to test rzeczywistej fizyki ani cyklu życia Unity.
- Import, wygenerowanie prefabu i Play Mode pozostają do sprawdzenia według SETUP_ENEMY_AI.md. Stan decyzji AI nie jest zapisywany; po wczytaniu jest wyznaczany ponownie. Nie zmieniono scen ani istniejących prefabów; .meta wygeneruje Unity.
- Następny etap po teście ruchu: ustawianie burty i odzyskiwanie dystansu, następnie ostrzał. Bez commit/push. Kopia zmienianych plików jest w katalogu roboczym enemy-ai-stage/backup-*.

## AI przeciwnika — etap 2, 2026-09-30

- Rozszerzono EnemyShipAI i EnemyNavigationMath: stały wybór burty dla pościgu, wcześniejsze ustawianie burty, ruch po łuku z korektą odległości, odejście poniżej minimum i powrót z marginesem. Nowe stany PositioningBroadside, Circling i GainingDistance; istniejące wartości enum zachowane.
- Enable Maneuvers domyślnie włączone. Istniejący EnemyAIApproach korzysta z nowego zachowania bez przebudowy. Wyłączenie pola przywraca podejście i zatrzymanie z etapu 1. Diagnostyka Target In Broadside Sector odczytuje geometrię rzeczywistych baterii wybranej burty, bez oceny możliwości strzału.
- Kontrolna kompilacja całego Framework/ThePirate z bibliotekami Unity: 0 błędów. 31 sprawdzeń poza Unity przeszło, w tym uproszczone symulacje obu burt i odejścia. Nie sprawdzono fizyki Rigidbody, importu aktualizacji ani Play Mode. Użytkownik poprosił o etap 2; nie zgłosił osobno wyników scenariuszy etapu 1.
- Zaktualizowano SETUP_ENEMY_AI.md. Bez zmian w scenach, prefabach, .meta, uzbrojeniu i kontrolerze ruchu. Ostrzał pozostaje wyłączony; brak omijania przeszkód, wyboru burty według uszkodzeń oraz zapisu decyzji AI.
- Następny krok: test manewrów w Unity, potem etap 3 — ostrzał. Bez commit/push. Kopia zmienionych plików: katalog roboczy enemy-ai-stage2/backup-*.

## AI przeciwnika — etap 3, 2026-09-30

- EnemyShipAI wydaje polecenia TryFire tylko bateriom wybranej burty i tylko w stanie Circling. Przekazuje aktualny cel i automatic=true, zachowując istniejące reguły sektora, zasięgu, uszkodzeń, przeładowania i bezpieczeństwa luf. Interwał sprawdzania 0.2 s; Enable Weapons domyślnie włączone.
- Dodano licznik przyjętych salw, ostatnią baterię oraz diagnostykę konfliktu z ShipAutoFire/CannonPlayerInput. Ostrzał AI nie obejmuje broni załogi, dziobu/rufy ani moździerza. Przełącznik blokuje nowe salwy, nie odwołuje rozpoczętych sekwencji/pocisków.
- Kontrolna kompilacja Framework/ThePirate: 0 błędów. 20 sprawdzeń wyodrębnionych produkcyjnych metod wydawania poleceń przeszło poza Unity z zastępnikami komponentów. Nie jest to test fizyki, rzeczywistego przeładowania, emisji pocisków ani Play Mode. Wcześniejsze etapy nie otrzymały osobnego potwierdzenia scenariuszy testowych.
- Uzupełniono SETUP_ENEMY_AI.md. Zachowano sceny, prefaby, .meta, reguły WeaponBattery i sterowanie. Istniejący EnemyAIApproach korzysta z aktualizacji skryptu. Brak przewidywania ruchu celu i wyboru burty według uszkodzeń.
- Następny krok: test ostrzału i pełnego pojedynku w Unity. Bez commit/push. Kopia plików: katalog roboczy enemy-ai-stage3/backup-*.

## Zdrowie i HUD przeciwnika — 2026-09-30

- EnemyAIApproach ma istniejące 500 HP kadłuba w Health; zachowano wspólny zasób zdrowia i reguły obrażeń. EnemyHealthHud pokazuje nazwę, pasek kadłuba i aktualne/maksymalne HP, aktualizowane przez Changed (również przy odtworzeniu zapisu). Zero HP pokazuje ZATOPIONY.
- EnemyHudBootstrap automatycznie podłącza widok do przeciwników (drużyny inne niż 0 i 1) w ShipSandbox, również po ich utworzeniu. HUD śledzi kamerę MainCamera, ukrywa się poza ekranem i po wyłączeniu statku; usunięcie statku usuwa widok. Grafiki nie przechwytują kliknięć.
- Dodano generator osobnego, edytowalnego EnemyHealthHud.prefab w Resources/GameWork. Po imporcie tworzy wyłącznie brakujący prefab; menu Tools > GameWork > AI > Create enemy HUD prefab pozwala go zaznaczyć/utworzyć ręcznie. Przy braku zasobu istnieje identyczny widok awaryjny tworzony w runtime.
- Kontrolna kompilacja całego Framework/ThePirate: 0 błędów. Kontrola kodu: CanvasRenderer tworzony przed grafikami, raycast targets wyłączone, subskrypcje odpinane. Nie przeprowadzono testu wizualnego, generacji w Unity ani Play Mode.
- Instrukcja: SETUP_ENEMY_HUD.md. Nie zmieniano istniejących scen, prefabów, .meta ani zdrowia. Nowy prefab i metadane generuje Unity. Następny krok: sprawdzić obrażenia kadłuba, zero HP, zoom, wyłączenie/usunięcie statku i zapis/wczytanie. Bez commit/push.

## Trafienia przeciwnika bez spadku HP — 2026-09-30

- Użytkownik zgłosił, że pociski trafiają i znikają, ale HP nie spada. Odczyt sceny wykazał EnemyAIApproach Y=0 przy PlayerShip Y=0.5. Pociski gracza lecą około Y=1.3 (promień 0.15), a górna granica kadłuba przeciwnika wynosiła Y=1.0, więc omijały kadłub i mogły trafiać strefę załogi. Wyrównano Y przeciwnika do 0.5; jedna zmiana wartości w scenie.
- CombatDamage.Receiver: sprawny moduł nadal otrzymuje obrażenia osobno. Zniszczony moduł nie pochłania już kolejnych trafień bez skutku — odbiorcą zostaje nadrzędny Health. Zmiana wspólna dla obu statków; nie zmieniono colliderów ani wartości obrażeń. Trafienie niszczące moduł nadal nie przekazuje nadmiaru obrażeń kadłubowi; dopiero kolejne trafienia.
- Kompilacja kontrolna całego Framework/ThePirate: 0 błędów. Sprawdzenie różnicy sceny: wyłącznie wysokość przeciwnika. To diagnoza geometrii i kodu, nie potwierdzenie Play Mode. HUD nadal pokazuje HP kadłuba, nie modułów.
- Test: ponownie wczytać ShipSandbox poza Play Mode, uruchomić świeżą scenę bez starego zapisu (zapis przechowuje poprzednie Y), sprawdzić Y obu statków = 0.5 i strzelać w kadłub oraz kilkukrotnie w ten sam moduł. Wynik wymaga potwierdzenia w Unity. Bez commit/push; kopia w katalogu roboczym enemy-hit-fix/backup-*.

## Zatapianie przeciwnika — 2026-09-30

- Dodano ShipSinking: przy 0 HP kadłuba natychmiast wyłącza collidery kadłuba/modułów, ruch, AI, uzbrojenie i wizualizację sektorów, zatrzymuje Rigidbody i przełącza je na kinematic. Następnie statek opada/przechyla się przez 3 s i znika wraz z HUD. Pauza zatrzymuje animację. Pociski już w locie pozostają aktywne.
- CombatShip automatycznie podłącza komponent dla drużyn innych niż 0 i 1. Własne Duration/Depth/Roll można zapisać, dodając ShipSinking do prefabu poza Play Mode. Nie zmieniano scen, prefabów ani istniejących .meta; nowe .meta generuje Unity. Śmierć gracza pozostaje poza zakresem.
- ShipCollisionDamage odrzuca zderzenia z kadłubem o 0 HP także po stronie żywego statku. Uszkodzenia żywych statków i przeszkód zachowują istniejące reguły.
- VoyageSaveRuntime: zapis po usunięciu przeciwnika wczytuje gracza i usuwa początkowego przeciwnika ze sceny, po sprawdzeniu zgodności całego zapisu. Zapis podczas zatapiania (0 HP) usuwa przeciwnika od razu po odtworzeniu. Zapis z żywym przeciwnikiem nadal go przywraca; format v1 bez zmian.
- Kompilacja kontrolna całego Framework/ThePirate z bibliotekami Unity: 0 błędów (ostrzeżenia CS0649 pól Inspectora). Osiem sprawdzeń wyodrębnionych produkcyjnych metod mapowania zapisu przeszło poza Unity z zastępnikami obiektów. Sprawdzono usuwanie przeciwnika, zachowanie żywego, brak wymaganego celu i brak częściowych zmian przy niezgodności. Nie jest to test serializacji dyskowej, animacji, fizyki ani cyklu życia Unity.
- Play Mode pozostaje do sprawdzenia według SETUP_SINKING.md: kolizja z pokonanym statkiem, zatapianie, pauza, HUD i zapis/wczytanie przed/w trakcie/po zatopieniu. Bez commit/push. Kopia zmienionych plików: katalog roboczy sinking-stage/backup-*.


## Bezpośrednie zderzenia statków — 2026-10-02

- Użytkownik zatwierdził korektę obrażeń i wytracania prędkości. ShipCollisionDamage: nowy Impact Damage Multiplier = 0.1 oraz Max Impact Health Fraction = 0.25 (maksymalnie 125 HP przy kadłubie 500 HP). Zachowano próg 2 m/s, masę, mnożniki stref i odporność. Obrażenia obu statków wyliczane przed wywołaniem zdarzeń zdrowia. Cooldown pary jest wspólny, z większej wartości obu uczestników. Pozostawanie w kontakcie nie nalicza obrażeń.
- ShipMovementController: Enter/Stay rejestrują poziome normalne kontaktu, następny FixedUpdate uwzględnia rzeczywistą prędkość Rigidbody po fizyce i usuwa składową napędu skierowaną w przeszkodę. Zmniejsza także zapamiętaną prędkość zadaną gracza w dotychczasowym kierunku; W pozwala ponownie rozpędzać się. Polecenia AI zachowane. Pociski i zatopione kadłuby pomijane; restore/disable czyszczą kontakty. Nie zmieniano scen, prefabów, .meta ani zapisu.
- Asystent: kontrolna kompilacja aktualnego kodu runtime Framework/ThePirate z bibliotekami Unity 6000.6.3f1: 0 błędów, ostrzeżenia CS0649 dotyczą pól Inspectora. 17 sprawdzeń wyodrębnionych produkcyjnych metod przeszło poza Unity z zastępnikami API: limit/skalowanie obrażeń, prędkość po zatrzymaniu/odbiciu/cofaniu, zachowanie poleceń AI, blokada ruchu w przeszkodę i zachowanie stycznej. To nie jest symulacja fizyki ani test cyklu życia Unity.
- Następny krok: test Play Mode według sekcji Korekta zderzeń w SETUP_COMBAT.md — czołowe uderzenie, puszczenie/przytrzymanie W, otarcie, cofanie, niska prędkość, pauza i wczytanie. Działanie w Unity niepotwierdzone. Kopia poprzednich plików: katalog roboczy collision-stage/backup-*. Bez commit/push.

## Port i cumowanie — 2026-10-02

- Na zatwierdzenie użytkownika przygotowano PortDock, ShipDocking i generator PortPrefabBuilder. Port obejmuje oddzielne edytowalne obiekty pomostu, lądu, słupków, punkt Berth i wizualną strefę podejścia 12 m. Brakujący prefab generuje Unity po imporcie; menu Tools > GameWork > Ports > Place port in current scene umieszcza instancję z unikalnym ID. Sceny ani istniejących prefabów nie zapisywano automatycznie. Metadane nowych skryptów/folderów i zasoby prefabu generuje Unity.
- E cumuje/odcumowuje. Warunki: żywy gracz, prawidłowy unikalny port w tej scenie, wolne miejsce, prędkość Rigidbody i napędu <=1 m/s, brak blokady walki i przeszkód w miejscu/podejściu. Statek ustawia się na Berth, utrzymuje wysokość, zatrzymuje się i blokuje napęd oraz wszystkie ścieżki uzbrojenia. Komunikat uGUI nie przechwytuje kliknięć. Odcumowanie przywraca poprzedni tryb Rigidbody, zeruje napęd i wymaga ponownego zadania prędkości.
- CombatShip odnawia 10 s blokady po rozpoczęciu salwy, emisji pocisku, ostrzale załogi i faktycznych obrażeniach kadłuba/modułów. Aktywny cel pościgu/walki EnemyShipAI blokuje cumowanie także przy pudłach. Salwa skierowana w cel odnawia jego blokadę. Powrót przeciwnika i jego zatopienie nie są aktywnym pościgiem; ostatnia aktywność nadal odlicza się przez 10 s czasu gry. Pauza zatrzymuje odliczanie. Ponowne zagrożenie/obrażenia zwalniają cumy, bez nietykalności portu.
- Framework.Health otrzymał uniwersalne zdarzenie Damaged (wyłącznie faktyczne TakeDamage, bez restore). Framework nie zależy od ThePirate. Pozostałe integracje są w ThePirate: moduły, Cannon, WeaponBattery, SpecialWeaponController, CrewAutoFireSystem, wejście gracza i kontroler ruchu.
- VoyageSaveData/VoyageSaveRuntime zapisują port ID, zacumowanie i pozostałą blokadę walki. Nowe opcjonalne pola nie zmieniają wersji v1 i umożliwiają odczyt starszych zapisów. Mapowanie portu i zgodność jego położenia są sprawdzane przed modyfikacją statków; brak/duplikat portu powoduje odmowę. Stan zacumowania odtwarzany po ruchu i zdrowiu. Port trzeba dodać i zapisać w scenie poza Play Mode.
- Asystent: kontrolna kompilacja aktualnego runtime i Editor Framework/ThePirate z bibliotekami Unity 6000.6.3f1: 0 błędów (CS0649 dotyczy pól Inspectora). 28 sprawdzeń produkcyjnej logiki z zastępnikami Unity przeszło poza edytorem: faktyczne obrażenia vs restore, zgodność starego/nowego zapisu i błędne stany, odliczanie/odnawianie walki, pościg bez trafień, zwalnianie cum, warunki prędkości, zajętości, przeszkody, sceny i identyfikatora. Nie testowano fizyki ani pełnej serializacji/load w Unity.
- Instrukcja i scenariusze: SETUP_PORT.md. Generacja/import prefabu, wygląd oraz Play Mode pozostają niepotwierdzone. Następny krok: umieścić port menu, zapisać ShipSandbox i sprawdzić E, blokadę walki, odcumowanie i save/load. Bez handlu/napraw/ekranu portu. Kopia plików przed integracją: katalog roboczy port-stage/backup-*. Bez commit/push.