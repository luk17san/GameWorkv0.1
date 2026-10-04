# Moździerz — 2026-10-03

PlayerShip ma SpecialWeaponController, Cannon na CombatRig/MortarMount oraz wizualizację obszaru trafienia. PPM włącza celowanie, LPM zatwierdza; Escape lub PPM anuluje. Podczas celowania LPM nie strzela z burty. Zielony okrąg oznacza gotowość, czerwony brak zasięgu/przeładowanie/uszkodzenie/blokadę broni.

Inspector / SpecialWeaponController: Minimum Range 20, Maximum Range 100, Flight Time 3 (minimalny czas; wysoki łuk może wydłużyć lot), Minimum Arc Height 25, Reload Time 15, Damage 100, Blast Radius 6. Damage Allies i Damage Owner domyślnie wyłączone. Parametry prefabu gracza zaktualizowano; lokalne nadpisania instancji pozostają nadrzędne.

Pocisk korzysta z grawitacji i zapisuje cel przy strzale. Lot około 4.5 s dla domyślnego wysokiego łuku. Kolizja kończy lot i wywołuje eksplozję; bez collidera wody detonuje na wysokości płaszczyzny celowania. Własne collidery ignorowane podczas lotu, również gdy Damage Owner jest włączone (opcja dotyczy eksplozji). Obrażenia kadłuba naliczane raz na statek, maleją liniowo z odległością najbliższego collidera. Eksplozja nie wymaga bezpośredniej widoczności; może razić za osłoną w promieniu. Neutralne obiekty nie otrzymują obrażeń.

Impact Effect na prefabie CannonProjectile pozwala przypisać efekt wybuchu. Bez przypisanego efektu detonacja usuwa pocisk i nalicza obrażenia bez dodatkowego VFX.

## Test Play Mode

1. Otwórz ShipSandbox poza Play Mode, potem uruchom. PPM, wybór punktu, LPM: tylko jeden pocisk z centralnego mocowania, brak salwy burtowej.
2. Sprawdź Escape/PPM, czerwony cel poza 20–100 m, przeładowanie, ruch statku w trakcie celowania i lotu. Cel nie śledzi wroga.
3. Strzel w wodę, nad niskim pomostem i w wysoką przeszkodę. Ostatni przypadek kończy lot na przeszkodzie.
4. Wróg z wieloma colliderami traci HP kadłuba raz; przy krawędzi mniej niż w centrum. Sojusznik i gracz bez obrażeń przy wyłączonych opcjach; sprawdź osobno obie opcje.
5. Pauza zatrzymuje lot/przeładowanie. Dokowanie i zniszczony moduł blokują strzał. Save/load odtwarza przeładowanie i anuluje celowanie; pociski w locie nie są zapisywane.

Kompilacja kontrolna runtime i Editor: bez błędów. Nie uruchomiono Unity ani Play Mode; fizyka, wygląd i powyższe scenariusze wymagają sprawdzenia.
