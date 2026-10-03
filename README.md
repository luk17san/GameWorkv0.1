# GameWorkv0.1

Gra piracka 3D z kamerą z góry, tworzona w Unity i C#. Projekt służy również nauce: niewielki framework rozwijamy razem z działającą grą, małymi i zrozumiałymi krokami.

## Zacznij tutaj

| Dokument | Do czego służy |
| --- | --- |
| [PLAN.md](PLAN.md) | Cel gry, kolejność etapów i warunki ich ukończenia |
| [STATUS.md](STATUS.md) | Aktualny stan, wyniki sprawdzeń i najbliższe zadanie |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Podział folderów, odpowiedzialności skryptów i decyzje projektowe |
| [SETUP_SHIP_MOVEMENT.md](SETUP_SHIP_MOVEMENT.md) | Konfiguracja statku i kamery oraz test ręczny w Unity |
| [SETUP_HEALTH.md](SETUP_HEALTH.md) | Konfiguracja celu testowego, zdrowie i ręczne zadawanie obrażeń |
| [SETUP_CANNON.md](SETUP_CANNON.md) | Prefab kuli, pojedyncze działo, przeładowanie i test trafień |
| [AGENTS.md](AGENTS.md) | Zasady pracy asystenta nad repozytorium |

## Uruchomienie

1. Otwórz ten projekt w Unity **6000.6.3f1** i poczekaj na import.
2. Otwórz scenę [ShipSandbox](Assets/ThePirate/Scenes/ShipSandbox.unity).
3. Uruchom Play i kliknij widok Game. W/S zmienia zadaną prędkość, A/D skręca.
4. Wykonaj test z instrukcji konfiguracji i zapisz wynik w STATUS.md.

Projekt używa URP i Input System. Wersje pakietów zapisuje [manifest](Packages/manifest.json). Nie zmieniamy ich bez potrzeby zadania.

## Jak zachowujemy ustalenia

Przed kolejnym zadaniem czytamy PLAN.md, STATUS.md oraz opis odpowiednich komponentów w ARCHITECTURE.md. Po pracy aktualizujemy status: co wykonano, jak sprawdzono wynik, czego jeszcze nie potwierdzono i jaki jest następny krok. Zmiana zachowania lub zależności wymaga także aktualizacji architektury i instrukcji konfiguracji, jeśli ich dotyczy.

Dokumentacja jest częścią repozytorium. Zapis lokalny zachowuje ją na tym komputerze; dostęp z innego urządzenia wymaga osobnego commit i push. Sama rozmowa nie synchronizuje plików. Każdy etap kończy się weryfikacją — obecność kodu nie oznacza zaliczonego testu w Unity.

## Menu gry

Podstawowe menu i konfigurację opisuje [SETUP_MENU.md](SETUP_MENU.md).

Instrukcja jednego slotu zapisu: [SETUP_SAVE.md](SETUP_SAVE.md).
