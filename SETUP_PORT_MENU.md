# Menu portu i naprawa — etap 3

## Zachowanie

Po zacumowaniu klawiszem E automatycznie otwiera się menu z nazwą portu, saldem złota i zakładkami Naprawa / Ładownia. Naprawa oferuje kadłub, wszystkie moduły albo cały statek. Każdy przycisk pokazuje pełny koszt. Brak złota lub brak uszkodzeń wyłącza przycisk. Nie pobieramy częściowej opłaty i nie wykonujemy częściowej naprawy.

Domyślnie 1 złoto naprawia 1 brakujący HP. Cena kadłuba i modułów jest osobna, edytowalna na PortDock: Hull Repair Gold Per Health / Module Repair Gold Per Health (co najmniej 1). Ceny są czytane z portu, w którym gracz faktycznie jest zacumowany. Moduły obejmują również zniszczone baterie, żagle, ster i inne istniejące ShipDamageModule. Naprawa przywraca ich zdrowie; nie resetuje przeładowania uzbrojenia.

Usługi nie zatrzymują czasu ani nie zapewniają ochrony. Ponowne zagrożenie/obrażenia zwalniają cumy i zamykają menu. E i przycisk „Odcumuj i wróć na morze” wracają do żeglugi. Odcumowanie blokuje wejście rozgrywki do końca bieżącej klatki, żeby kliknięcie przycisku nie stało się strzałem. Esc nadal otwiera istniejącą pauzę; portowe UI jest wtedy ukryte i wraca po wznowieniu.

Zakładka Ładownia pokazuje masę, pojemność i ilości zasobów, również anonimowy ładunek ze starych zapisów. Lista jest przewijana kółkiem myszy. Nie dodano handlu/wyrzucania towarów ani zakupu/ulepszeń statku.

## Pliki i edycja

- Framework/Economy/RepairCost: sprawdza brakujące HP i koszt z ochroną przed przepełnieniem.
- PortRepairService: sprawdza żywego gracza, faktyczne cumowanie, port, zagrożenie, pauzę, wycenę i saldo; następnie opłaca naprawę i odtwarza HP.
- PortMenuController: automatycznie podłączany do gracza, tworzy UI i pilnuje widoczności.
- PortMenuView: aktualizuje tekst i wydaje polecenia; nie ustala cen ani zdrowia samodzielnie.
- PortMenuLayout i PortMenuPrefabBuilder: identyczny edytowalny układ uGUI i generator brakującego prefabu.

Unity po imporcie tworzy Assets/ThePirate/Resources/GameWork/PortMenu.prefab. Ręcznie: Tools → GameWork → Ports → Create port menu prefab. Istniejącego prefabu generator nie nadpisuje. Widok awaryjny w runtime ma ten sam układ.

Otwórz prefab i rozwiń Window → PortPanel. Osobne obiekty: nagłówek, saldo, zakładki, RepairPanel, CargoPanel, przyciski i komunikat. Window domyślnie jest nieaktywny; włącz go w podglądzie prefabu, aby oglądać układ, i przywróć wyłączenie przed zapisem. Zachowaj referencje na PortMenuView. Istniejący HUD nie został zmieniony. Metadane nowych plików i prefab tworzy Unity.

## Test Play Mode (do wykonania)

1. Poczekaj na import/generację prefabu i sprawdź Console. Port musi być wcześniej umieszczony i zapisany w scenie zgodnie z SETUP_PORT.md.
2. Podpłyń i zacumuj E przy <=1 m/s, bez walki przez ostatnie 10 s. Menu otwiera się automatycznie. Zdrowy statek ma wyłączone przyciski naprawy i nie traci złota.
3. Zdobądź złoto przez zatopienie przeciwnika. Przy uszkodzonym kadłubie powróć do portu. Sprawdź cenę: przy 20 brakujących HP i stawce 1 koszt wynosi 20. Kliknij naprawę kadłuba: pełne HP, saldo mniejsze dokładnie o 20, moduły bez zmian.
4. Przy uszkodzonych modułach sprawdź osobną sumę ich brakujących HP i koszt. Naprawa modułów nie naprawia kadłuba. Bateria z 0 HP powinna odzyskać możliwość strzału po odcumowaniu z zachowaniem przeładowania.
5. Przy cenie większej niż saldo przycisk jest wyłączony. Kilkukrotne kliknięcie zdrowego elementu nie pobiera pieniędzy.
6. Aby przetestować pełną naprawę dużych uszkodzeń, możesz poza Play Mode dodać/ustawić ShipGoldReward przeciwnika na 1000; po teście przywróć wybraną nagrodę. To konfiguracja testowa, nie zmiana wprowadzona automatycznie.
7. Otwórz Ładownię: porównaj ilości i masę z Ship Cargo, przewiń dłuższą listę. Złoto nie zajmuje miejsca.
8. Esc: port znika pod pauzą, naprawa nie działa podczas pauzy. Po wznowieniu menu wraca. E i przycisk odcumowania zamykają je; sprawdź, czy kliknięcie nie strzela.
9. Nowe zagrożenie/obrażenia przy pomoście muszą zwolnić cumy i zamknąć usługi. Port nie zatrzymuje AI i nie zapewnia nietykalności.
10. Zapisz po naprawie w porcie, odcumuj i wczytaj: wracają HP, saldo, ładownia i cumowanie; menu otwiera się ponownie. Nie ma osobnego zapisu zakładki menu ani komunikatów.
11. Sprawdź układ na swoich rozdzielczościach, kliknięcia i widoczność kosztów. Zmień stawki portu poza Play Mode i sprawdź nową wycenę.

## Weryfikacja

Kontrolna kompilacja aktualnego runtime i Editor z bibliotekami projektu: 0 błędów (CS0649 dla pól Inspectora). 26 sprawdzeń produkcyjnego RepairCost, PortRepairService, GoldWallet, PlayerGold i Health z zastępnikami Unity przeszło: ceny, przepełnienie, brak pieniędzy, różne zakresy naprawy, zniszczone moduły, zmieniona wycena, ponowne/reentrantne kliknięcie, pauza, martwy gracz i utrata portu. Nie jest to test rzeczywistego cumowania, generacji prefabu, raycastów UI ani Play Mode/save-load.

Użytkownik potwierdził wcześniej działanie złota i ładowni. Ten etap wymaga osobnego testu. Następny krok po nim: pierwsze ulepszenia statku i dział.
