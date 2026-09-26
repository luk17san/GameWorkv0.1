# Etap 3 — jedno działo i pocisk

## Podział odpowiedzialności

Wszystkie trzy nowe skrypty znajdują się w `Assets/ThePirate/Scripts/Combat`:

| Skrypt | Rola |
| --- | --- |
| CannonPlayerInput | Jedno kliknięcie LPM próbuje oddać jeden strzał |
| Cannon | Przechowuje parametry, pilnuje przeładowania i tworzy pocisk w FirePoint |
| CannonProjectile | Porusza się przez Rigidbody, ignoruje własny statek, trafia w Health i znika |

Zależności: `Mouse → CannonPlayerInput → Cannon → CannonProjectile → Health`. Framework nie zna broni ani statków. Nie zmieniamy zdrowia, ruchu ani kamery.

## Przygotuj prefab kuli — poza Play Mode

1. W ShipSandbox utwórz Sphere o nazwie `Cannonball`.
2. Ustaw Scale `(0.3, 0.3, 0.3)`. Zostaw jeden Sphere Collider (Radius `0.5`, Is Trigger wyłączone).
3. Dodaj `Cannon Projectile`. Rigidbody powinien dodać się automatycznie.
4. W Rigidbody ustaw Use Gravity wyłączone, Is Kinematic wyłączone, Linear Damping `0`, Interpolate `Interpolate`, Collision Detection `Continuous Dynamic`. Skrypt ustawia je także przy uruchomieniu. Nie dodawaj kolejnych colliderów ani colliderów na dzieciach kuli.
5. Przeciągnij Cannonball do `Assets/ThePirate/Prefab`, tworząc `Cannonball.prefab`. Prefab i komponent Cannon Projectile mają pozostać aktywne.
6. Usuń roboczą kulę z Hierarchy. Działo będzie tworzyć nowe instancje podczas strzałów.

## Dodaj jedno działo

1. Jako dziecko PlayerShip utwórz pusty obiekt `TestCannon`: lokalne Position i Rotation `(0, 0, 0)`, Scale `(1, 1, 1)`.
2. Dodaj `Cannon Player Input`. Komponent Cannon powinien dodać się automatycznie.
3. Pod TestCannon utwórz pusty obiekt `FirePoint`: lokalne Position `(0, 0, 2)`, Rotation `(0, 0, 0)`. To przykład dla początkowej bryły statku o długości 3; przy większym modelu przesuń punkt przed kadłub.
4. Kierunek strzału to niebieska oś +Z obiektu FirePoint. Na tym etapie działo strzela prosto w tym kierunku, niezależnie od kursora.
5. W Cannon przypisz Owner = główny PlayerShip z Hierarchy; Fire Point = FirePoint; Projectile Prefab = Cannonball z okna Project.
6. Zostaw Damage `25`, Projectile Speed `20`, Projectile Lifetime `5`, Reload Time `1.5`.
7. Zapisz scenę. Jeżeli chcesz zachować działo w PlayerShip.prefab, zastosuj te zmiany do prefabu przez Overrides → Apply All po sprawdzeniu, że pozostałe nadpisania też mają zostać zapisane. Nie zapisuj obiektu kamery w prefabie statku.

## Ustaw cel

1. Użyj HealthTarget z poprzedniego etapu, z Health i HealthDebugTester.
2. Dla statku w pozycji `(0, 0.5, 0)` i rotacji `(0, 0, 0)` ustaw cel na `(0, 0.5, 10)`. Jeśli statek ma inne położenie, ustaw cel przed FirePoint na tej samej wysokości.
3. Cel musi mieć włączony Box Collider z Is Trigger wyłączonym. Health może znajdować się na obiekcie collidera lub jego rodzicu.
4. Na pierwszą próbę nie ruszaj statkiem. Usuń z linii strzału przeszkody, także wodę z fizycznym colliderem; trafienie w dowolny zwykły collider kończy lot kuli.

## Test w Play Mode

1. Uruchom Play, kliknij Game. Kliknięcie aktywujące widok może już wystrzelić kulę.
2. LPM: kula wylatuje z FirePoint, trafia cel i znika. Console testera pokazuje spadek zdrowia o 25.
3. Szybkie kolejne kliknięcia nie powinny strzelać częściej niż co 1.5 sekundy. Kliknięcie podczas przeładowania przepada; przytrzymanie przycisku nie daje automatycznego ognia.
4. Cztery skuteczne trafienia sprowadzają cel ze 100 do 0; Depleted pojawia się raz. Cel pozostaje widoczny zgodnie z poprzednim etapem.
5. Skręć statkiem i strzel obok celu: kula powinna zniknąć po 5 sekundach (czas gry). Nie powinna zostawać w Hierarchy.
6. Ustaw pomiędzy działem a celem Cube z colliderem, bez Health. Pocisk powinien zniknąć na przeszkodzie, a cel zachować zdrowie.
7. Sprawdź własny kadłub: na czas próby dodaj Health i HealthDebugTester na PlayerShip, przesuń FirePoint do wnętrza jego collidera i strzel. Statek nie powinien tracić zdrowia ani blokować wyjścia kuli. Po teście przywróć punkt wylotu i usuń tymczasowy tester oraz Health ze statku, jeśli służyły tylko tej próbie.
8. Dla celu z kilkoma colliderami sprawdź, że pojedyncza kula zabiera tylko 25 HP. Collidery celu powinny mieć wspólnego rodzica z jednym Health.
9. Sprawdź Console pod kątem błędów. Zapisz wynik w STATUS.md.

Unity generuje `.meta` nowych skryptów i folderów. Scena i prefab wymagają powyższej konfiguracji — asystent nie przebudowuje ich automatycznie.

## Granice prototypu

Pocisk leci prosto, bez grawitacji i bez dziedziczenia prędkości statku. Maksymalny dystans przy domyślnych parametrach to około 100 jednostek. Przeładowanie i czas życia korzystają z czasu gry.

Własne collidery są ignorowane według hierarchii Owner w chwili strzału; ustaw Owner na cały statek. Nie dodawaj colliderów do kadłuba podczas lotu kuli. Warstwy celu i pocisku muszą móc się zderzać w ustawieniach fizyki. Triggery nie są celami trafień w tej wersji.

CCD zmniejsza ryzyko przelatywania przez cienkie collidery, ale trafienia przy wybranej prędkości i geometrii trzeba sprawdzić w Unity. Brak wyboru burty, celowania kursorem, efektów, UI, blokowania strzału nad przyszłym UI i systemu zatonięcia. Nie testujemy tu jeszcze salw.
