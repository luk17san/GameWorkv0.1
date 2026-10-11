# Ulepszenia statku i baterii — etap 4

## Pierwszy poziom

W menu portu jest nowa zakładka Ulepszenia. Każda opcja pokazuje efekt oraz cenę przed zakupem:

| Ulepszenie | Domyślny efekt | Cena |
| --- | --- | --- |
| Kadłub | +100 maksymalnego HP | 100 złota |
| Napęd | ×1.10 prędkości maksymalnej, ×1.15 przyspieszenia | 150 złota |
| Baterie dział | +5 obrażeń na każdy pocisk, ×0.90 czasu kolejnych przeładowań | 200 złota |

Po jednym poziomie każdej opcji. Baterie są ulepszane wspólnie: lewa/prawa burta, dziób/rufa i inne WeaponBattery tego statku. Moździerz oraz broń załogi nie należą do tej grupy. Baterie zachowują sektory, zasięg, liczbę luf i kary uszkodzeń. Ulepszenie zniszczonej baterii nie naprawia jej; do tego służy Naprawa.

Kadłub zachowuje liczbę brakujących HP: np. 450/500 po ulepszeniu ma 550/600, nadal wymaga naprawy 50 HP. Nie wskrzeszamy martwego statku. Zakup napędu zachowuje istniejące kary masy i uszkodzeń, proporcje cofania oraz dotychczasową fizykę. Ulepszenie dział nie zmienia trwającego nextFireTime/reloadDuration: krótszy czas dotyczy kolejnej salwy.

Zakup wymaga żywego gracza, poprawnego zacumowania i braku walki/pauzy. Przycisk kupionego poziomu jest wyłączony. Brak złota, błędna definicja lub zmieniona cena odrzucają zakup przed pobraniem opłaty. Ponowna próba zakupu nie pobiera pieniędzy.

## Edycja danych i UI

Po imporcie Unity tworzy tylko brakujący asset:
Assets/ThePirate/Resources/GameWork/ShipUpgradeRules.asset

Tools → GameWork → Economy → Ship upgrade rules zaznacza go w Project. Inspector pozwala ustawić trzy ceny, bonus HP, bonus obrażeń i mnożniki napędu/przeładowania. Zmiany testuj w nowej sesji Play Mode — uruchomiony statek używa kopii parametrów. Nie zmieniamy wspólnych assetów ruchu ani bazowych wartości prefabów przy zakupie, więc przeciwnicy pozostają przy swoich parametrach.

Generator menu rozszerza istniejący PortMenu.prefab wyłącznie o UpgradeTab i UpgradePanel oraz ich referencje; zachowuje poprzednie obiekty i GUID. Do ręcznego uruchomienia: Tools → GameWork → Ports → Create port menu prefab. Pierwotny prefab został skopiowany do katalogu backup-* etapu przed instalacją. Sekcję można edytować w prefabie: Window → PortPanel → UpgradePanel. Runtime ma taki sam układ awaryjny, jeżeli nie został jeszcze wygenerowany/rozszerzony prefab.

Jeżeli samodzielnie zmieniono nazwy Window/PortPanel albo usunięto referencje nowej sekcji, generator zgłosi błąd zamiast zastępować istniejący układ. Zachowaj te obiekty oraz odnośniki PortMenuView/PortUpgradePanel.

## Zapis

Opcjonalny ShipSaveData.upgrades w formacie v1 zawiera hullLevel/driveLevel/batteryLevel i sygnaturę efektów. Stare zapisy bez tego pola oznaczają poziomy 0. Wczytanie sprawdza poziomy, dostępność komponentów, wynikowe maksymalne HP i zgodność efektów przed zmianą statków. Następnie odtwarza parametry od bazowych wartości i dopiero zapisane zdrowie/ruch/przeładowania. Wielokrotne wczytanie nie mnoży bonusów.

Zmiana samych cen jest zgodna ze starym zapisem. Zmiana efektów po kupieniu któregokolwiek ulepszenia powoduje odmowę zastosowania tego zapisu; przywróć poprzednie efekty albo rozpocznij nową grę. Migracja kupionych poziomów do nowych zasad pozostaje osobnym zadaniem. Nie zapisujemy gotowych mnożników jako kolejnych modyfikacji do dodania.

## Test Play Mode (do wykonania)

Menu portu z etapu 3 nie otrzymało jeszcze osobnego potwierdzenia użytkownika; sprawdź też SETUP_PORT_MENU.md.

1. Zaczekaj na import, utworzenie ShipUpgradeRules.asset i rozszerzenie PortMenu.prefab. Sprawdź Console.
2. Zdobądź złoto i zacumuj E. Otwórz Ulepszenia: powinny być trzy podglądy i ceny. Brak złota wyłącza przyciski.
3. Domyślna nagroda 100 pozwala kupić kadłub. Aby przetestować wszystkie zakupy w jednej rozgrywce, poza Play Mode możesz ustawić nagrodę przeciwnika ShipGoldReward na 1000; po teście przywróć wybraną wartość. Nie zmieniono nagrody automatycznie.
4. Kup kadłub przy uszkodzeniach. Maksymalne HP wzrasta o 100, liczba brakujących HP zostaje taka sama, saldo spada o 100. HUD i wycena naprawy powinny odpowiadać nowemu stanowi.
5. Powtórne kliknięcie/próba zakupu kadłuba nie pobiera pieniędzy. Poziom pokazuje „kupiony”.
6. Kup napęd. Po odcumowaniu sprawdź nową prędkość/przyspieszenie, także z obciążoną ładownią i uszkodzonymi żaglami. Bazowe ShipMovementStats pozostają zachowane.
7. Kup baterie. Działa 25 obrażeń powinny emitować pociski z 30; przy sprawnym module następna salwa ma reload 6 × 0.9 = 5.4 s. Dotychczasowe mnożniki uszkodzeń 1.5/2.5 nadal obowiązują. Zniszczona bateria wymaga naprawy.
8. Sprawdź, że czas rozpoczętego przeładowania nie zeruje się przy zakupie. Cumowanie nadal wymaga 10 s bez walki; dla dłuższego cooldownu dopasuj parametry testowe przed uruchomieniem.
9. Zapisz po zakupach, odcumuj, wczytaj kilka razy. Poziomy, HP, złoto, ładownia i przeładowania wracają; bonusy nie rosną po każdym wczytaniu. Wczytaj też zapis sprzed zakupu — poziomy i saldo wracają do wcześniejszej historii. Nowa gra zaczyna od poziomów 0.
10. Sprawdź pauzę, odcumowanie i zagrożenie: poza usługami zakup jest niemożliwy. Oceń zakładkę na używanych rozdzielczościach.
11. Na kopii testowej sprawdź starszy zapis bez upgrades, zmianę ceny i zmianę efektu kupionego poziomu — zgodność/odmowa powinny odpowiadać opisowi powyżej.

## Weryfikacja

Kontrolna kompilacja aktualnego runtime i Editor: 0 błędów, ostrzeżenia CS0649 pól Inspectora. 40 sprawdzeń produkcyjnego ShipUpgrades/ShipUpgradeRules/Health/portfela oraz wyodrębnionych metod parametrów ruchu/baterii i walidacji zapisu przeszło z zastępnikami Unity. Sprawdzono limity, opłaty, podwójny/reentrantny zakup, zachowanie uszkodzeń i cooldownu, brak nakładania bonusów, zgodność zapisów i składanie kar ruchu. Roundtrip DTO użył System.Text.Json, nie Unity JsonUtility. Nie uruchomiono Unity, generacji/edycji prefabu, rzeczywistego ostrzału/fizyki ani pełnego save/load.

Następny etap: projekty statków i zakup/wymiana aktywnej jednostki. Będzie wymagał rozszerzenia zapisu, który obecnie mapuje stałe statki sceny.
