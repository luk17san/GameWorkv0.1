# Katalog zasobów i ładownia — etap 2

## Co powstało

ResourceDefinition i ResourceCatalog (Framework) są edytowalnymi assetami Unity. CargoInventory przechowuje ilości, sprawdza pojemność i umożliwia atomowe dodanie/usunięcie/odtworzenie zawartości. ShipCargo (ThePirate) automatycznie pojawia się na graczu i aktualizuje masę ShipMovementController, korzystając z jego CargoCapacity. Dotychczasowy HUD masy i kary ruchu korzystają z tej samej wartości.

Złoto pozostaje osobną walutą. Etap nie dodaje jeszcze łupów surowcowych, handlu, zużywania prochu przez działa ani menu ładowni w grze. Przyciski Inspectora służą testowaniu. Złoto użytkownik potwierdził jako działające; nie oznacza to potwierdzenia wszystkich scenariuszy save/load.

## Edytor zasobów

Po imporcie Unity tworzy brakujący katalog:
Assets/ThePirate/Resources/GameWork/ResourceCatalog.asset
oraz definicje: wood (Drewno, 2 kg, cena 5), iron (Żelazo, 5 kg, cena 12), gunpowder (Proch, 1 kg, cena 20). Ceny są danymi pod późniejszy handel, nie aktywnym cennikiem sklepu. Istniejące assety nie są nadpisywane. Metadane generuje Unity.

Otwórz Tools → GameWork → Economy → Resource catalog. Wpisz nazwę i kliknij „Dodaj zasób do katalogu”. Nowy zasób otrzymuje unikalne ID. Wybierz go przyciskiem na liście i ustaw nazwę, opis, ikonę, masę, cenę oraz kategorię. „Sprawdź katalog” wykrywa brakujące wpisy, duplikaty ID i błędne wartości.

ID dopuszcza 1–64 znaków a-z, 0-9, _ i -. Nie zmieniaj ID zasobu używanego w zapisach. Nazwę można zmieniać. Nieznane ID albo zmieniona masa zajętego zasobu powodują odmowę zastosowania starego zapisu; nie kasujemy po cichu zawartości. Wtedy przywróć poprzednią definicję lub rozpocznij nową grę. Docelowa migracja zapisów będzie oddzielnym etapem.

Ładownia kopiuje masy podczas inicjalizacji. Edytuj katalog poza Play Mode, potem uruchom nową sesję. Katalog jest wybierany wspólnie przez ścieżkę Resources/GameWork/ResourceCatalog; nie dodawaj drugiego assetu pod identyczną ścieżką Resources.

## Test w Unity (do wykonania)

1. Zaczekaj na import i sprawdź Console. Otwórz katalog, potwierdź trzy zasoby i poprawną walidację.
2. Uruchom nową grę, zaznacz PlayerShip → Ship Cargo. Ładownia pusta, masa 0 / pojemność z ShipCargoStats (obecnie 1000 kg).
3. Wybierz Drewno, ilość 10 → „Test: załaduj”. Ilość 10, masa 20 kg. Sprawdź Current Cargo Weight w ruchu i HUD.
4. Załaduj Żelazo, ilość 10. Łącznie 70 kg. Wyładuj 5 drewna: 60 kg. Próba wyładowania większej ilości nie zmienia stanu.
5. Próba załadowania 1000 drewna musi pokazać brak miejsca i nie zmienić żadnej ilości. Ilość 0 lub ujemna także jest odrzucana.
6. Załaduj statek do około 80% pojemności i porównaj prędkość/przyspieszenie z pustą ładownią. Obowiązują dotychczasowe parametry ShipCargoStats; ich wartości nie zostały zmienione.
7. Zapisz przy kilku zasobach, zmień zawartość, wczytaj. Ilości, masa i złoto wracają do zapisu. Powtórz w porcie i po zatopieniu przeciwnika. Nowa gra ma pustą ładownię.
8. Wczytaj starszy zapis z samym cargoWeight. Masa pojawi się jako „Stary ładunek bez zawartości”; zachowuje wpływ na ruch i limit. Przycisk testowy umożliwia wyzerowanie tej masy, nie zgaduje rodzaju zasobu.
9. Poza Play Mode dodaj własny zasób przez katalog. Uruchom grę, załaduj go, zapisz i wczytaj. Zmień tylko jego nazwę — zapis nadal rozpoznaje ID.
10. W razie potrzeby na kopii testowej sprawdź duplikat ID i pusty wpis katalogu: walidacja musi odmówić utworzenia ładowni zamiast wybrać przypadkową definicję.

## Kod i zapis

CargoInventory.TryAdd/TryRemove zwracają false z komunikatem przy błędzie; nie przyjmują części ilości. Snapshot zwraca kopię do UI/zapisu. Changed aktualizuje masę ruchu. Maksymalnie 1024 rodzaje zasobów, ilości dodatnie int, masa dodatnia i skończona.
Opcjonalne pola ShipSaveData v1: hasCargoInventory, cargo[], legacyCargoWeight. cargoWeight pozostaje sumą masy. Stary zapis bez pól jest mapowany na pustą listę i zachowaną masę anonimową. Wszystkie ID, ilości, masy i pojemność są sprawdzane przed zastosowaniem złota/zdrowia/ruchu z zapisu. Brak ShipCargoStats albo poprawnego katalogu uniemożliwia zapis/odtworzenie ładowni.

Kontrolna kompilacja runtime/Editor przeszła bez błędów. 34 sprawdzenia produkcyjnej logiki katalogu/ładowni i wyodrębnionej walidacji zapisu przeszły poza Unity z zastępnikami API. Test roundtrip DTO użył System.Text.Json, więc nie potwierdza JsonUtility. Import, generacja assetów, Inspector, wpływ na rzeczywisty ruch i save/load w Play Mode czekają na test w Unity.

Następny etap po sprawdzeniu: proste menu portu i naprawa za złoto. Dodawanie surowców jako łupów można dołączyć później do tej ładowni.
