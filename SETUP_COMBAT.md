# System walki statków

## Wdrożenie

Kod znajduje się w `Assets/ThePirate/Scripts/Combat`. Framework.Health pozostaje uniwersalnym zdrowiem. Generator `GameWork > Combat > Configure combat prefabs` konfiguruje istniejący PlayerShip i tworzy CombatTestEnemy. Ponowne wywołanie zachowuje istniejący CombatRig i prefab przeciwnika. Generator pracuje na prefabach, nie zapisuje otwartej sceny.

PlayerShip ma kadłub 500 HP, masę 1000, cztery baterie (burty po 4 emitery, dziób i rufa po 1), moździerz oraz strefy uszkodzeń żagli i załogi. Stary TestCannon pozostaje nieaktywny. Rozmiary collidera i pozycje emiterów są wyznaczane z modelu; należy ocenić je wizualnie dla tego modelu i poprawić w Inspectorze. Obiekty pod CombatRig są niezależnie edytowalne.

## Sterowanie i ustawienia

- LPM: salwa jednej najlepiej skierowanej baterii obejmującej punkt kursora. Celowanie działa na płaszczyźnie wody Y=0. Cel poza sektorem nie uruchamia strzału.
- PPM: włączenie/anulowanie moździerza. LPM zatwierdza prawidłowy cel; Escape anuluje celowanie (istniejące menu może też włączyć pauzę).
- WeaponBattery: wspólne zakresy 8–80 m, sektor 70°, przeładowanie 6 s, obrażenia 25 na pocisk, rozrzut 0.5°. Nie ma przeładowania pojedynczych dział.
- Salwa: osobne losowe opóźnienia 0.001–0.01 s od początku; dokładność zależy od klatki Unity. Pociski mają wspólny kierunek bazowy, nie zbiegają się w punkcie kursora.
- ShipDamageModule: zdrowie punktu montażowego; przy ≤50% przeładowanie i rozrzut ×1.5, przy ≤25% ×2.5, przy 0% brak strzału. Repair przywraca zdrowie. Żagle ograniczają prędkość i przyspieszenie przez istniejące SetCondition; na zniszczonych żaglach pozostaje 15% napędu. ShipConditionController jest źródłem mnożników uszkodzeń ruchu.
- ShipAutoFire / Automatic Fire: domyślnie wyłączony dla gracza, włączony dla przeciwnika. Team 0 to neutralni; różne niezerowe zespoły są wrogie. System używa tych samych baterii i wspólnego przeładowania. Sprawdza przeszkody ponownie przed emisją.
- CrewAutoFireSystem: niezależny ogień natychmiastowy do 18 m, 4 obrażenia co 2 s, celuje w załogę albo lekki kadłub. Zniszczona załoga nie strzela.
- Moździerz: 15–100 m, czas lotu 3 s, promień 6 m, 50 obrażeń, przeładowanie 12 s. Fizyczny lot z grawitacją; detonacja przy kolizji lub zejściu do poziomu wody Y=0. Obrażenia obszarowe liczone raz na moduł/odbiorcę.
- FireRangeVisualizer: kontury wspólnego sektora z obu promieni; biały dostępny, szary niesprawny/przeładowywany, zielony prawidłowy, czerwony zablokowany. W trybie specjalnym widoczny okrąg trafienia.
- ShipCollisionDamage: bezpieczna prędkość normalna 2 m/s, masa zredukowana obu ciał, kwadrat prędkości powyżej progu, mnożniki dziobu/burty/rufy, odporność kadłuba. Pierwszy callback rozlicza obu uczestników i blokuje parę na 0.75 s. Brak obrażeń za samo pozostawanie w kontakcie (nie używa OnCollisionStay). Pociski nie uruchamiają obrażeń taranowania.

Ręczny strzał może trafić przeszkodę lub sojusznika. Własne collidery blokują emisję, a pocisk po wystrzeleniu ignoruje własny statek. Automatyczny ostrzał odrzuca przeszkody, neutralnych i sojuszników. Ruch jednostek już po strzale może nadal doprowadzić do przypadkowego trafienia. Pole Impact Effect pocisku pozwala przypisać efekt; pakiet nie dodaje nowych dźwięków ani efektów graficznych.

## Test w Unity

1. Po imporcie sprawdź Console i `Temp/GameWorkCombatSetup.result`. Generator kontroluje referencje i 40 przypadków geometrii/zdrowia w Edit Mode. To nie jest test fizyki ani rozgrywki.
2. Otwórz ShipSandbox. Sprawdź, że instancja PlayerShip dziedziczy CombatRig; lokalne overrides sceny mogą wymagać osobnej korekty. Nie używaj bez sprawdzenia Revert All.
3. Przeciągnij CombatTestEnemy na scenę około 30 m obok gracza, na tej samej wysokości; jest to nieruchomy przeciwnik testowy, bez AI nawigacji. Ustaw burtą do gracza.
4. Play: LPM w lewo/prawo/przód/tył, poza zasięgiem i w lukę sektorów. Sprawdź liczbę pocisków, brak krzyżowania i blokadę przeładowania.
5. Zasłoń lufę własnym colliderem. Postaw skałę i sojusznika na linii ognia; ręczny atak powinien w nie trafiać, automatyczny nie powinien strzelać przez nie.
6. Trafiaj w baterię/żagle/załogę. Sprawdź Current Health modułów, spowolnienie statku, zatrzymanie zniszczonej baterii i niezależność broni załogi.
7. PPM → przesuwanie okręgu → LPM, następnie PPM/Escape do anulowania. Sprawdź trafienie moździerza w wodę i kadłub oraz pojedyncze naliczenie obrażeń.
8. Pauza, kliknięcie menu, wznowienie: brak przypadkowych strzałów; czas przeładowania zatrzymany.
9. Kolizje przy małej/dużej prędkości, dziób w burtę, burta w burtę, skała; pozostaw statki w kontakcie i rozdziel. Sprawdź brak wielokrotnych obrażeń jednej pary.

## Granice bieżącego wdrożenia

Brak potwierdzonego testu Play Mode. Konfiguracja wartości i stref jest prototypowa. Maszt, ster i magazyn mają typy modułów do rozszerzeń; automatyczna konfiguracja tworzy kadłub, żagle, baterie i załogę. Nie dodano pożarów, przecieków, eksplozji magazynu, animacji zatonięcia, UI napraw ani nawigacji przeciwnika. Kontury sektorów nie mają wypełnienia/poświaty. HUD zdrowia i wyniku pojedynku pozostaje osobnym etapem.


## Korekta zderzeń — 2026-10-02

ShipCollisionDamage zachowuje próg 2 m/s i mnożniki stref. Impact Damage Multiplier = 0.1 zmniejsza wcześniejsze obrażenia dziesięciokrotnie. Max Impact Health Fraction = 0.25 ogranicza każde uderzenie do 25% maksymalnego HP odbiorcy (125 dla kadłuba 500 HP); statek z mniejszą liczbą HP może zatonąć. Pola są edytowalne w Inspectorze. Obrażenia naliczane wyłącznie na początku kontaktu; wspólny cooldown pary nadal obowiązuje.

Ruch uwzględnia prędkość Rigidbody po fizyce przy Enter/Stay. Napęd nie dodaje prędkości skierowanej w powierzchnię kontaktu. Gracz traci także zapamiętaną prędkość zadaną w dotychczasowym kierunku; W pozwala ponownie rozpędzać się. AI nadal korzysta z poleceń nawigacji. Otarcie zachowuje prędkość wzdłuż przeszkody. Pociski i zatopione kadłuby są pomijane.

Test Play Mode w świeżym ShipSandbox:
1. Wyłącz ostrzał na czas pomiaru. Czołowe zderzenie: każdy kadłub traci najwyżej 125 HP od jednego uderzenia; prędkość wyraźnie spada.
2. Puść W przed uderzeniem: po rozdzieleniu statków dawna prędkość nie wraca samoczynnie. W ponownie rozpędza statek.
3. Przytrzymaj W przy nieruchomej przeszkodzie: brak ciągłego taranowania z pełną prędkością i obrażeń za sam kontakt. Po odpłynięciu i nowym uderzeniu obrażenia mogą wystąpić ponownie.
4. Otarcie burtą: pozostaje ruch wzdłuż przeszkody. Sprawdź też cofanie i uderzenie rufą.
5. Niska prędkość normalna (do 2 m/s): brak obrażeń. Uszkodzony kadłub poniżej limitu może zostać zatopiony. Sprawdź pauzę i wczytanie po kolizji.

Kompilacja i kontrola logiki poza Unity nie potwierdzają fizyki ani zachowania w Play Mode.