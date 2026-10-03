# AI przeciwnika — etap 3: ostrzał burtowy

## Uruchomienie

1. Poczekaj na kompilację skryptów w Unity i sprawdź Console.
2. Użyj istniejącego EnemyAIApproach w scenie. Nie trzeba generować go ponownie. Jeśli jeszcze go nie masz: Tools > GameWork > AI > Create approach test prefab, następnie przeciągnij prefab na otwartą wodę, 50–60 jednostek od gracza.
3. Na EnemyShipAI sprawdź Enable Maneuvers = włączone. Nowe pole domyślnie jest włączone; jego wyłączenie przywraca zachowanie etapu 1 (podejście i zatrzymanie).
4. Na przeciwniku pozostaw ShipAutoFire, CrewAutoFireSystem i CannonPlayerInput wyłączone. Na EnemyShipAI włącz Enable Weapons (sekcja Broadside Fire, Stage 3) oraz Enable Maneuvers. Ostrzałem kieruje teraz AI przez WeaponBattery. Wyłącz starego CombatTestEnemy, aby w scenie pozostał jeden przeciwnik.
5. Uruchom Play Mode. Zaznacz przeciwnika i obserwuj Current State, Selected Side, Target In Broadside Sector, Salvos Started oraz Last Fired Battery.

## Zachowanie

AI wybiera najbliższy żywy statek przeciwnej drużyny w tej samej scenie. Wybiera burtę na podstawie położenia celu: Selected Side = -1 oznacza lewą, +1 prawą, 0 brak wyboru. Cel dokładnie na wprost daje prawą burtę. Wybór pozostaje stały do utraty celu; AI nie zmienia go przy przekroczeniu dziobu przez gracza.

Z daleka podpływa. Około 45 jednostek od celu zaczyna ustawianie burty. Kierunek płynięcia łączy ruch po łuku z korektą odległości, zamiast prowadzić dziób bezpośrednio na gracza. Dąży do dystansu około 30 jednostek, ale nie utrzymuje go idealnie — wynik zależy od zwrotności, uszkodzeń i ruchu gracza.

Poniżej 18 jednostek przechodzi do GainingDistance: skręca na kurs odchodzący od gracza i płynie z boczną składową manewru. Po odzyskaniu 23 jednostek wraca do manewru burtowego. Przy dużym błędzie kierunku zwalnia lub zatrzymuje napęd, aby wykonać skręt istniejącym kontrolerem. Nie teleportuje statku i nie omija kolizji.

Target In Broadside Sector odczytuje rzeczywiste sektory i zasięgi WeaponBattery skierowanych na wybraną burtę. To informacja geometryczna, nie potwierdzenie możliwości strzału: nie sprawdza przeładowania, stanu dział ani przeszkód na torze pocisku. W obecnym prefabie zasięg baterii wynosi 8–80, sektor 70°. Zmiana tych parametrów nie dostraja automatycznie odległości manewru.

Stany: Waiting, Approaching, PositioningBroadside, Circling, GainingDistance, Returning, Destroyed. HoldingDistance pozostaje dla wyłączonego Enable Maneuvers. Circling oznacza manewrowanie z celem w sektorze. Opuszczenie rejonu przez cel lub AI, utrata celu albo zniszczenie celu powoduje powrót; w trakcie powrotu nie wybiera nowego przeciwnika.

## Parametry

| Pole | Domyślnie | Znaczenie |
|---|---:|---|
| Detection Range | 80 | Wykrywanie celu |
| Lose Target Range | 110 | Utrata celu z powodu odległości |
| Territory Radius | 140 | Rejon wokół pozycji startowej |
| Approach Distance | 30 | Preferowana odległość manewru |
| Resume Margin | 5 | Margines powrotu z odejścia i z manewrowania do podejścia |
| Cruise Speed | 8 | Prędkość podejścia |
| Maneuver Speed | 4.5 | Prędkość manewru, zmniejszana przy dużych skrętach |
| Minimum Distance | 18 | Rozpoczęcie odzyskiwania dystansu |
| Turn In Distance | 15 | Rozpoczęcie manewru przed preferowaną odległością i siła korekty promienia |
| Home Tolerance | 3 | Tolerancja powrotu do miejsca startowego |

Gizma: żółty zasięg wykrywania, błękitny preferowany dystans, fioletowy dystans minimalny, szary rejon, czerwona linia do celu. Odległości dotyczą środków statków; dobierz je do rozmiarów kadłubów.

## Test w Play Mode

- Zatrzymaj gracza. AI powinno podpłynąć, ustawić burtę i płynąć po łuku; Target In Broadside Sector powinno okresowo lub stale być prawdziwe.
- Powtórz start z graczem po drugiej stronie dziobu AI. Sprawdź obie burty, Selected Side -1 i +1.
- Płyń graczem powoli. AI powinno korygować odległość; wybrana burta nie powinna zmieniać się w trakcie jednego pościgu.
- Wpłyń poniżej 18 jednostek: GainingDistance. Po odejściu powyżej 23 AI ma wrócić do manewru burtowego. Nagłe wtargnięcie gracza pod kadłub może wywołać kolizję — brak omijania przeszkód.
- Oddal się poza rejon lub zasięg śledzenia: Returning, następnie Waiting. Nowy pościg wybiera burtę ponownie.
- Sprawdź pauzę, wznowienie, wyłączenie/usunięcie celu oraz zniszczenie kadłuba AI; Console bez nowych błędów.
- W/S/A/D steruje wyłącznie graczem. Przeciwnik rozpoczyna salwy tylko z wybranej burty, gdy cel i bateria spełniają warunki strzału.

## Weryfikacja i ograniczenia

Kompilacja kontrolna całego kodu Framework/ThePirate: bez błędów. 31 sprawdzeń poza Unity obejmuje matematykę podejścia, progi odejścia, kierunki obu burt i uproszczone symulacje manewrów z bliskiej i większej odległości. Symulacja nie obejmuje Rigidbody, kolizji, rzeczywistego cyklu życia Unity ani wyglądu. To historyczne sprawdzenia etapu 2. Etapy 2 i 3 wymagają testu Play Mode; samo przejście do tego etapu nie jest potwierdzeniem wcześniejszego testu.

Brak omijania wysp, przeszkód i przewidywania ruchu celu. Wybór burty nie uwzględnia uszkodzeń uzbrojenia; pozostaje to ograniczeniem obecnego etapu. AI korzysta ze wspólnego kontrolera, ograniczeń ruchu, ładunku i uszkodzeń. Nie gwarantuje utrzymania dystansu od szybszego przeciwnika. Przy braku sterowności lub napędu nie wykona zaplanowanego manewru.

Stan decyzji i wybrana burta nie są zapisywane. Po wczytaniu są wyznaczane ponownie; rejon odnosi się do pozycji w scenie przy Awake, przed odtworzeniem zapisu. Do tego testu użyj świeżo uruchomionej sceny.

Następny etap: weryfikacja pełnego pojedynku, reakcji na zatopienie i strojenie parametrów.

## Ostrzał — etap 3

Enable Weapons domyślnie włączone. Istniejący prefab nie wymaga przebudowy. Do testowania samego ruchu odznacz to pole; wyłączenie Enable Maneuvers także blokuje nowe salwy AI.

AI co Fire Check Interval (domyślnie 0.2 s) sprawdza możliwość strzału, wyłącznie w stanie Circling. Nie strzela podczas podejścia, powrotu, odzyskiwania dystansu, pauzy ani po utracie/zniszczeniu celu. To częstotliwość sprawdzania, nie czas przeładowania — przeładowaniem zarządza WeaponBattery (obecnie bazowo 6 s, z karą za uszkodzenia).

Tylko baterie skierowane na Selected Side dostają polecenie TryFire z bieżącym celem i trybem automatic=true. Bateria sprawdza swój rzeczywisty sektor i zasięg, zdrowie, gotowość oraz drogę z każdej lufy. Nieprzyjaciel wskazany przez AI jest jedynym dozwolonym celem; inne napotkane obiekty blokują daną lufę. Obecna kontrola drogi sprawdza cały zasięg baterii, więc może konserwatywnie blokować strzał także przez obiekt za celem. Nie zmieniono zasad balistyki ani obrażeń.

Salvos Started liczy przyjęte przez baterię żądania salwy w bieżącym uruchomieniu; Last Fired Battery wskazuje ostatnią taką baterię. Nie dowodzi to trafienia ani wystrzelenia każdej lufy — moduł uzbrojenia sprawdza drogę ponownie przy emisji. Conflicting Fire Controller oznacza aktywny ShipAutoFire lub CannonPlayerInput: AI wstrzymuje własne żądania, dopóki te komponenty nie zostaną wyłączone. Pozostaw również CrewAutoFireSystem wyłączony, aby test dotyczył samych burt.

Wyłączenie Enable Weapons lub odejście ze stanu Circling blokuje nowe salwy. Salwą już rozpoczętą zarządza istniejący moduł baterii; wyłączenie przełącznika nie odwołuje wyemitowanych pocisków ani rozpoczętej sekwencji luf. AI celuje w obecną pozycję celu, bez przewidywania ruchu, więc szybki gracz może unikać pocisków. AI nie używa dziobu, rufy, moździerza ani broni załogi.

### Test ostrzału w Unity

1. Uruchom świeżą scenę z jednym przeciwnikiem. Zatrzymaj gracza, pozwól AI ustawić burtę. Sprawdź rzeczywiste pociski, spadek zdrowia po trafieniu oraz wzrost Salvos Started.
2. Sprawdź przerwę przeładowania — licznik nie powinien rosnąć co 0.2 s. Powtórz dla drugiej burty.
3. Umieść collider przeszkody na torze strzału. Sprawdź blokowanie zasłoniętych luf; częściowo odsłonięta bateria może nadal strzelać pozostałymi lufami. Powtórz z innym statkiem na torze.
4. Wyjdź poza sektor lub zasięg. Nowa salwa nie powinna wystartować. Wejście poniżej minimum manewru ma przełączyć na GainingDistance i wstrzymać nowe salwy.
5. Sprawdź pauzę i wznowienie, utratę/usunięcie celu, zniszczenie baterii i kadłuba AI oraz wyłączenie Enable Weapons. Oddziel już lecące pociski od nowych strzałów.
6. Conflicting Fire Controller powinno być odznaczone. Console bez nowych błędów.

Kompilacja kontrolna etapu 3: 0 błędów. 20 sprawdzeń dwóch metod wydawania poleceń, wyodrębnionych bez zmian z produkcyjnego EnemyShipAI, przeszło poza Unity z zastępnikami komponentów. Sprawdzono wybór strony, przekazywanie celu i automatic=true, odstęp żądań, odrzucenie żądania przez baterię, pauzę, stany AI oraz konflikty kontrolerów. Nie testowano w ten sposób wewnętrznej fizyki baterii, przeładowania, kolizji ani Play Mode — te wymagają powyższego testu.
