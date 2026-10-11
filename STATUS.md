# PortVisualKit — 2026-10-05

- Przygotowano 7 PNG: tło, rama, 2 stany przycisków, pergamin, atlas 12 ikon i portret statku. Wbudowane imagegen; prompty w PROMPTS.md.
- Osobny PortVisualKitBuilder generuje 12 wzorców obiektów oraz kompletny PortVisualCanvas. Prawdziwe komponenty uGUI, osobne teksty i paski Slider, 9-slice przycisków/pergaminu, import ikon jako osobne Sprite subassets.
- Generator tworzy nowy Build-XX, zachowuje starsze wersje, nie zapisuje scen i nie zmienia istniejącego PortMenu.prefab. Domyślny font Unity; opcjonalna własna PortSerif.ttf.
- Przeczytano aktualne AGENTS.md, PLAN.md, STATUS.md, PortMenuLayout i PortMenuPrefabBuilder oraz ścieżkę PortMenuController. W projekcie istnieje już logika napraw, ładowni, ulepszeń i stoczni; zestaw wizualny pozostaje osobnym materiałem do integracji.
- Kontrolna kompilacja kodu z bibliotekami Unity 6000.6.3f1: 0 błędów. Kontrola PNG i alfa wykonana poza Unity. Import/generowanie prefabów, wygląd w edytorze i Play Mode nie zostały przeprowadzone.
- Gotowa paczka jest w workspace ChatGPT; nie kopiowano plików do aktywnego checkoutu GitHub, nie zmieniano jego scen ani istniejących skryptów/prefabów, bez commit/push.
- Następny krok: import zestawu i wygenerowanie obiektów w Unity; następnie podłączenie wyglądu do PortMenuView i usług w osobnym etapie.

## PortVisualKit — etap 2: integracja usług, 2026-10-05

- Na polecenie użytkownika podłączono nowy panel wizualny do istniejących usług. PortMenuView zachowuje logikę napraw/ładowni/ulepszeń/stoczni; nowy PortVisualBridge obsługuje nawigację i odczyt rzeczywistego złota, nazwy projektu statku, HP kadłuba i masy ładowni. Ekwipunek prowadzi obecnie do listy ładowni, Warsztat do ulepszeń, Stocznia do kupna/budowy, Naprawy do dotychczasowych wycen, Wypłyń do odcumowania.
- Flota/zadania/handel/magazyn/tawerna nie mają jeszcze usług i są wyłączone. Przykładowy quest ukryto, liczba załogi pokazuje brak danych. Portret statku pozostaje ilustracją. Nie rozszerzono ekonomii ani zapisu; kupno/budowa nadal wymienia aktywny statek z istniejącym potwierdzeniem.
- PortVisualIntegrationBuilder po imporcie poza Play Mode łączy ostatni wygenerowany PortVisualCanvas z istniejącym PortMenu.prefab, zachowuje jego GUID i referencje usług. Weryfikuje 22 referencje mostka przed zapisem. Zachowuje edytowalne obiekty i blokuje kliknięcia przez tło UI. Można wykonać ręcznie Tools > GameWork > Ports > Visual kit > Connect port to game logic. Wynik generowania: Temp/PortVisualIntegration.result.
- Kompilacja kontrolna aktualnych źródeł Framework/ThePirate z lokalnymi bibliotekami Unity 6000.6.3f1: 0 błędów; ostrzeżenia CS0649 Inspectora i CS1701 kompilatora kontrolnego. Import i migracja prefabu wymagają osobnego potwierdzenia; Play Mode, wygląd i kliknięcia usług pozostają do sprawdzenia.
- Backup wcześniejszego PortMenuView, PortMenu.prefab + .meta i STATUS: workspace ChatGPT/port-ui-integration/backup-before-install. Unity dodatkowo zapisuje kopię prefabu w Temp/PortVisualIntegrationBackup przed migracją. Bez zmian scen i bez commit/push.
- Następny krok: w ShipSandbox zacumować E, sprawdzić aktualne dane, naprawę, ładownię, ulepszenia, anulowanie/potwierdzenie kupna lub budowy, Wypłyń oraz przerwanie menu przy zagrożeniu. Pauza i zmiana statku powinny zachować dotychczasowy cykl menu.

- Weryfikacja w działającym Unity przez istniejące Unity Pipeline: migracja PortMenu.prefab wykonana, 22 referencje mostka zweryfikowane; walidacja Edit Mode potwierdziła obecność usług i budowy, jeden Canvas, brak Missing Script, komplet CanvasRenderer, wyłączone niezaimplementowane usługi i ukryty przykładowy quest. Wyrenderowano dwa podglądy poza sceną gry w port-ui-integration/Verification. Play Mode i faktycznych transakcji nie przeprowadzono. Próba osobnej kopii wsadowej nie powiodła się z powodu konfiguracji ILPostProcessor; walidację dokończono w otwartym edytorze.

- Doprecyzowanie weryfikacji: zapisane podglądy offscreen były puste, więc nie potwierdzają wyglądu UI. Kolejna próba renderowania została zatrzymana przez błąd kodowania narzędzia run_script. Kontrola struktury i referencji prefabu pozostaje zaliczona; wygląd, kliknięcia i transakcje wymagają Play Mode.

## Ekwipunek portu — 2026-10-05
- Przygotowano edytowalny panel uGUI: siatka przewijana, filtry katalogu, szczegóły, masa i wyrzucanie 1/stosu z potwierdzeniem. PortMenuView wiąże panel z istniejącym ShipCargo/CargoInventory. Bez zmian katalogu, ekonomii i formatu zapisu. Kompilacja kontrolna: 0 błędów. Import i prefab oraz Play Mode pozostają do weryfikacji. Kopia poprzedniego widoku: workspace ChatGPT/port-inventory/backup. Bez zmian scen i commit/push.

- Unity wygenerowało i zapisało panel Inventory w PortMenu.prefab przez Tools/GameWork/Ports/Create inventory panel. Zachowano oddzielne obiekty i wcześniejsze usługi; meta skryptów wygenerowane przez Unity. Zasoby w obecnym katalogu nie mają przypisanych ikon ani opisów — UI korzysta z tych pól po uzupełnieniu Inspector. Play Mode nadal nieprzeprowadzony.

- Walidacja Edit Mode w Unity: PASS — 11 referencji panelu, 4 filtry, wszystkie CanvasRenderer oraz zachowane usługi portu. Play Mode pozostaje do sprawdzenia.

## CannonCombat VFX — podłączenie zdarzeń, 2026-10-06
- Cannon.Emit: błysk/dym każdego rzeczywistego strzału, także moździerza, oraz dym po salwie. Istniejący CannonBarrelRecoilVFX jest wywoływany opcjonalnie; wymaga przypisania osobnej lufy.
- CannonProjectile.Launch: ślad dymny. Trafienie: punkt i normalna kontaktu, drzazgi dla statku, pył dla lądu, rozbryzg dla wody bez collidera w ścieżce moździerza; większy efekt moździerza. Nie dodano mechaniki krytyków.
- CombatVFXLink tworzy wizualne komponenty dla aktywnych statków; Health.Changed steruje dymem i ogniem także po naprawie/wczytaniu. Udana PortRepairService.TryRepair wywołuje efekt naprawy; samo wczytanie nie wywołuje naprawy.
- ShipSinking.Begin uruchamia dym/bąble, zachowując istniejącą animację ruchu zatonięcia. Piana zależy od prędkości Rigidbody. Punkty przy burtach wynikają z BoxCollider; domyślny poziom piany/wody VFX to Y=0.65 i wymaga oceny wizualnej w ShipSandbox.
- Tekstury skopiowano do Resources/GameWork/CannonVFX, dzięki czemu podłączenie działa również dla statków tworzonych przez stocznię, bez ręcznej przebudowy prefabów.
- Poprawiono zatrzymanie ParticleSystem przed konfiguracją, materiały przezroczyste URP i nieobsługiwane GetInstanceID w pakiecie.
- Kompilacja kontrolna źródeł Framework/ThePirate z bibliotekami Unity 6000.6.3f1: PASS, 0 błędów. Nie wykonano importu/walidacji w edytorze ani Play Mode; wygląd, wielkość, położenie piany, kierunek błysku i trafienia wymagają testu Unity. Meta nowych plików ma wygenerować Unity. Bez commit/push. Backup w workspace ChatGPT/vfx-stage/backup-*.

## Czytelność i wygląd VFX — 2026-10-06
- Na podstawie informacji użytkownika o zbyt słabej widoczności zwiększono błysk lufy 0.85→2.4 m (czas 0.12→0.18 s), dym 0.62→1.55 m, rozbryzg 1→3.2 m, trafienie kadłuba 0.65→2.2 m, ślad kuli 0.16→0.42 m. Są to domyślne wartości komponentów tworzonych w runtime; własne wartości serializowane w Inspectorze pozostają nadrzędne.
- Dodano gorący rdzeń błysku i trafienia kadłuba, drobne żarzące cząstki przy lufie, rozszerzanie sprite dymu/rozbryzgu i grawitację kropli/odłamków. Wzmocniono dym uszkodzeń, płomienie, pianę i czytelność naprawy. Dym salwy pozostaje ograniczony czasowo.
- ParticleSystemScalingMode.Shape uniezależnia rozmiar cząsteczek od skali rodzica, zachowując lokalne punkty emisji. Pierścienie mają szerszą linię i współdzielony materiał cząsteczek z przezroczystością zamiast URP/Unlit. Naprawiono blok inicjalizacji Play błysku.
- Kontrolna kompilacja pełnych aktualnych źródeł Framework/ThePirate z bibliotekami Unity 6000.6.3f1: PASS, 0 błędów. Nie wykonano Play Mode ani renderowania sceny; końcowy wygląd i nakładanie dymu przy pełnej salwie wymagają oceny w Unity z normalnym i maksymalnym oddaleniem kamery. Bez zmian mechanik, scen, tekstur i commit/push. Backup w workspace ChatGPT/vfx-polish/backup-*.

## Trajektoria moździerza — 2026-10-06
- PPM pokazuje okrąg celu i linię przewidywanej trajektorii od lufy; LPM strzela. Podgląd i strzał korzystają z SpecialWeaponController.TryGetTrajectory. Linia znika po strzale/anulowaniu/wyłączeniu komponentu, kolor odpowiada poprawności celu.
- Czas lotu rośnie liniowo z odległością: Close Flight Time domyślnie 1.25 s przy 20 m, około 4.52 s przy 100 m z istniejącymi Flight Time=3 i Minimum Arc Height=25. Bliskie strzały lecą niżej. Parametry dalekiego lotu i istniejące prefaby zachowują dotychczasowe wartości.
- Zmieniono SpecialWeaponController.cs i FireRangeVisualizer.cs. Zachowano obsługę Rigidbody, kolizji, obrażeń, VFX, przeładowania i sterowania.
- Kontrolna kompilacja pełnych źródeł Framework/ThePirate z bibliotekami Unity 6000.6.3f1: PASS, 0 błędów. Sprawdzenie obliczeń dla 20/60/100 m: monotoniczny czas i wysokość, koniec toru na celu (start Y=2, cel Y=0). Nie wykonano Play Mode ani importu w edytorze. Podgląd pokazuje pełny tor balistyczny do celu; przeszkoda może przerwać rzeczywisty lot wcześniej. Dyskretna integracja Rigidbody może dać niewielką różnicę względem krzywej analitycznej.
- Następny krok: Unity ShipSandbox, sprawdzić podgląd po PPM, cele bliskie/dalekie, LPM i anulowanie PPM oraz trafienie w statek/ląd/wodę. Backup: workspace ChatGPT/mortar-trajectory/backup-*. Bez commit/push.

## Salwy burtowe do punktu kursora — 2026-10-06
- WeaponBattery oblicza czas lotu z odległości od środka luf i Projectile Speed (prędkość pozioma), następnie prędkość pionową do poziomu wody. Salwa zachowuje równoległe przesunięcie poziome i rozstaw dział, z istniejącym rozrzutem; kursor wskazuje środek przybliżonego obszaru upadku. Przy różnych wysokościach luf prędkości pionowe są dostosowane osobno. Tor ustalany na początku salwy; ruch statku podczas opóźnień może nieznacznie przesunąć miejsca upadku.
- CanFire/Safe sprawdzają łuk od każdej lufy w 24 odcinkach zamiast prostej. Zachowano ochronę przed własnym kadłubem i przeszkodami/innymi statkami w ostrzale AI, obrażenia, ulepszenia, kary uszkodzeń, przeładowanie i opóźnienia salwy. Wspólna zmiana obejmuje również AI oraz baterie dziobową/rufową.
- CannonProjectile wykonuje kontrolę odcinka i trafienia w wodę dla wszystkich pocisków z włączoną grawitacją, nie tylko moździerza. Trafienie w wodę kończy lot i uruchamia istniejący rozbryzg; wcześniejsze trafienie w statek/ląd działa jak dotychczas.
- ShipCombatController wybiera baterię, która rzeczywiście może strzelić w punkt kursora. FireRangeVisualizer pokazuje tylko jej sektor i przewidywane trajektorie poszczególnych luf; przeładowanie, uszkodzenie lub brak poprawnego celu je ukrywają. Podczas celowania moździerzem widoczny pozostaje jego dotychczasowy podgląd. Linie pokazują tor bez losowego rozrzutu i bez skracania na pierwszej przeszkodzie.
- Zmieniono WeaponBattery.cs, ShipCombatController.cs, CannonProjectile.cs, FireRangeVisualizer.cs. Bez zmian prefabów/scen i formatu zapisu. Backup: workspace ChatGPT/broadside-trajectory/backup-*.
- Kontrolna kompilacja pełnych bieżących źródeł Framework/ThePirate z bibliotekami Unity 6000.6.3f1: PASS, 0 błędów. Osobne sprawdzenie wzorów dla 8/20/40/80 m, trzech wysokości luf: koniec toru na wodzie, środek salwy i zachowany rozstaw PASS. Przy 35 m/s czasy wynoszą około 0.23/0.57/1.14/2.29 s. Odczyt kodu potwierdza wywołanie wspólnej baterii przez EnemyShipAI i istniejącego VFX wody; nie jest to test runtime.
- Nie przeprowadzono importu w Unity ani Play Mode. Do sprawdzenia w ShipSandbox: obie burty i dziób/rufa, bliski/daleki kursor, zmiana wybranej baterii, blokada przeładowania, cel poza sektorem, woda/ląd/kadłub, zachowanie AI, pauza oraz podgląd moździerza. Dyskretna fizyka może dać niewielkie odchylenie od analitycznego podglądu; wizualna ocena trajektorii i skuteczność AI pozostają niepotwierdzone. Bez commit/push.


## Pływające materiały i łup — 2026-10-06

- Dodano FloatingLoot, ShipLootCollector i generator edytowalnych prefabów Planks/Wreckage. Wizualne kołysanie nad ustawionym poziomem, zbieranie najbliższej paczki pod E w 10 m od środka gracza. CargoInventory atomowo dodaje zasoby; odmowa pozostawia łup. Port ma pierwszeństwo pod E; pauza i brak fokusu blokują wejście.
- ShipResourceReward wystawia paczki z istniejącej tabeli zamiast automatycznego przyznawania materiałów. Złoto bez zmiany. Wczytanie i ponowne obrażenia nie generują dodatkowej nagrody.
- VoyageSaveData/Runtime zapisują i odtwarzają nieodebrany łup; sprawdzają zasoby, ilości, pozycje i limit. Nowa pusta lista usuwa początkowe obiekty sceny; starszy zapis bez pola zachowuje rozstawione obiekty. Nie zapisujemy własnych wizualnych wariantów ani fazy kołysania.
- Bez zmian scen, istniejących prefabów i meta. Unity generuje nowe meta/prefaby po imporcie. Instrukcja: SETUP_FLOATING_LOOT.md. Bez commit/push.
- Weryfikacja: kontrolna kompilacja pełnych źródeł z bibliotekami Unity 6000.6.3f1: PASS, 0 błędów (ostrzeżenia CS0649/CS1701). 21 sprawdzeń wyodrębnionych metod produkcyjnych walidacji i zbierania z rzeczywistym CargoInventory: PASS; zastępniki cyklu życia Unity, brak testu fizyki/JsonUtility. Import Unity, prefab generation i Play Mode pozostają niepotwierdzone. Następny krok: test zbierania, pełnej ładowni, E przy porcie i zapisu podczas zatapiania.

## Ilustrowane menu główne — 2026-10-07
- Wdrożono MainMenu według referencji użytkownika: malowany port o zachodzie słońca, osobne logo, pergaminowe przyciski z czerwonym haftem słowiańskim, oddzielne ikony i edytowalne napisy. Twórcy pominięci. Cztery prefaby w Assets/ThePirate/UI/MainMenuVisualKit/Prefabs. GUID sceny i istniejące usługi zachowane.
- Nowa gra/Wczytaj/Wyjście korzystają z MainMenuView/GameFlow/SaveMenuActions. Dodano panel głośności ogólnej i istniejących poziomów QualitySettings, wyciszenie oraz PlayerPrefs. PL jest oznaczeniem języka; nie dodano nieobsługiwanych przycisków muzyki, lokalizacji i aktualizacji. Bez zmian formatu zapisu, pauzy i ShipSandbox.
- PirateMainMenuLayout obsługuje proporcje i Screen.safeArea, zostawia miejsce na komunikaty; Wyjście ukrywa się w Android/iOS. MainMenuVisualBuilder uruchamiany jawnie tworzy prefaby i migruje Canvas z kopią sceny w Documentation/MenuVisualBackups. Nowe meta wygenerowało Unity.
- Kontrolna kompilacja pełnych źródeł Framework/ThePirate: PASS, 0 błędów. Unity import/generowanie/reference checks: PASS. Play Mode przy 1600x900, 2000x900 i 1200x900: PASS — zdarzenia przycisków ustawień, modal, suwak, wyciszenie, powrót, dostępność zapisu, polskie znaki i granice przycisków. Wyrenderowano i obejrzano rzeczywiste Game View menu oraz ustawień. Testy przycisków wywoływały Button.onClick; nie są testem fizycznej myszy/dotyku. Fizyczny telefon, wycięcia, pełne wczytanie podróży, odsłuch i build wymagają osobnego sprawdzenia.
- Dokumentacja SETUP_MAIN_MENU_VISUAL_PL.md. Podglądy Documentation/MenuVisualPreviews; backup sprzed instalacji także w workspace ChatGPT/menu-implementation/backup-*. Bez commit/push.
- Rzeczywiste zdarzenie Nowa gra sprawdzone w Play Mode: aktywna scena ShipSandbox załadowana poprawnie. Po testach wyłączono Play Mode i przywrócono ustawienia podglądu. Brak błędów menu w Console; wcześniejszy timeout diagnostycznego eval dotyczył Pipeline i został ominięty przez polecenia edytora.

## Wybrzeze Trzech Sosen — 2026-10-10
- Po zatwierdzeniu zakresu utworzono osobna scene Assets/ThePirate/Scenes/WybrzezeTrzechSosen.unity: Unity Terrain 3000 x 2000 m, heightmap 1025, poziom wody Y=0, collider terenu, piasek/trawa/skala z istniejacego ArchipelagoKit.
- Recznie zinterpretowano kontury ilustracji art/Mapa na jasnym pergaminie.png: dwie glowne polnocne wyspy, poludniowy i wschodni lad, zatoki, ciesnina, mniejsze wyspy/skaly; 40 edytowalnych wielokatow w World/ThreePines/Coastlines.json. Wysokosci sa przyblizeniem, nie danymi geograficznymi.
- Generator Editor/World/ThreePinesMapBuilder.cs tworzy nowe wersje sceny/TerrainData, bez nadpisywania wczesniejszego rzezbienia. Unity wygenerowalo meta. Osobny shader/material oceanu poprawia obsluge glebi dla kamery ortograficznej; material i shader istniejacego oceanu pozostaja niezmienione.
- Unity 6000.6.3f1: generowanie/import PASS; TerrainCollider raycasts w 3 glownych obszarach ladu 3/3 (Editor); kompilacja bez bledow. Zapisano i oceniono podglad z gory Documentation/ThreePines/Overview.png. To nie jest dowod plywania ani kolizji statku w Play Mode.
- Scena zawiera kamere przegladowa i pusty Punkt_startowy_statku; bez mechanik gry/statku, drzew, osad i portow. Bez zmian Build Settings, MainMenu i ShipSandbox. Otwarta nowa scena w edytorze. Dokumentacja SETUP_THREE_PINES_PL.md.
- Nastepny krok: ocena konturow przez uzytkownika, dopracowanie terenu, potem drzewa/zabudowa i integracja statku z testem Play Mode. Bez commit/push.

## Statek gracza i port w WybrzezeTrzechSosen — 2026-10-10
- Dodano kopie aktualnego PlayerShip z ShipSandbox (model/uzbrojenie/ruch), pozycja (-667.26,0.5,399), kierunek +X. Port_Trzech_Sosen jest instancja istniejacego PortDock z unikalnym id three-pines-northwest-port, przy zatoce NW; drewniany pomost i lacznik z plaza, istniejace materialy Wood_Warm/Shadow.
- Main Camera ma TopDownCamera/Target=PlayerShip; ShipCombatController/Aim Camera=Main Camera i Water Height=0. PlayerHUD/Source jest jawnie przypisany do PlayerHudController na graczu (bootstrap HUD obsluguje tylko ShipSandbox).
- Nowy Editor/World/ThreePinesGameplaySetup.cs: kontrolowana instalacja w tej scenie, ochrona przed drugim graczem, menu sprawdzenia dokowania w Play Mode. Kopia sceny sprzed zmian w workspace ChatGPT/map-stage/WybrzezeTrzechSosen.before-gameplay.unity.
- Unity 6000.6.3f1: kompilacja bez bledow; spawn bez kolizji; Play Mode PASS po finalnej zmianie pomostu: CanDock (podejscie/fizyka), RestoreDock, dostepnosc PortRepairService, Undock, blokada/przywrocenie broni, przypisanie HUD. Metody wywolano programowo; fizyczne W/S/A/D/E, klikniecia uslug i dlugie plywanie pozostaja do sprawdzenia przez gracza.
- Zapisano i oceniono podglad kamery Documentation/ThreePines/PlayerPort.png (bez Canvas HUD). Play Mode zakonczony, scena zapisana. Zaktualizowano SETUP_THREE_PINES_PL.md. Bez zmian istniejacego ShipSandbox/prefabow mechanik/menu/Build Settings, bez commit/push.

## Mapa pergaminowa — 2026-10-10
- Dodano ParchmentMapView i jawny generator ParchmentMapBuilder oraz kopię oryginalnego pergaminu jako teksturę UI. M otwiera/zamyka, Esc zamyka, znacznik śledzi X/Z i kurs statku. Otwarta mapa blokuje wejście rozgrywki; nie zatrzymuje świata.
- Przeliczenie odpowiada ThreePinesMapBuilder: teren -1500..1500 X, -1000..1000 Z; obszar ilustracji x=40..1495, y od góry=40..970. Znacznik poza terenem jest ukryty. Po zmianie statku komponent wyszukuje aktywnego gracza w tej samej scenie.
- Instalacja prefabu i powiązania sceny przez GameWork/Map/Wybrzeze Trzech Sosen/Install parchment map; oczekujące żądanie Temp/ParchmentMap.install. Generator chroni istniejącą mapę, wymaga zapisanej sceny i robi backup sceny. Unity generuje meta.
- Weryfikacja instalacji/kompilacji i Play Mode wymaga aktualnego edytora; szczegóły SETUP_PARCHMENT_MAP_PL.md. Bez commit/push.
+- Weryfikacja finalna: kontrolna kompilacja Framework/ThePirate z nową mapą PASS, 0 błędów; Unity import i instalacja prefab/scena PASS (player UV 0.29,0.69), meta wygenerowane przez Unity. Poprawiono NPOT Scale=None, aby zachować 1536x1024.
+- Play Mode PASS: rzeczywisty komponent i marker reagują na zmianę X/Z oraz kurs 90 stopni; poza terenem marker ukryty, gameplay input zablokowane, Close działa. Wywołania programowe, fizyczne M/Esc nie testowane. Pozycję i obrót gracza odtworzono po teście.
+- Wygląd UI pozostaje do sprawdzenia: screenshot Pipeline pomija Canvas overlay, a dalsze próby ScreenCapture/run_script zatrzymał błąd ROSLYN001 Illegal byte sequence. Documentation/ThreePines/ParchmentMap.png pokazuje tylko kamerę i nie dowodzi wyglądu mapy. Play Mode zakończono.

## Regulowany zasięg minimapy — 2026-10-10
- HudMinimapView: Match Gameplay Camera domyślnie włączone; Visible Area Multiplier=4. Powierzchnia minimapy (koło) jest czterokrotnością obszaru ekranu rzutowanego na wodę Y=0. Promień sqrt(pole*mnożnik/pi); obliczenia obejmują perspektywę, pochylenie, zoom i proporcje ekranu. Gameplay Camera opcjonalna, domyślnie Main Camera z tej samej sceny.
- Inspector PlayerHUD/SafeArea/Minimap: mnożnik >=0.01, Water Height zgodny z oceanem. Po wyłączeniu Match Gameplay Camera działa World Radius w metrach (minimum 1); także fallback, gdy nie wszystkie narożniki widoku trafiają płaszczyznę wody w zasięgu kamery. Znaczniki i obraz używają wspólnego promienia; usunięto wcześniejsze przesunięcie skali znaczników o 10 pikseli.
- Kontrolna kompilacja: PASS 0 błędów. Unity import i Play Mode: PASS, pole kamery 31255.6 m², promień 199.489 m, stosunek powierzchni=4; mnożnik 16 podwaja promień, tryb ręczny 123 m działa. Metody wywołano programowo; fizyczny zoom i wygląd minimapy nie były testowane. Ustawienia testowe odtworzono, Play Mode zakończono. Kopia oryginalnego skryptu w workspace ChatGPT/minimap-scale-stage dostępna poprzez wcześniejszy stan pliku w hud-ui-stage; meta zachowane. Bez commit/push.

# Edytor przeciwników — 2026-10-10

- Dodano EnemySpawnZone, EnemySpawnProfile i EnemySpawnEditor: koło/prostokąt, grupy, poziomy, odnowienie, profile, punkty, wykluczenia, podgląd Gizmos, uchwyt promienia i otwieranie prefabu.
- Operacje edytora rejestrują Undo; sceny oznaczane jako zmienione, bez automatycznego zapisu.
- Walidacja sprawdza identyfikatory, liczebność, zakresy, dodatnie mnożniki, podstawowe komponenty i kolizje ręcznych punktów.
- Kompilacja Roslyn z lokalnymi bibliotekami Unity i Assembly-CSharp: 0 błędów. Ostrzeżenia przestarzałego API wyszukiwania obiektów skorygowano dla bieżącego Unity 6000.6.
- Nie wykonano testów importu, Edit Mode ani Play Mode. Nie zmieniono istniejących mechanik gry, scen, prefabów ani formatu zapisu.
- Następny etap: podłączenie runtime, poziomów i trwałego zapisu wielu przeciwników. Zapis dotychczas rozpoznaje przeciwnika po drużynie i wymaga rozszerzenia przed uruchomieniem wielu wrogów.


## Linie celowania niezależne od zoomu — 2026-10-11
- FireRangeVisualizer: ekranowe siatki uGUI, rdzeń 1.25 px PC / 2 px mobile, miękka krawędź 1 px i glow 2 px. Projekcja punktów świata przy budowaniu Canvas; gęstsze łuki 258 punktów. Aktywny sektor złoty, pozostałe gotowe sektory subtelne, trajektorie jasne.
- Wspólne TryGetTrajectory nadal wyznacza tory dział/moździerza. Bez zmian inputu, balistyki, scen/prefabów/meta lub zapisu. Nakładka bez raycastów, pod HUD-em; linie widoczne nad geometrią sceny (bez zasłaniania głębokością).
- Kontrolna kompilacja pełnych bieżących źródeł Framework/ThePirate z lokalnymi bibliotekami Unity/URP: PASS, 0 błędów. Unity import, wygląd przy zoomie, fizyczny telefon, wydajność i Play Mode nieprzetestowane.
- Instrukcja SETUP_AIM_LINES_PL.md; backup w workspace ChatGPT/aim-lines-stage/backup-*. Następny krok: ShipSandbox, oba skrajne zoomy, obie burty, przeładowanie i PPM/LPM moździerza. Bez commit/push.

# Spawnowanie runtime — 2026-10-11

- Dyrektor automatycznie obsługuje istniejące strefy; grupy definiują miejsca, poziomy, limity i odnowienie. Punkty ręczne/losowe sprawdzane względem kamer, dystansu, przeszkód, statków i portów.
- Parametry poziomu są stosowane per instancja do HP, WeaponBattery, prędkości ruchu i AI. Zachowano komponenty i nagrody z prefabów.
- Zapis rozszerzono opcjonalnymi polami dynamicznych wrogów i miejsc odnowienia. Przygotowanie wszystkich przeciwników jest walidowane przed zatwierdzeniem gracza i ekonomii; niepowodzenie usuwa przygotowane kopie bez łupu.
- Kompilacja kontrolna z aktualnymi źródłami projektu i bibliotekami Unity: 0 błędów. Istniejące ostrzeżenia pól Inspectora i VFX.
- Dodano jawnie uruchamiany test batch Play Mode: dwie strefy, limity, poziomy, capture/restore JSON, odtworzenie HP, brak duplikatów, śmierć i wyczerpanie odnowień. Wynik wykonania zostanie dopisany po uruchomieniu.
- Bez zmian zapisanych scen, prefabów, poziomów wody, warstw ani slotu użytkownika. Backup modyfikowanych źródeł w workspace enemy-spawn-runtime/backup-before-install.
- Pozostaje ręczny test scen użytkownika: rozmieszczenie, wygląd HUD, ostrzał/kolizje, kamery, porty i balans. Stan AI po wczytaniu zaczyna od nowego wyboru celu, z zachowanym punktem powrotu.


- Weryfikacja końcowa: izolowane Unity 6000.6.3f1, natywny import/kompilacja i krokowany Play Mode: PASS, 22 sprawdzenia. Objęto dwie strefy, globalny limit, poziomy/HP/baterie, JSON, stare zapisy, odtworzenie HP i tożsamości, brak duplikatów, czas odnowienia oraz jego limit. Test jawnie wywołuje AdvanceSpawning z powodu braku fokusu Game view w batch; normalna pętla zachowuje blokadę pauzy/fokusu. Wynik: enemy-spawn-runtime/unity-check/SpawnVerification/EnemySpawnRuntimeChecks.result. Slot gry użytkownika i jego sceny nie były zapisywane.
- Poprawiono istniejącą walidację pustego obiektu upgrades tworzonego przez JsonUtility dla przeciwników; niepuste ulepszenia gracza na wrogu nadal są odrzucane.
- Kontrolna kompilacja Roslyn po reimporcie bibliotek ujawniła niezgodność referencji BCL narzędzia kontrolnego. Weryfikację końcowych źródeł wykonano natywnym kompilatorem Unity, bez błędów.

