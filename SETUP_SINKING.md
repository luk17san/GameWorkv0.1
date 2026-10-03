# Zatapianie przeciwnika

Przy 0 HP kadłuba przeciwnik natychmiast traci kolizje, ruch i uzbrojenie.
Przez 3 sekundy opada o 6 jednostek i przechyla się o 18 stopni, następnie
znika wraz ze swoim HUD. Pauza zatrzymuje animację. Już wystrzelone pociski
pozostają aktywne. Ten etap nie zmienia śmierci gracza ani nie dodaje nagród.

CombatShip automatycznie dodaje ShipSinking przy starcie dla drużyn innych
niż 0 i 1. Działa to na obecnych CombatTestEnemy i EnemyAIApproach bez
przebudowy sceny/prefabów. Aby zachować własne ustawienia, poza Play Mode
dodaj ShipSinking na głównym obiekcie prefabu przeciwnika:

- Duration: czas zatapiania w sekundach (domyślnie 3).
- Depth: głębokość opadania (domyślnie 6); dostosuj do wielkości modelu.
- Roll: przechył wokół lokalnej osi Z (domyślnie 18 stopni).

Zapis w trakcie zatapiania zachowuje 0 HP; po wczytaniu przeciwnik jest
od razu usuwany. Zapis po zniknięciu zawiera tylko gracza i usuwa początkowego
przeciwnika ze świeżo wczytanej sceny. Starszy zapis z żywym przeciwnikiem
przywraca go normalnie. Obowiązuje dotychczasowe ograniczenie zapisu do
gracza team=1 i maksymalnie jednego przeciwnika team=2.

## Test w Unity

1. Poczekaj na import skryptów i brak błędów Console. Uruchom ShipSandbox.
2. Zredukuj HP kadłuba przeciwnika do zera. Sprawdź natychmiastowe zatrzymanie
   ognia i wyłączenie wszystkich jego colliderów (również modułów).
3. Przepłyń przez zatapiany statek: brak fizycznej blokady i obrażeń od
   zderzenia. Wcześniej wystrzelona kula może nadal zadać obrażenia.
4. Sprawdź opadanie/przechył, zniknięcie po około 3 sekundach i zniknięcie HUD.
5. W osobnym przebiegu zapauzuj zatapianie: pozycja i przechył nie zmieniają się.
6. Zapisz w trakcie zatapiania, wczytaj: przeciwnika nie ma. Powtórz zapis
   i wczytanie po całkowitym zniknięciu oraz po drugim zapisie tego stanu.
7. W osobnym przebiegu zapisz żywego przeciwnika, pokonaj go i wczytaj zapis:
   wraca z zapisanym HP, ruchem, kolizjami i uzbrojeniem.
8. Zweryfikuj, że zderzenia z żywym statkiem i przeszkodami nadal zadają
   obrażenia oraz że zatapianie nie uruchamia się na graczu.

Kompilacja i sprawdzenia logiki poza Unity nie potwierdzają fizyki,
wyglądu animacji ani kolejności zdarzeń w Play Mode.
