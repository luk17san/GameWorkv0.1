# Spawnowanie przeciwników — etap runtime

## Twoje istniejące strefy

Nie trzeba tworzyć ich ponownie. Po kompilacji zapisz scenę i uruchom Play Mode. Każda aktywna strefa automatycznie instaluje EnemySpawnDirector w tej samej scenie. Dotychczasowe pliki .meta i konfiguracja scen zostają zachowane.

W oknie **ThePirate > Edytor przeciwników** sprawdź:

1. **Groups > Profile > Prefab**: użyj poruszającego się EnemyAIApproach. Prefab musi mieć włączone EnemyShipAI i ShipMovementController, Movement Stats oraz External Control. Sam model statku albo CombatTestEnemy bez AI nie wystarczy.
2. **Initial Count**: liczba miejsc dla tej grupy, minimum 1, aby coś się pojawiło. **Maximum Alive** grupy i strefy: limity jednoczesnej obsady. Mniejszy limit nie dodaje nowych miejsc ponad Initial Count.
3. **Activation Distance**: odległość środka strefy od gracza, np. 250. Podejdź w jej zasięg.
4. **Obstacle Mask**: wybierz warstwy przeszkód/wysp/statków. Puste pole celowo blokuje spawn; warstwa wody nie może być zaznaczona. Jeśli woda i wyspy mają tę samą warstwę, najpierw rozdziel je w projekcie. Kod nie zmienia warstw automatycznie.
5. **Spawn Points**: ręczne punkty muszą leżeć w strefie. Pusta lista uruchamia losowanie w obszarze. Y strefy/punktów odpowiada wysokości głównego obiektu statku nad wodą, np. 0.5 dla obecnego prefabu, jeżeli taki jest poziom Twojej sceny.
6. **Minimum Player Distance**: np. 100 m. Jeśli cała mała strefa leży bliżej gracza, spawn nie ma dostępnego miejsca.
7. **Require Outside Camera**: domyślnie włączone. Dla pierwszego testu możesz odznaczyć, żeby zobaczyć tworzenie statków. W normalnej grze włącz ponownie. Brak Main Camera blokuje spawn wymagający widoku poza kamerą.
8. **Clearance Radius**: promień bezpieczeństwa kadłuba. **Exclusion Radius** chroni wszystkie PortDock w scenie oraz ręcznie dodane Exclusion Centers.

W Play Mode okno edytora pokazuje **Status** dyrektora, np. brak punktu, gracz poza aktywacją, niepoprawny profil lub osiągnięty limit.

## Limit globalny

Domyślnie 6 żywych przeciwników w scenie, wliczając ręcznie ustawione statki drużyn >=2. Aby zapisać inny limit, poza Play Mode utwórz pusty obiekt EnemySpawnDirector, dodaj komponent o tej nazwie i ustaw **Global Maximum Alive**. Automatyczny instalator wykorzysta istniejący komponent. Używaj jednego dyrektora na scenę.

## Poziomy i odnowienie

Poziom jest losowany jako liczba całkowita z zakresu grupy. Krzywe profilu zmieniają HP kadłuba, obrażenia i przeładowanie WeaponBattery oraz limit prędkości i żądane prędkości AI. Bazowe assety i prefab nie są zmieniane. Krzywe: wartości >0 i <=10; przeładowanie >=0.1; poziomy 1–1000. Broń spoza WeaponBattery, np. osobne specjalne kontrolery, zachowuje statystyki prefabu.

Respawn Limit dotyczy każdego miejsca: -1 bez limitu, 0 bez powrotu po śmierci, 1 jeden powrót. Czas liczony od zniszczenia kadłuba; pauza zatrzymuje go. Zatopiony wrak kończy animację i nie zajmuje limitu żywych.

Oddalony statek jest usuwany tylko poza kamerą i po wygaśnięciu walki. Usunięcie nie wywołuje zniszczenia zdrowia ani nagród, nie zużywa limitu odnowień. Po powrocie statek zostaje wygenerowany od nowa, z pełnym HP. Zachowanie usuniętego statku jest uproszczeniem tego etapu.

## Zapis

Nowy zapis przechowuje osobne identyfikatory dynamicznych statków, poziomy, HP, ruch, moduły, przeładowanie, punkt powrotu AI i stan miejsc odnowienia. Nie zmienia starego slotu, dopóki sam nie zapiszesz gry. Stare zapisy bez danych stref są nadal obsługiwane i rozpoczynają obsadę stref od początku.

Zmiana identyfikatora strefy, liczby miejsc, kolejności grup, nazwy profilu, prefabu lub krzywych może uniemożliwić wczytanie starego spotkania. System zgłasza błąd zamiast przywracać je do przypadkowej grupy. Ręcznie ustawione statki nadal korzystają z wcześniejszego kontraktu: jeden gracz i najwyżej jeden przeciwnik drużyny 2. Wielu przeciwników dodawaj przez strefy.

## Test Twojej sceny

- Dwie strefy, po jednym przeciwniku, poprawny profil i warstwy; na początek wyłącz Require Outside Camera. Sprawdź oba statki i ich poziomy.
- Zatop jednego; sprawdź łup, odnowienie i limit. Włącz wymaganie spawnu poza kamerą.
- Zapisz z dwoma wrogami o różnym HP, wczytaj; nie powinno być duplikatów ani podwójnego łupu.
- Zapisz podczas odliczania odnowienia; wczytaj i sprawdź pozostały czas.
- Sprawdź port, pauzę, oba HUD-y, broń, kolizje i odległe usuwanie w normalnej rozgrywce.

Test automatyczny uruchamia osobną pustą scenę w sesji Unity batch i nie zapisuje scen użytkownika ani slotu gry. Wynik: SpawnVerification/EnemySpawnRuntimeChecks.result w kopii testowej. Zapis jest badany przez ten sam kod capture/apply i JSON, bez zapisu na dysk slotu użytkownika. Unity batch nie ma aktywnego widoku gry: test jawnie krokował klatki Play Mode i wywoływał AdvanceSpawning. Wynik: PASS, 22 sprawdzenia obejmujące także zgodność starego formatu i odtworzenie pozostałego czasu odnowienia. Nie jest to potwierdzenie wyglądu, walki ani konfiguracji Twoich dwóch stref.

