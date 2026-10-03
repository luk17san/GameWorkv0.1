# Zdrowie i HUD przeciwnika

Przeciwnik EnemyAIApproach ma już komponent Framework.Health.Health z Max Health = 500. HUD korzysta z tego samego zdrowia co obrażenia kadłuba i zapis gry. Nie dodano drugiego zasobu HP. Trafienie modułu może uszkodzić moduł zamiast kadłuba; pasek pokazuje wyłącznie zdrowie kadłuba.

## Uruchomienie

1. Poczekaj na import skryptów w Unity. Generator utworzy brakujący `Assets/ThePirate/Resources/GameWork/EnemyHealthHud.prefab`. Istniejący prefab nie jest nadpisywany.
2. Uruchom ShipSandbox. W ciągu 0.5 s HUD pojawi się nad aktywnymi statkami drużyn innych niż 0 i 1. Drużyna 1 jest graczem w obecnym projekcie. Nowo utworzone statki także otrzymują HUD, bez duplikowania komponentu.
3. HUD pokazuje nazwę WROGI STATEK, czerwony pasek i aktualne/maksymalne HP. Przy zerze pasek jest pusty, tekst nazwy zmienia się na ZATOPIONY.

Nie trzeba przebudowywać EnemyAIApproach. Jeżeli prefab UI nie został jeszcze utworzony, identyczny widok powstaje w czasie gry. Kamera rozgrywki musi mieć tag MainCamera. Poza ShipSandbox można dodać EnemyHealthHud ręcznie na obiekcie CombatShip.

## Edycja

- W Project otwórz EnemyHealthHud.prefab. Obiekty Panel, Name, HP, Track i Fill są osobnymi elementami uGUI: można zmieniać rozmiary, pozycje, kolory oraz font. Zachowaj nazwy i hierarchię — skrypt używa ich do podłączenia.
- Jeśli potrzebujesz zaznaczyć lub wygenerować prefab, wybierz Tools > GameWork > AI > Create enemy HUD prefab.
- Na statku można ręcznie dodać EnemyHealthHud poza Play Mode, aby zapisać Display Name, World Offset i Screen Offset. Automatyczne podłączenie rozpoznaje istniejący komponent.
- Maksymalne HP ustawiaj w Health na statku poza Play Mode. MaxHealth jest inicjalizowane w Awake; modyfikacja pola w trakcie gry nie przelicza maksimum.
- CanvasGroup ma wyłączone Interactable i Blocks Raycasts; wszystkie grafiki mają wyłączone Raycast Target. HUD nie blokuje sterowania i celowania.

## Test w Unity

- Przy świeżym starcie domyślnego przeciwnika zobacz 500 / 500; rozpoczęcie z zapisu powinno pokazać przywróconą wartość.
- Traf kadłub: liczba HP i szerokość paska powinny zmaleć. Sprawdź w Inspectorze Health, czy rzeczywiście uszkadzany jest kadłub.
- Sprowadź HP do zera: 0 / 500, pusty pasek, ZATOPIONY. HUD nie tworzy animacji zatopienia; zatrzymanie AI wynika z dotychczasowego systemu zdrowia.
- Przesuń kamerę i użyj zoomu: HUD śledzi statek i zachowuje czytelną wielkość ekranową. Po wyjściu statku poza ekran lub za kamerę znika.
- Wyłącz i włącz statek: HUD znika i wraca z aktualnym HP; usunięcie statku usuwa jego HUD. Powrót do menu i ponowne uruchomienie nie powinny mnożyć widoków.
- Sprawdź kliknięcie przez pasek i Console bez nowych błędów. Powtórz po zapisaniu i wczytaniu gry.

## Zakres weryfikacji

Kontrolna kompilacja kodu Framework/ThePirate z nowymi skryptami i bibliotekami Unity zakończyła się bez błędów. Sprawdzono obecność CanvasRenderer na tworzonych grafikach, wyłączenie przechwytywania kliknięć i parowanie subskrypcji Changed. To kontrola kodu, nie wykonany test wyglądu ani Play Mode. Import, rzeczywista generacja prefabu i powyższe scenariusze wymagają Unity.

HUD jest ekranowy: nie sprawdza zasłaniania przez wyspy i nie rozsuwa nakładających się pasków. Wyłączenie statku ukrywa widok, zniszczenie obiektu usuwa go. Każdy przeciwnik otrzymuje własny Canvas; obecny zakres to test pojedynku, nie duże floty.
