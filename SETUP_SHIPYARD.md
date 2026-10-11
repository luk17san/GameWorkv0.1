# Stocznia — zakup i wymiana statku

Etap 5 ekonomii. Jeden aktywny statek gracza; kupno zastępuje poprzedni egzemplarz. Budowa za zasoby będzie kolejnym etapem.

## Jak działa wymiana

1. Zacumuj w dostępnym porcie i wybierz **Stocznia**.
2. Wybierz projekt. Podgląd pokazuje bazowe HP, prędkość, pojemność i cenę.
3. **Kup i wymień** otwiera potwierdzenie. **Anuluj** zachowuje obecny statek.
4. Potwierdzenie ponownie sprawdza cenę, port, złoto i ładunek. Zbyt ciężki ładunek blokuje zakup, bez usuwania towarów.
5. Nowy egzemplarz przejmuje ilości zasobów, anonimową masę ze starszych zapisów i złoto pomniejszone o cenę. Zaczyna ze zdrowiem bazowego prefabu i bez kupionych ulepszeń. Stary statek znika bez zwrotu ceny; jego uszkodzeń i ulepszeń nie przenosimy.

Nie można ponownie kupić aktualnego typu. To nie jest darmowa naprawa ani magazyn floty. Zamiana startowy → bryg → startowy jest dozwolona i za każdym razem płatna.

Nowy obiekt powstaje najpierw jako przygotowany egzemplarz. Jego Awake inicjalizuje komponenty, potem obiekt jest wyłączany do zatwierdzenia. Ładunek i saldo ustawiamy na nowym obiekcie; saldo starego pozostaje nienaruszone. Sprawdzamy bryłę nowego kadłuba przy pomoście, ignorując własne collidery i poprzedni statek. Dopiero potem stary gracz zostaje wyłączony, nowy przejmuje pomost, kamera i HUD dostają nowy cel, a stary obiekt zostaje usunięty. Odmowa zajęcia pomostu lub wyjątek przypinania widoku przywraca poprzedni statek; nieudany egzemplarz jest usuwany.

Symulacja portu nadal trwa. Obowiązują wcześniejsze warunki cumowania i blokada walki. Zakup jest niedostępny przy pauzie; kliknięcie wymiany tłumi wejście rozgrywki w tej klatce.

## Dane edytowalne w Unity

Po imporcie `ShipCatalogBuilder` tworzy tylko brakujący katalog i dwa projekty. Można wywołać **Tools → GameWork → Economy → Ship catalog**. Istniejącego katalogu, projektów i prefabów generator nie zastępuje.

| Projekt | Cena | Kadłub | Prędkość | Ładownia |
| --- | ---: | ---: | ---: | ---: |
| Statek startowy | 200 zł | Z oryginalnego prefabu, obecnie 500 HP | Obecnie 15 m/s | Obecnie 1000 kg |
| Ciężki bryg | 400 zł | 750 HP | 12 m/s | 1800 kg |

Bryg ma przyspieszenie 3 m/s². Na tym etapie używa kopii obecnego modelu, powiększonej o 15% w X/Z; osobny model nie został dodany. Oba projekty mają osobne kopie danych ruchu i ładowni. Oryginalny `PlayerShip.prefab`, jego konfiguracje, scena i metadane nie są przepisywane przez generator.

Zasoby powstają w `Assets/ThePirate/Resources/GameWork/Ships/`:

- `starter.asset`, `brig.asset`: `ShipDefinition` — stały Type Id, nazwa, opis, prefab, dodatnia cena.
- `starter.prefab`, `brig.prefab`: kadłub, moduły, uzbrojenie i sterowanie. HP ustawiasz w `Health`; parametry żeglugi i pojemności w przypiętych assetach ruchu/ładowni.
- `*-movement.asset`, `*-cargo.asset`: niezależne konfiguracje projektów.
- `../ShipCatalog.asset`: lista dostępnych projektów, maksymalnie 32; ID muszą być unikalne.

Nowy projekt: przygotuj aktywny prefab gracza (Team 1), z jednym `CombatShip`, `Health`, `Rigidbody`, `BoxCollider`, konfiguracją ruchu i ładowni. Ustaw początkowe zdrowie modułów oraz poprawne referencje dział. Utwórz **Create → GameWork → Ships → Ship Definition**, wskaż prefab, wpisz trwałe ID z a-z/0-9/_/-, cenę i nazwę, następnie dodaj definicję do katalogu. Nie zmieniaj ID projektu używanego w zapisach. Walidacja wykrywa podstawowe braki; nie zastępuje testu całego uzbrojenia prefabu.

Zakładka stoczni jest edytowalnym uGUI: `PortMenu.prefab → Window → PortPanel → ShipyardPanel`. Generator rozszerza istniejące menu, zachowując jego referencje i GUID. Ponowne uruchomienie nie dodaje duplikatu sekcji. Runtime ma ten sam fallback układu. Nowe .meta generuje Unity.

## Objaśnienia kodu i zapisu

- `ShipIdentity`: ID typu oraz losowy GUID egzemplarza. Zakup nadaje nowy GUID, wczytanie odtwarza zapisany.
- `ShipDefinition` / `ShipCatalog`: opis oferty i walidacja projektów. Statystyki podglądu pochodzą z bazowego prefabu, ceny z definicji.
- `ShipyardService`: ponowna wycena, warunki zakupu, przeniesienie ładunku/salda i blokada powtórnej operacji.
- `ShipReplacement`: tworzenie, aktywacja, przejęcie pomostu, przypięcie kamery/HUD oraz wycofanie nieudanej zamiany.
- `PortShipyardPanel` / `Layout`: wybór i potwierdzenie, bez samodzielnego naliczania kosztów.
- `ShipCatalogBuilder`: tworzenie pierwszych dwóch ofert z istniejącego prefabu statku.

Zapis v1 otrzymał opcjonalne `shipTypeId` i `shipInstanceId`. Przy wczytywaniu nowego zapisu przygotowujemy prefab odpowiedniego typu, następnie sprawdzamy cały zapis: gracza, przeciwnika, HP, ulepszenia, moduły, baterie, zasoby i port. Zmiana aktywnego gracza następuje dopiero po poprawnym mapowaniu wszystkich zapisanych statków. Nieznany typ, brak prefabu lub niezgodny projekt przerywa operację przed zamianą. Starsze zapisy bez tych pól korzystają z początkowego statku sceny. Nie dodano migracji po zmianie struktury modułów ani efektów ulepszeń.

## Weryfikacja i test Play Mode

Kompilacja kontrolna całego runtime/Editor z bibliotekami Unity: 0 błędów. 44 sprawdzenia produkcyjnych usług/katalogu/ładowni i wyodrębnionego przepływu save oraz 16 sprawdzeń produkcyjnego `ShipReplacement` przeszło z zastępnikami Unity. Obejmują brak złota, zmianę ceny, zbyt ciężki ładunek, błędne zasoby, reentrancję, odmowę pomostu, wyjątek HUD, usuwanie nieudanego egzemplarza, stare/nowe pola zapisu i walidację przed zamianą. Zastępniki nie potwierdzają fizyki, cyklu życia Unity ani JsonUtility.

Nie uruchomiono edytora ani Play Mode dla tego etapu. Testy menu portu i ulepszeń z poprzednich etapów również pozostają niepotwierdzone; wcześniejsze „działa” dotyczyło złota/ładowni.

1. Po imporcie sprawdź Console, utworzenie katalogu, obu prefabów i sekcji stoczni. Uruchom ShipSandbox; nowa gra nadal zaczyna od zera złota.
2. Zdobądź przynajmniej 400 zł. Do szybkiego testu możesz przed Play zwiększyć nagrodę istniejącego przeciwnika w `ShipGoldReward`; przywróć normalną wartość po teście.
3. Zacumuj. Sprawdź dropdown, podgląd, brak złota, zakup bieżącego typu, anulowanie i przełączanie zakładek.
4. Dodaj drewno/żelazo do ładowni, kup ulepszenie kadłuba lub dział. Kup bryg: saldo spada o 400, ilości zasobów zostają, nowy kadłub ma 750 HP i poziomy ulepszeń 0.
5. Odcumuj i sprawdź kamerę, HUD, W/S/A/D, broadsides i moździerz; przeciwnik powinien wykryć nowy statek. Sprawdź, czy nie pozostał stary statek lub jego menu/prompt.
6. Przy ładunku ponad 1000 kg próba powrotu na startowy statek ma być zablokowana. Sprawdź również przeszkodę zajmującą miejsce większego kadłuba: odmowa bez opłaty i utraty poprzedniej jednostki.
7. Zapisz bryg w porcie i na morzu, z uszkodzeniami i ulepszeniami. Wczytaj przez menu: właściwy projekt, GUID, złoto, zasoby, moduły, reload i kamera. Powtórz dla startowego oraz starego zapisu bez ID typu.
8. Sprawdź pauzę, zagrożenie podczas menu, szybkie podwójne kliknięcie, zapis po zwycięstwie i różne rozdzielczości UI. Pełna zamiana i save/load wymagają tego testu w Unity.

Następny etap: receptury budowy za złoto i zasoby, wykorzystujące te same projekty i wymianę aktywnego statku.
