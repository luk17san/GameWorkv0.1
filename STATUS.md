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
