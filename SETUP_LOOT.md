# Łup zasobów po zatopieniu przeciwnika

Etap 7 ekonomii. Materiały trafiają bezpośrednio do ładowni jedynego żywego gracza w tej samej scenie. Dotychczasowa nagroda złota działa niezależnie. Nie dodano pływających skrzyń ani losowania nagród.

## Pierwsza tabela łupu

Po imporcie Unity generator tworzy tylko brakujący `Assets/ThePirate/Resources/GameWork/ShipLootTable.asset`:

| Zasób | Stałe ID | Ilość |
| --- | --- | ---: |
| Drewno | `wood` | 30 |
| Żelazo | `iron` | 10 |
| Proch | `gunpowder` | 5 |

Przy domyślnych masach katalogu paczka waży 115 kg. To wartości startowe do testu, nie ostateczny balans. Receptura brygu potrzebuje 60 drewna, 20 żelaza i 150 zł; standardowe dwa takie zwycięstwa dostarczają materiały oraz 200 zł. Obecna scena/zapis obsługują jednak jednego przeciwnika: nie dodano respawnu, kolejnych spotkań ani zapisu wielu przeciwników w tym etapie. Do testu pełnej budowy w jednym pojedynku skonfiguruj większą nagrodę testową według instrukcji poniżej.

Otwórz **Tools → GameWork → Economy → Ship loot table**, aby utworzyć brakujący asset lub zaznaczyć istniejący. Generator nie nadpisuje własnych ilości, istniejących assetów ani prefabów. Nowe .meta i asset tworzy Unity.

## Edycja nagród

W Inspectorze `ShipLootTable` lista **Rewards** zawiera pary Resource Id / Quantity. Ilości oznaczają sztuki, nie kilogramy. Dodaj nowe zasoby przez dotychczasowy edytor **Resource catalog**, następnie wpisz ich ID w tabeli. ID muszą istnieć w katalogu i wystąpić raz. Ilości muszą być dodatnie; pusta lista oznacza brak materiałów.

Inspector sprawdza ID/ilości/katalog, pokazuje nieznane zasoby i masę całej paczki. Zmiana w tabeli wpływa na następne zatopienie korzystające z tego assetu. To deterministyczna nagroda, bez szansy wypadnięcia.

`CombatShip.Awake` dodaje `ShipResourceReward` do statków wrogich (Team różny od 0 i 1). Na przeciwniku:

- **Reward Enabled** wyłącza nagrodę materiałową; nie wyłącza złota.
- **Loot Table** może wskazywać osobny asset dla konkretnego przeciwnika. Puste pole używa domyślnego `GameWork/ShipLootTable`.

Aby ustawić te dane trwale, dodaj komponent `ShipResourceReward` do przeciwnika/prefabu poza Play Mode. Automatyczny komponent dodany podczas gry znika po zatrzymaniu Play. Osobną tabelę utworzysz przez **Create → GameWork → Economy → Ship Loot Table**. Prefabów/sceny nie zmieniono automatycznie.

## Przyznawanie i brak miejsca

Nagroda reaguje na `Health.Damaged` dopiero wtedy, gdy faktyczne obrażenia obniżyły kadłub do 0. Nie przyznaje materiałów za uszkodzenie modułu, nietrafiony pocisk, zwykłe naprawy ani wczytanie zdrowia. Źródło obrażeń może być działem, moździerzem lub zderzeniem; obecny Health nie wskazuje autora trafienia, więc odbiorcą jest jedyny żywy gracz sceny.

`CargoInventory.TryAddBatch` w Framework sprawdza całą paczkę przed zmianą zawartości: ID, ilości, przepełnienie licznika i masę. Wszystkie materiały trafiają razem lub żaden. Udane przyznanie emituje pojedyncze `Changed`; istniejący ShipCargo aktualizuje masę ruchu i HUD/ładownia odczytują nowe ilości.

Przy braku miejsca **cały łup materiałowy przepada**, a HUD i Console pokazują powód. Nie ma częściowego załadunku, skrzyni do późniejszego zebrania ani ponownej próby po wyładowaniu towarów. Złoto nadal może zostać przyznane. Błędna tabela, nieznany zasób lub niepoprawna ładownia również odrzucają materiały bez częściowej zmiany. Pustej tabeli nie towarzyszy komunikat.

Komponent oznacza obsłużone zatopienie przed przyznaniem nagrody. Ponowne obrażenia, wyłączenie/włączenie czy ręczna naprawa tego samego egzemplarza nie dają drugiego łupu. Brak odbiorcy, martwy gracz lub kilku żywych graczy powodują odmowę. Nowy egzemplarz wroga ma nową możliwość przyznania nagrody.

## Zapis gry

Nie dodano nowych pól zapisu. Przyznane materiały zapisuje istniejąca ładownia, złoto istniejący portfel. `RestoreCurrentHealth` nie emituje `Damaged`: wczytanie pokonanego wroga nie przyznaje nagrody ponownie. Wczytanie zapisu sprzed bitwy odtwarza także poprzednie złoto i zasoby; następne zwycięstwo należy do odtworzonej rozgrywki.

Nie zapisujemy oddzielnych skrzyń ani nieodebranego łupu, bo w tej wersji nie istnieją. Nie rozszerzono ograniczenia zapisu do wielu przeciwników.

## Weryfikacja i test Unity

Kontrolna kompilacja runtime/Editor z bibliotekami Unity: **0 błędów**, ostrzeżenia CS0649 danych Inspectora. **38 sprawdzeń** produkcyjnego CargoInventory, ShipLootTable, ShipResourceReward, Health, GoldWallet i nagrody złota przeszło z zastępnikami Unity/HUD. Sprawdzono atomowość, brak miejsca, licznik int, błędne ID, masę legacy, komunikaty, override tabeli, wyłączenie, ponowne zatopienie i brak łupu przy RestoreCurrentHealth(0).

Nie uruchomiono edytora ani Play Mode w tym etapie. Testy nie potwierdzają importu assetu, prawdziwego HUD, synchronizacji masy w fizyce ani pełnego JsonUtility/save/load. Poprzednie etapy menu/ulepszeń/stoczni/budowy nadal nie mają osobnego potwierdzenia Play Mode.

1. Poczekaj na import, sprawdź Console i utworzenie ShipLootTable.asset. Uruchom świeżą scenę ShipSandbox. Zatop przeciwnika: oprócz 100 zł powinno pojawić się 30 drewna, 10 żelaza i 5 prochu; HUD pokaże listę materiałów. Otwórz portową ładownię i sprawdź ilości oraz dodatkowe 115 kg.
2. W nowej rozgrywce przed zatopieniem zapełnij ładownię narzędziami `ShipCargoInspector`, pozostawiając mniej niż 115 kg. Zatop wroga: złoto ma się pojawić, materiały nie; komunikat wyjaśnia odmowę. Dotychczasowe towary pozostają.
3. Utwórz inną tabelę (np. 1 drewno) i przypisz ją przeciwnikowi. Sprawdź także Reward Enabled = false, pustą tabelę, błędny ID i brak assetu. Przy testowaniu poza Play dodaj komponent do przeciwnika i zapisz scenę/prefab świadomie.
4. Zapisz grę przed walką, podczas zatapiania i po usunięciu wroga. Wczytaj: stan zasobów/salda ma odpowiadać zapisowi; odtwarzanie zatopionego wroga nie dodaje kolejnej nagrody. Powtórne trafienia kadłuba na 0 HP również nie zmieniają zasobów.
5. Do próby **łup → budowa brygu** w jednym pojedynku tymczasowo ustaw tabelę na 60 drewna i 20 żelaza oraz `ShipGoldReward` przeciwnika na co najmniej 150 zł. Możesz zmienić pole komponentu podczas Play przed zwycięstwem; jeśli komponentu nie ma poza Play, dodaj go ręcznie do testu. Zatop, wróć do portu, zbuduj bryg i sprawdź zużycie materiałów. Przywróć zwykłe wartości po teście.
6. Sprawdź rzeczywiste spowolnienie od masy towarów, HUD w różnych rozdzielczościach i komunikat podczas zatapiania. Nie dodano nietykalności, zmian w broni, respawnu ani automatycznego restartu gospodarki.

Następny etap z pierwotnej listy: wizualny efekt faktycznych obrażeń statku.
