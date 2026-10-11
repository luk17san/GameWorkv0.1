# Pływający łup i zbieranie pod E

Po imporcie skryptów generator tworzy tylko brakujące prefaby w `Assets/ThePirate/Resources/GameWork/FloatingLoot`: Planks i Wreckage oraz brązowy materiał Driftwood. Ręcznie: **Tools → GameWork → Economy → Floating loot prefabs**. Nie zapisuje scen i nie nadpisuje istniejących prefabów. Meta tworzy Unity.

Przeciągnij prefab do ShipSandbox, ustaw Y na około 0.25, poza punktem cumowania. W FloatingLoot ustaw Resources (ID wood, iron lub gunpowder i dodatnia Quantity), Bob Height, Bob Speed i Roll. To edytowalne obiekty z prostych brył; docelowe modele można podmienić w prefabie. Nie mają kolizji z kadłubem ani pociskami. Kołysanie jest wizualne względem ustawionej wysokości, nie próbuje symulować wyporu ani odczytywać fal shadera.

ShipLootCollector dodawany jest automatycznie do gracza. Collect Range domyślnie 10 m, mierzone poziomo od środka statku. E zbiera jedną najbliższą paczkę, wszystkie jej materiały razem. Brak miejsca pozostawia obiekt na wodzie. Zawartość i masa aktualizują istniejącą ładownię/HUD. Przy porcie pierwszeństwo ma E cumowania, nawet jeśli cumowanie jest chwilowo zablokowane; aby zbierać, odpłyń poza zasięg portu. Pauza, nieaktywny gracz i brak fokusu okna blokują zbieranie.

ShipResourceReward korzysta z dotychczasowej tabeli łupu, lecz każdą pozycję wystawia jako osobny obiekt obok zatopionego wroga. Drewno ma wygląd szczątków, pozostałe zasoby pływających desek. Loot Surface Height domyślnie 0.25; ustaw wysokość względem powierzchni swojej sceny. Złoto nadal przyznawane jest osobno. Łup reaguje na faktyczne końcowe obrażenia kadłuba tylko raz, nie na wczytanie pokonanego statku. Instrukcje o automatycznym łupie i jego przepadaniu w starszym SETUP_LOOT.md są zastąpione tym etapem.

Zapis v1 otrzymuje opcjonalne floatingLoot: pozycja bazowa, kierunek, wygląd i kopia zasobów. Nowy zapis zastępuje wszystkie aktywne obiekty sceny swoim stanem; pusta lista oznacza, że nic nie pozostało. Starszy zapis bez pola zachowuje ręcznie rozstawione obiekty początkowe. Nie zapisujemy fazy kołysania ani własnych modeli niestandardowego prefabu; odtwarzamy jeden z dwóch prefabów. Limit 4096 obiektów. Brak respawnu lub zanikania po czasie.

## Test w Unity (do wykonania)

1. Poczekaj na import, sprawdź Console i oba wygenerowane prefaby. Rozstaw deski na wodzie i uruchom świeży ShipSandbox. Sprawdź wygląd i kołysanie, E w zasięgu i poza nim, jedną paczkę na naciśnięcie i zmianę ilości/masy ładowni.
2. Zapełnij ładownię. Próba zebrania ma pokazać odmowę, pozostawić obiekt i nie zmienić ekwipunku. Zwolnij miejsce i zbierz ten sam obiekt.
3. Zatop wroga: złoto otrzymasz jak wcześniej, materiały tylko po E przy powstałych paczkach. Powtórne obrażenia nie tworzą drugiego łupu.
4. Sprawdź E przy porcie, odcumowanie, pauzę i ponowne wejście do gry: pojedyncze E nie zbiera i nie cumuje jednocześnie.
5. Zapisz przed walką, podczas zatapiania, po pokonaniu i po zebraniu jednej/wszystkich paczek. Wczytaj każdy stan: zawartość ładowni i nieodebrany łup odpowiadają zapisowi, brak duplikacji. Starszy zapis też powinien się wczytać.

Kontrolna kompilacja źródeł i sprawdzenia logiki zapisu są osobnymi dowodami; nie potwierdzają importu prefabów ani Play Mode.
