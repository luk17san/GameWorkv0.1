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
- Na tym etapie klasy przechowują i obliczają dane; nie poruszają jeszcze obiektu.

## Następny krok

1. Pobrać gałąź `feature/ship-movement-data` na komputer.
2. Otworzyć projekt w Unity i pozwolić edytorowi wygenerować pliki `.meta`.
3. Sprawdzić Console pod kątem błędów kompilacji.
4. Utworzyć zasoby `ShipMovementStats` i `ShipCargoStats` w Inspectorze.
5. Następnie dodać `ShipMovementInput` oraz `ShipMovementController`.
