# Budowa statku ze złota i materiałów

Etap 6 ekonomii. Budowa jest natychmiastowa i zastępuje aktywny statek, tak jak zakup w stoczni. Nowa jednostka ma bazowe zdrowie i poziomy ulepszeń 0; stara znika bez zwrotu ceny. Pozostałe towary i złoto przechodzą na nowy statek. Obowiązują dostępność usług portu, brak walki/pauzy i miejsce przy pomoście.

## Pierwsze receptury

| Projekt | Złoto | Drewno `wood` | Żelazo `iron` |
| --- | ---: | ---: | ---: |
| Statek startowy | 75 | 20 szt. | 5 szt. |
| Ciężki bryg | 150 | 60 szt. | 20 szt. |

Są to startowe wartości do testów, nie docelowy balans. Zakup nadal kosztuje 200/400 zł i nie zużywa materiałów. Możesz zmieniać niezależnie cenę zakupu i recepturę budowy. Aktualnego typu nie można ponownie kupić ani zbudować.

Po imporcie Unity `ShipBuildCatalogBuilder` tworzy tylko brakujące assety:

- `Assets/ThePirate/Resources/GameWork/ShipBuildCatalog.asset`: lista receptur, maksymalnie 32, jedna na typ statku.
- `starter-build.asset` i `brig-build.asset` w tym samym folderze: `ShipBuildRecipe`.

Menu **Tools → GameWork → Economy → Ship build recipes** tworzy brakujący katalog albo zaznacza istniejący. Istniejących assetów nie nadpisuje. Nie zmienia projektów, modeli statków ani oryginalnego prefabu gracza.

## Edycja w Inspectorze

Wybierz asset receptury:

1. **Ship Type Id** musi odpowiadać stałemu ID w `ShipDefinition`, np. `brig`.
2. **Gold Cost** jest nieujemnym kosztem. Zero jest dozwolone przy budowie za same materiały.
3. **Materials** zawiera ID zasobu i dodatnią ilość w sztukach. Przykładowo `wood`, `60` oznacza 60 sztuk drewna, nie 60 kg.
4. ID muszą istnieć w katalogu zasobów. Każdy materiał wpisuj raz. Pusta receptura, powtórzone ID, ujemne/zerowe ilości są odrzucane.

Inspector pod danymi pokazuje błędy receptury, brak projektu lub nieznane zasoby. Zasoby tworzysz dotychczasowym edytorem **Tools → GameWork → Economy → Resource catalog**, a nowe projekty przez `ShipDefinition`. Dla kolejnego projektu utwórz **Create → GameWork → Ships → Build Recipe**, ustaw koszty i dodaj asset do listy `ShipBuildCatalog`.

## Stocznia w grze

W zakładce **Stocznia** masz **Kup i wymień** oraz **Zbuduj**. Lista materiałów pod parametrami statku pokazuje koszt złota i ilości **posiadasz / potrzeba**. Można ją przewijać kółkiem. Brak materiałów, złota lub miejsca blokuje budowę z podaniem powodu.

Kliknięcie **Zbuduj** wymaga potwierdzenia utraty poprzedniego statku i jego ulepszeń. Anulowanie, zmiana wyboru, zmiana kosztów lub utrata dostępności usługi zamyka potwierdzenie. Przy zatwierdzeniu koszt jest sprawdzany ponownie. Zmiana receptury w Inspectorze podczas gry wymaga potwierdzenia nowego kosztu; nie obciąża gracza wcześniej zaakceptowaną ofertą.

Menu pozostaje edytowalnym uGUI. `PortShipBuildLayout` dodaje przycisk i przewijaną listę do istniejącego `ShipyardPanel`; dzieli dotychczasowy obszar zakupu na dwa przyciski. `PortMenuPrefabBuilder` rozszerza istniejący prefab z zachowaniem GUID i pozostałych referencji. Runtime fallback korzysta z tego samego układu. Powtórny import nie powinien tworzyć kolejnego przycisku.

## Logika transakcji

`CargoInventory.TryPlanConsumption` w Framework przygotowuje niezależną listę ładunku po odjęciu wymaganych materiałów. Sprawdza wszystkie ID, ilości i docelową pojemność przed zwróceniem planu. Nie zmienia starej ładowni ani nie emituje `Changed`.

Pojemność nowego statku jest liczona **po zużyciu materiałów**. Przykład: ładunek 1100 kg może nie pozwolić na zakup statku z ładownią 1000 kg. Jeżeli budowa zużywa 200 kg materiałów, pozostające 900 kg mieści się w nowej jednostce i budowa może być dostępna. Anonimowa masa ze starego zapisu nie jest materiałem budowy, nadal zajmuje miejsce.

`ShipBuildRecipe.TrySnapshot` kopiuje koszty i tworzy sygnaturę typu statku, złota oraz materiałów. `ShipBuildQuote` nie udostępnia wewnętrznych tablic do modyfikacji. `ShipBuildCatalog` odrzuca błędne/powtórzone receptury.

`ShipyardService.TryBuild` sprawdza ofertę, sygnaturę potwierdzenia, złoto oraz materiałowy plan. Budowa i zakup korzystają ze wspólnego `TryExchange` i blokady operacji. Nowy egzemplarz otrzymuje pozostały ładunek i saldo pomniejszone o koszt; stary nie jest obciążany. Dopiero udane przejęcie pomostu i przypięcie widoku usuwa stary obiekt. Przy błędzie nowy egzemplarz jest usuwany, a stary zachowuje złoto i wszystkie towary.

Nie dodano kolejki ani czasu budowy. Zapis nie wymaga nowych pól: ukończony statek, jego typ/GUID, złoto i pozostałe zasoby zapisują istniejące mechanizmy. Nie zapisujemy tymczasowego potwierdzenia w menu.

## Weryfikacja

Kontrolna kompilacja całego runtime/Editor z bibliotekami Unity: 0 błędów, ostrzeżenia CS0649 pól Inspectora. Przeszły **84 sprawdzenia**: 44 regresji stoczni/zapisu i 40 nowych sprawdzeń receptur, planowania materiałów oraz budowy. Testy używają produkcyjnego CargoInventory, receptur/katalogu, ShipyardService, portfela i wyodrębnionego przepływu save z zastępnikami Unity. Testowa fabryka nie potwierdza rzeczywistego tworzenia prefabów, fizyki, uGUI ani JsonUtility.

Nie uruchomiono edytora ani Play Mode w tym etapie. Nie dopisano potwierdzenia działania wcześniejszych menu/ulepszeń/stoczni; użytkownik polecił jedynie kontynuować.

## Test w Unity

Na razie zdobywanie surowców podczas walki nie jest zaimplementowane. Do testu budowy użyj istniejących narzędzi `ShipCargoInspector`; kolejny etap doda surowce jako łup.

1. Poczekaj na import, sprawdź Console, assety receptur i nową sekcję `PortMenu.prefab`.
2. Uruchom ShipSandbox, zdobądź 150 zł lub więcej i w komponencie `ShipCargo` gracza wybierz **Test: załaduj**: 60 drewna oraz 20 żelaza. Nagrodę złota przeciwnika można zwiększyć przed testem; potem przywróć normalny balans.
3. Zacumuj, wybierz bryg i przewiń listę kosztów. Anuluj budowę: statek/złoto/towary powinny pozostać bez zmian.
4. Potwierdź: bryg ma 750 bazowych HP, poziomy ulepszeń 0, saldo mniejsze o 150, a drewno i żelazo pomniejszone o 60/20. Nadmiar materiałów i inne towary pozostają. Sprawdź kamerę, HUD, ruch i działa po odcumowaniu.
5. Powtórz z brakiem jednej sztuki materiału, brakiem złota oraz blokadą miejsca przy pomoście. Odmowa ma zachować wszystkie zasoby i stary statek.
6. Sprawdź budowę mniejszej jednostki, gdy materiały obniżają masę poniżej jej pojemności; zwykły zakup przy tym samym początkowym ładunku ma być blokowany. Nadmiar masy także po zużyciu nadal blokuje budowę.
7. Otwórz potwierdzenie, zmień Gold Cost lub ilość materiału w assetcie. Stare potwierdzenie ma zostać anulowane albo odrzucone przez usługę. Nowy podgląd powinien pokazywać aktualne koszty.
8. Sprawdź szybkie klikanie budowy/zakupu, pauzę, zagrożenie i zmianę zakładki. Zapisz ukończony statek i wczytaj z menu: właściwy typ, pozostałe zasoby oraz złoto.
9. Ponowny import/menu generatora ma zachować własne koszty i nie tworzyć duplikatów UI. Sprawdź przewijanie i przyciski w różnych rozdzielczościach.

Następny etap: pozyskiwanie zasobów z pokonanych statków, według edytowalnych nagród.
