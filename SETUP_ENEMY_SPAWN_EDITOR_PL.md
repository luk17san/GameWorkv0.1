# Edytor stref przeciwników

Edytor jest już podłączony do spawnowania w Play Mode. Aktualna instrukcja: **SETUP_ENEMY_SPAWN_RUNTIME_PL.md** w katalogu głównym projektu.

1. Otwórz **ThePirate > Edytor przeciwników**.
2. Wybierz istniejącą strefę lub użyj **Dodaj strefę**. Circle: Radius; Rectangle: Rectangle Size. Jednostki świata, obrót Y, Scale = (1,1,1).
3. **Nowy profil** tworzy asset; przypisz prefab EnemyAIApproach lub własny z CombatShip, Health, Rigidbody, EnemyShipAI i poprawnie skonfigurowanym ShipMovementController.
4. **Dodaj grupę**; ustaw Profile, Initial Count >=1, limity i poziomy. Stały poziom: Minimum Level = Maximum Level.
5. Dodaj ręczne Spawn Points lub pozostaw pustą listę dla losowania. Ustaw Obstacle Mask bez warstwy wody i wysokość Y punktów zgodną z prefabem.
6. **Sprawdź konfigurację**, zapisz scenę, uruchom Play Mode. Okno pokazuje diagnostykę dyrektora. Require Outside Camera wyłącz tylko na czas pierwszego widocznego testu.
7. **Otwórz prefab i jego komponenty** w Inspectorze profilu umożliwia edycję uzbrojenia, modułów, nagród i pozostałych systemów.

Duplikowanie przez okno nadaje nowy Zone Id. Profil jest współdzielony; utwórz jego kopię, jeżeli strefa ma mieć inne statystyki. Obsługa Undo i podgląd granic/punktów pozostają dostępne poza Play Mode.
