# Rozwój statku, gospodarka i port — plan logiki

Ten dokument opisuje docelowe zależności. Zaimplementowano etap 1 (złoto), etap 2 (katalog zasobów i ładownię) etap 3 (menu portu i naprawy za złoto) etap 4 (pierwszy poziom kadłuba, napędu i baterii) etap 5 (katalog projektów i zakup/wymianę aktywnego statku) etap 6 (budowę za złoto i materiały) oraz etap 7 (materiałowy łup po zatopieniu). Pozostałe propozycje doprecyzujemy przed odpowiednim etapem i sprawdzimy osobno w Unity.

## Pętla gry

Port → wypłynięcie → walka → nagroda → powrót → naprawa/ulepszenie/nowy statek.
Najpierw jedna waluta, później surowce. Złoto nie zajmuje ładowni. Nowa gra tworzy nowy stan, ale nie kasuje istniejącego pliku zapisu.

## 1. Złoto (implementacja)

GoldWallet w Framework przechowuje nieujemną liczbę całkowitą i zgłasza Changed przy rzeczywistej zmianie salda. TryCredit/TrySpend odmawiają operacji o wartości <=0, braku pieniędzy i przepełnienia int. TryRestore zastępuje saldo (także zerem).
PlayerGold w ThePirate wiąże portfel ze statkiem drużyny 1. Nie przechowujemy pieniędzy w statycznym singletonie: nowa scena rozgrywki daje nowy portfel z saldem 0.
ShipGoldReward przyznaje domyślnie 100 złota raz na życie wrogiego obiektu, kiedy prawdziwe obrażenia sprowadzą kadłub do 0. Korzysta z Health.Damaged, a nie Depleted, bo Depleted jest emitowane również podczas wczytywania. Odbiorcą jest jedyny żywy gracz w tej samej scenie. Drużyna neutralna i gracz nie dają nagród. Zatopienie przez zderzenie również daje nagrodę. Nie przypisujemy ostatniego trafienia do strzelca — obecne API Health nie ma takiej informacji.
Saldo zapisuje VoyageSaveData.gold. Starszy zapis v1 bez tego pola oznacza 0. Ujemne saldo powoduje odrzucenie zapisu. Całe mapowanie statków jest sprawdzane przed zastąpieniem salda. Zapis w trakcie zatapiania obejmuje już przyznaną nagrodę; wczytanie nie przyznaje jej drugi raz.

## 2. Katalog zasobów i ładownia

ResourceDefinition (ScriptableObject): stałe unikalne ID, nazwa, opis, ikona, masa jednostkowa, cena bazowa i kategoria. Zapis używa ID, nie nazwy ani ścieżki assetu. Edytor katalogu tworzy nowe definicje i sprawdza duplikaty, brak ID oraz błędną masę/cenę. Zmiana nazwy nie zmienia ID. Usunięte ID w zapisie wymaga jawnej migracji albo odmowy wczytania z wyjaśnieniem.
Ładownia przechowuje pary ID/ilość; ilości są dodatnimi liczbami całkowitymi. Masa jest sumą masa × ilość. Dodanie sprawdza pojemność przed mutacją; usunięcie sprawdza dostępny zapas. Nie przyjmujemy części nagrody bez pokazania tego graczowi. Przeciążenie po zmianie typu statku blokuje wymianę do czasu rozładunku.
Ładownia synchronizuje faktyczną masę z istniejącym ShipMovementController.SetCargoWeight. Nie tworzymy drugiej niezależnej symulacji masy. Wyposażenie zamontowane na statku jest oddzielne od towarów.

## 3. Port i naprawa

Menu dostępne dla żywego, faktycznie zacumowanego gracza. Korzysta z obecnych reguł cumowania i walki. Odnowienie zagrożenia lub obrażenia zwalniają cumy i zamykają usługi; przed zatwierdzeniem ponownie sprawdzamy warunki.
Naprawa kadłuba i modułów ma osobny podgląd brakujących HP oraz kosztu. Koszt wynika z faktycznie odtworzonego zdrowia. Zero uszkodzeń daje koszt 0 i nie pobiera pieniędzy. Przy braku złota odmawiamy pełnej naprawy. Dodatnie zdrowie odtwarzamy API naprawy/restore, a nie ujemnymi obrażeniami.

## 4. Ulepszenia statku i dział

Definicje opisują poziomy, koszt oraz wynikowe parametry: kadłub, napęd, pojemność i baterie dział. Stan zapisuje poziomy; statystyki wyliczamy od bazowego projektu, aby wczytanie nie dodawało mnożników wielokrotnie. Ulepszenie dotyczy wybranej baterii albo jasno wskazanej grupy.
Pierwsza wersja: jedno ulepszenie kadłuba, napędu i dział. Podgląd pokazuje wartości przed/po i koszt. Ulepszenie kadłuba zachowuje brakujące HP, więc nie zastępuje naprawy. Przy zmianie parametrów broni nie resetujemy przeładowania dla darmowej salwy.

## 5. Kupowanie i wymiana statku

ShipDefinition jest projektem jednostki: ID, prefab, parametry bazowe, pojemność, cena i dozwolone ulepszenia. OwnedShipState jest stanem konkretnego egzemplarza: ID egzemplarza, typ, zdrowie, moduły i poziomy ulepszeń. Pierwsza wersja utrzymuje jeden aktywny statek, bez floty i przechowywania starych jednostek.
Przed wymianą sprawdzamy port, pieniądze, prefab, zgodność ładunku i miejsce pojawienia. Nowy statek przejmuje ładunek i przypięcie kamery/HUD/sterowania; zdrowie i ulepszenia starego statku nie są automatycznie kopiowane. Najpierw przygotowujemy nową jednostkę, potem pobieramy koszt i usuwamy poprzednią. Błąd przygotowania nie może zabrać złota. Zapis wymaga wtedy migracji do formatu rozpoznającego ID typu/egzemplarza; obecny zapis zakłada stałą strukturę sceny.

## 6. Budowa

Zaimplementowano ShipBuildRecipe/ShipBuildCatalog i budowę ze wspólnym TryExchange zakupu. Domyślne receptury: starter 75 zł + 20 wood + 5 iron; brig 150 zł + 60 wood + 20 iron. Koszty są kopiowane i potwierdzane sygnaturą; zmiana receptury wymaga ponownego potwierdzenia. TryPlanConsumption nie zmienia starej ładowni; pojemność nowej jednostki liczymy po zużyciu materiałów. Budowa używa tej samej definicji statku i ścieżki tworzenia co zakup, ale wymaga receptury zasobów i złota. Pierwsza wersja kończy ją natychmiast i wymienia aktywną jednostkę. Czas budowy, kolejka oraz flota to dalszy zakres. Wszystkie koszty sprawdzamy przed pobraniem; operacja jest całościowa.

## 7. Menu i efekty

Edytowalne uGUI, zakładki dodawane wraz z funkcjami: Ładownia, Naprawa, Ulepszenia, Stocznia. UI prezentuje stan i wydaje polecenia do logiki, nie ustala salda ani HP samodzielnie. Podwójne kliknięcie nie może uruchomić tej samej operacji dwa razy.
Efekt kontaktu pocisku (iskry/woda) jest oddzielny od efektu faktycznych obrażeń (Damaged). Dym trwały zależy od udziału aktualnych HP w maksimum i znika po naprawie. Pozycję trafienia bierze się z systemu pocisków, ponieważ Health.Damaged przekazuje tylko liczbę obrażeń. Osobny mały etap efektów można wykonać po złocie.

## Sprawdzanie etapów

Każdy etap: analiza zależności → implementacja → kontrola kompilacji i istotnej logiki → test Play Mode użytkownika → STATUS.md. Kompilacja nie potwierdza cyklu życia Unity, wyglądu UI, fizyki ani serializacji JsonUtility.

## Łup materiałowy (etap 7 — implementacja)

ShipLootTable określa deterministyczną paczkę zasobów: domyślnie 30 wood, 10 iron, 5 gunpowder. ShipResourceReward reaguje raz na faktyczne Damaged z kadłubem na 0 HP; restore nie przyznaje nagrody. Cały łup musi się zmieścić: TryAddBatch jest atomowe, odmowa oznacza utratę paczki z komunikatem HUD, złoto działa osobno. Można wyłączyć materiałową nagrodę lub przypisać tabelę per przeciwnik. Bez skrzyń, respawnu i rozszerzenia wieloosobowego mapowania zapisu. Testy i ograniczenia w SETUP_LOOT.md.
