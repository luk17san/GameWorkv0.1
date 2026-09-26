# Etap 1 — statek i kamera

## Zależności

`ShipPlayerInput` odczytuje W/S/A/D z Input System i przekazuje dwie liczby do `ShipMovement`. `ShipMovement` nie zna klawiatury; zmienia prędkość i obrót przez `Rigidbody` w `FixedUpdate`. `TopDownCamera` potrzebuje tylko wskazanego `Transform` i śledzi go w `LateUpdate`.

Wszystkie trzy skrypty należą teraz do gry (`Assets/ThePirate/Scripts`). Nie dodajemy jeszcze modułów Framework. Istniejący plik InputSystem_Actions nie wymaga zmian: ten prototyp odczytuje Keyboard.current. Przypisywanie klawiszy i sterowanie mobilne pozostają na później.

## Konfiguracja w Unity — poza Play Mode

1. Poczekaj na import skryptów i sprawdź Console. Unity samo utworzy pliki `.meta`.
2. Otwórz `Assets/ThePirate/Scenes/ShipSandbox.unity`.
3. Utwórz pusty obiekt `PlayerShip`: Position `(0, 0.5, 0)`, Rotation `(0, 0, 0)`, Scale `(1, 1, 1)`.
4. Dodaj do niego `ShipPlayerInput`. Wymagane komponenty `ShipMovement` i `Rigidbody` powinny zostać dodane automatycznie; sprawdź ich obecność.
5. Na `PlayerShip` dodaj `Box Collider`: Center `(0, 0, 0)`, Size `(1.5, 0.6, 3)`.
6. Utwórz jako dziecko `PlayerShip` obiekt Cube nazwany `Visual`: lokalne Position `(0, 0, 0)`, Rotation `(0, 0, 0)`, Scale `(1.5, 0.6, 3)`. Usuń Box Collider z tego dziecka. Kształt zmieniamy na dziecku, więc główny obiekt zachowuje skalę 1.
7. Dodaj drugie dziecko Cube nazwane `BowMarker`: lokalne Position `(0, 0.45, 1.1)`, Scale `(0.4, 0.3, 0.4)`. Usuń jego Box Collider. Znacznik wskazuje dziób: lokalna oś +Z (niebieska).
8. Rigidbody: Use Gravity wyłączone, Is Kinematic wyłączone, Linear Damping `0`, Angular Damping `0`, Interpolate `Interpolate`. Constraints: Freeze Position Y oraz Freeze Rotation X i Z. Skrypt również ustawia grawitację, tryb, interpolację i blokady po uruchomieniu.
9. Na Main Camera dodaj `TopDownCamera`. Przeciągnij `PlayerShip` do pola Target. Zostaw Offset `(0, 18, -12)` i Smooth Time `0.2`. W Camera wybierz Projection `Orthographic`, Size `12`.
10. Dodaj Plane o nazwie `TestGround`: Position `(0, 0, 0)`, Scale `(10, 1, 10)`. Dodaj obok trasy kilka sześcianów jako punkty odniesienia, aby widzieć ruch nad jednolitą powierzchnią.
11. Zapisz scenę. Prefab można utworzyć później, po sprawdzeniu sterowania.

## Parametry ShipMovement

| Pole | Domyślnie | Znaczenie |
| --- | --- | --- |
| Max Speed | 10 | Maksymalna prędkość w jednostkach Unity/s |
| Throttle Rate | 5 | Tempo zmiany zadanej prędkości przy W/S |
| Acceleration | 3 | Tempo rozpędzania do zadanej prędkości |
| Braking | 5 | Tempo hamowania do zadanej prędkości |
| Turn Speed | 60 | Stopnie skrętu na sekundę przy A/D |

## Test w Play Mode

1. Kliknij widok Game. Statek początkowo stoi.
2. Przytrzymaj W: stopniowo nabiera prędkości. Puść W: nadal płynie, dochodząc do zapamiętanej prędkości zadanej.
3. Przytrzymaj S: zwalnia i zatrzymuje się. Trzymaj dłużej: nie zaczyna płynąć wstecz.
4. A skręca w lewo, D w prawo. Zwolnienie klawisza kończy obrót. Na tym etapie można obracać statek również w miejscu.
5. W+S oraz A+D wzajemnie znoszą swoje polecenia.
6. Kamera płynnie śledzi statek, zachowując stały kierunek patrzenia podczas skrętów.
7. Zakończ i ponownie uruchom Play: statek znów zaczyna z prędkością zero (przy standardowym przeładowaniu sceny).
8. Sprawdź Console. Zanotuj błędy oraz odczucia dotyczące przyspieszania i skręcania.

To prosty model sterowania: brak dryfu, wyporności i realistycznej reakcji na kolizje. Prędkość jest nadawana przez sterownik w każdym kroku fizyki. Nie jest to jeszcze symulacja żeglowania.
