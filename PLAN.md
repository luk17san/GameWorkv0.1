# GameWorkv0.1 — plan

## Cel i zakres

Tworzymy grę piracką 3D w Unity i C#, z kamerą z góry. Framework powstaje stopniowo z mechanizmów potrzebnych działającej grze. Priorytetem jest zrozumienie kodu i czytelność projektu.

Najbliższy cel: **grywalny pojedynek dwóch statków**, od rozpoczęcia przez walkę do zwycięstwa albo porażki i restartu.

Obecnie gracz steruje wyłącznie statkiem. Chodzenie postacią po pokładzie lub portach jest możliwością na przyszłość; nie implementujemy teraz jego infrastruktury.

Wcześniejszy plan repozytorium zakłada prototyp PC, później Android, oraz docelową oprawę anime ze słowiańskimi akcentami. Są to dalsze kierunki, poza zakresem obecnego etapu.

## Etapy do pierwszego pojedynku

Poniższa numeracja zastępuje wcześniejszą, ogólną tabelę etapów. Każdy etap wymaga osobnego testu i wpisu w STATUS.md.

| Etap | Zakres | Warunek ukończenia |
| --- | --- | --- |
| 0 | Repozytorium i scena testowa | Scena uruchamia się bez błędów Console |
| 1 | Domknięcie ruchu i kamery, parametry, prefab statku | W/S/A/D i kamera działają zgodnie z ustaleniami; prefab pozwala odtworzyć statek |
| 2 | Uniwersalne zdrowie i obrażenia w Framework | Cel testowy traci zdrowie; wyczerpanie zdrowia zgłaszane jest tylko raz |
| 3 | Jedno działo, pocisk, trafienie i przeładowanie | Pocisk trafia cel, zadaje obrażenia i znika; nie trafia własnego statku; przeładowanie blokuje kolejny strzał |
| 4 | Ostrzał burtowy LPM, wybór burty kursorem | Strzela właściwa burta, pociski lecą równolegle, przeładowanie działa |
| 5 | Prosty przeciwnik używający wspólnego ruchu i uzbrojenia | Podpływa, ustawia burtę, strzela; oba statki mogą zostać zatopione |
| 6 | Zdrowie na ekranie, wynik i restart | Można rozegrać cały pojedynek i uruchomić kolejny bez ręcznego resetowania sceny |

Szczegóły kolejnych etapów, np. reguły wyboru burty i przeładowania, doprecyzujemy przed ich implementacją. Tabela określa kierunek, nie gotowy projekt wszystkich klas.

## Po działającym pojedynku

Kolejny cel to krótka rozgrywka: port → wypłynięcie → walka → nagroda → powrót. Potem dodamy zapis postępu. Wcześniejsza propozycja pokonania trzech wrogich statków pozostaje wariantem tego późniejszego celu, a nie wymaganiem pierwszego pojedynku.

Rozbudowane żagle i wiatr, załoga, handel, questy, streaming świata i chodzenie postacią pozostają poza pierwszym prototypem. Modele, woda i efekty będziemy dodawać po sprawdzeniu podstawowych mechanik.

## Sposób pracy

1. Wybieramy jedno małe zadanie i określamy zachowanie, które ma powstać.
2. Przed implementacją omawiamy pliki, ich odpowiedzialności i zależności.
3. Wprowadzamy zmiany oraz podajemy konfigurację w Inspectorze.
4. Sprawdzamy kod i wykonujemy konkretny test w Unity.
5. Zapisujemy wynik, ograniczenia i następny krok w STATUS.md. Jeśli coś jest niejasne lub nie działa, rozwiązujemy to przed dokładaniem kolejnego systemu.

Nie dodajemy abstrakcji ani pustych menedżerów na zapas. Uniwersalne elementy umieszczamy w Framework, a zachowania pirackie w ThePirate. Szczegóły: [ARCHITECTURE.md](ARCHITECTURE.md).

## Praca komputer–telefon

1. Przed przejściem na telefon zapisz sceny i zaktualizuj STATUS.md, następnie wykonaj commit i push w ramach osobno uzgodnionej publikacji.
2. Zadanie kodowania w chmurze powinno korzystać z aktualnego repozytorium i osobnej gałęzi.
3. Po powrocie zabezpiecz lokalne zmiany, przejrzyj zmiany z chmury i pobierz właściwą gałąź.
4. Uruchom projekt w Unity i wykonaj test opisany w zadaniu.
5. Po poprawnym teście połącz zmiany i uzupełnij status.

Rozmowa nie synchronizuje lokalnych plików. Repozytorium i STATUS.md są źródłem informacji o postępie. Zadanie zakończone przez AI może nadal oczekiwać na sprawdzenie w Unity.

## Równoległy etap: podstawowe menu

Menu główne, nowa gra, pauza i wyjście są przygotowane jako osobny etap infrastruktury gry. Ich test w Unity opisuje SETUP_MENU.md. Po potwierdzeniu działania można kolejno dodać zapis/wczytywanie i ustawienia.

## Zapis i wczytywanie

Po podstawowym menu dodano jeden slot zapisu bieżącej sceny. Test działania i ograniczenia opisuje SETUP_SAVE.md; ustawienia gry pozostają następnym osobnym etapem.
