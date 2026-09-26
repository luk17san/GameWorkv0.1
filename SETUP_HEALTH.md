# Etap 2 — zdrowie i obrażenia

## Co dodajemy

- `Assets/Framework/Health/Health.cs`: uniwersalny komponent zdrowia. Nie zna statku, klawiatury ani sceny.
- `Assets/ThePirate/Scripts/Debug/HealthDebugTester.cs`: pomoc do ręcznego testowania. Wywołuje Health.TakeDamage i wypisuje stan oraz zdarzenia do Console.

Nie usuwamy istniejących plików. Unity wygeneruje `.meta` podczas importu. Scena i prefab nie są automatycznie modyfikowane.

## Jak działa Health

Max Health ustawiamy w Inspectorze przed Play. Awake zapamiętuje maksimum (co najmniej 1) i ustawia pełne zdrowie. Wartości MaxHealth i CurrentHealth można odczytać z kodu, ale zmieniać zdrowie należy przez TakeDamage.

TakeDamage przyjmuje dodatnią liczbę całkowitą. Zero, wartości ujemne i obrażenia po wyczerpaniu zdrowia są ignorowane. Zdrowie nigdy nie spada poniżej zera. Zdarzenie Changed przekazuje aktualne i maksymalne zdrowie po przyjęciu obrażeń. Przy pierwszym osiągnięciu zera następuje również Depleted.

Zdarzenie to informacja dla innych komponentów: np. przyszły pasek zdrowia będzie słuchał Changed, a zachowanie zatonięcia — Depleted. Health sam nie usuwa obiektu i nie zatrzymuje statku. Obecny tester jedynie zapisuje informację w Console.

Włączenie i wyłączenie obiektu nie odnawia zdrowia. Nowa instancja zaczyna z pełnym zdrowiem. Brak leczenia, wskrzeszania, pancerza, zapisu i zatonięcia w tym etapie. Wartość Max Health zmieniaj poza Play Mode; zmiana pola podczas gry nie przelicza bieżącego stanu. Nie zadawaj obrażeń z Awake innych komponentów: inicjalizacja Health musi się zakończyć (np. użyj Start lub późniejszego zdarzenia).

## Konfiguracja w Unity

1. Po imporcie sprawdź Console i otwórz ShipSandbox, poza Play Mode.
2. Utwórz Cube o nazwie `HealthTarget` obok statku, np. Position `(4, 0.5, 4)`. Nie potrzebuje Rigidbody.
3. Dodaj komponent `Health Debug Tester`. Wymagany `Health` powinien dodać się automatycznie.
4. W Health ustaw Max Health `100`. W testerze Test Damage `25`.
5. Zapisz scenę i uruchom Play. Zaznacz HealthTarget.
6. Otwórz menu komponentu **Health Debug Tester** (trzy kropki lub prawy przycisk na jego nagłówku). Wybierz `Test/Apply Damage`.
7. Czytaj Console. `Test/Log State` wypisuje stan bez zadawania obrażeń. Wyłącz Collapse, aby móc policzyć powtarzające się wpisy.

Na razie testujemy osobny cel, aby oddzielić mechanizm zdrowia od sterowania. Nie dodawaj testera do wszystkich statków. Późniejszy pocisk zastąpi ręczne wywołanie TakeDamage.

## Test akceptacyjny

| Czynność | Oczekiwany wynik |
| --- | --- |
| Wejście w Play | HP 100/100 |
| Cztery razy Apply Damage | Kolejno 75, 50, 25, 0 |
| Czwarte trafienie | Jeden wpis ZDROWIE WYCZERPANE |
| Kolejne Apply Damage i Apply Lethal Damage | Nadal 0; brak kolejnego zdarzenia Changed i Depleted; tester nadal wypisuje stan |
| Wyłączenie i włączenie HealthTarget przy 0 HP | Nadal 0; brak ponownego Depleted |
| Zakończenie i ponowne Play (standardowe przeładowanie sceny) | Ponownie 100/100 |
| Apply Lethal Damage przy pełnym zdrowiu | Dokładnie 0, jeden sygnał Depleted |

Obiekt pozostaje widoczny po wyczerpaniu zdrowia — taki jest zakres tego etapu. Zapisz wynik testu w STATUS.md, z aktualnym stanem Console. Następnym etapem jest pojedyncze działo i pocisk.
