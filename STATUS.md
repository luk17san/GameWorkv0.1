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
