# GameWork — prototypy VFX walki morskiej v0.2

Pakiet zawiera pierwszy efekt wystrzału oraz pozostałe efekty z listy. Działa w Unity URP i nie wymaga dodatkowego pakietu Unity.

## Efekty w pakiecie

1. **Wystrzał:** błysk i dym przy lufie (`CannonMuzzleFlashVFX`).
2. **Kula w locie:** dymny ślad na Particle Systemie pocisku (`CannonballTrailVFX`). Widoczny model kuli pozostaje częścią prefabrykatu pocisku.
3. **Trafienie w wodę:** rozbryzg, krople, piana i rozszerzający się krąg (`PlayWaterImpact`).
4. **Trafienie w kadłub:** drzazgi, drobne odłamki i iskry (`PlayHullImpact`).
5. **Trafienie krytyczne:** wzmocnione trafienie kadłuba z dymem (`PlayHullImpact(..., true)`).
6. **Trafienie w ląd:** pył i drobne odłamki (`PlayGroundImpact`).
7. **Dodatkowy pocisk / moździerz:** większy efekt dopasowany do powierzchni (`PlaySpecialImpact`).
8. **Odrzut działa:** krótkie cofnięcie lufy i powrót (`CannonBarrelRecoilVFX`).
9. **Dym po salwie:** dodatkowy dym unoszący się nad punktem montażowym (`PlayLingeringBatterySmoke`).
10. **Uszkodzenia statku:** dym i płomień zależne od wspólnego poziomu wytrzymałości (`ShipDurabilityVFX`).
11. **Naprawa:** drobne iskry i wióry oraz ciepły impuls przy naprawianym fragmencie (`PlayRepair`).
12. **Piana przy statku:** małe rozbryzgi przy linii wody podczas ruchu (`ShipWaterFoamVFX`).
13. **Zatonięcie:** opadanie i przechył wizualnego modelu, dym oraz bąble (`ShipSinkingPresentationVFX`).

## Instalacja

Skopiuj folder `Assets/GameWork/VFX/CannonCombat` do folderu `Assets/GameWork/VFX/` projektu Unity. Po imporcie przypisz tekstury w Inspectorze. Skrypt wystrzału używa tekstur `CannonMuzzleFlash.png` i `CannonSmokePuff.png` z podfolderu `Textures`.

## Podpięcie do systemu walki

Na statku lub obiekcie zarządzającym efektami dodaj `CannonCombatVFX` i przypisz tekstury wody, drzazg, dymu i błysku.

```csharp
// W chwili strzału konkretnego działa:
muzzleFlash.Play();
barrelRecoil.Play();
combatVfx.PlayLingeringBatterySmoke(mountTransform, gunsInMount);

// Po trafieniu pocisku:
if (hitWater) combatVfx.PlayWaterImpact(hit.point, hit.normal);
else if (hitShip) combatVfx.PlayHullImpact(hit.point, hit.normal);
else combatVfx.PlayGroundImpact(hit.point, hit.normal);

// Trafienie krytyczne lub pocisk specjalny:
combatVfx.PlayHullImpact(hit.point, hit.normal, true);
combatVfx.PlaySpecialImpact(hit.point, hit.normal, CannonImpactSurface.Water);

// Naprawa części wytrzymałości:
combatVfx.PlayRepair(repairPoint.position, repairPoint.up, repairedFraction);
```

Wywołuj błysk i odrzut przy każdym faktycznym strzale. Przy salwie opóźnionej efekty uruchamiają się z tym samym opóźnieniem co działa.

## Pocisk

Dodaj komponent `CannonballTrailVFX` do prefabrykatu kuli. Unity doda wymagany Particle System. Przypisz `CannonSmokePuff.png` do **Smoke Texture**. Pocisk powinien już mieć własny widoczny model i collider.

## Odrzut działa

Dodaj `CannonBarrelRecoilVFX` na obiekt działa, a w polu **Barrel** wskaż transform lufy. Skrypt przesuwa wyłącznie wizualną lufę; nie zmienia logiki strzału ani fizyki statku.

## Uszkodzenia i naprawa

Dodaj `ShipDurabilityVFX` do statku, przypisz punkty **Smoke Anchor** i **Fire Anchor** oraz tekstury. Po zmianie wspólnej wytrzymałości wywołaj:

```csharp
durabilityVfx.SetDurability01(currentDurability / maxDurability);
```

Dla naprawy wywołaj `PlayRepair(...)` po zwiększeniu wytrzymałości. Efekt wizualny nie zmienia wartości zdrowia.

## Piana i zatonięcie

Dla piany przypisz Rigidbody statku oraz dwa puste obiekty przy linii wody: lewy i prawy. Efekt aktywuje się po przekroczeniu progu prędkości.

Dla zatonięcia przypisz `ShipSinkingPresentationVFX` i ustaw **Ship Visual** na dziecko zawierające wyłącznie widoczny model. Metoda `PlaySinking()` porusza tym dzieckiem; nie przypisuj tu korzenia z colliderami ani Rigidbody.

## Ograniczenia prototypu

Parametry i skale są wartościami startowymi do strojenia w scenie. Ten zestaw nie zawiera poolingu, dźwięku ani finalnych shaderów; Unity i projekt nie były dostępne w tym workspace do sprawdzenia kompilacji w edytorze. Efekty są wizualne i nie wpływają na obrażenia, kolizje ani wytrzymałość.
