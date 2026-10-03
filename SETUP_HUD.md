# HUD — dane, zdarzenia i adapter statku

Kod: `Assets/ThePirate/UI/HUD/Runtime`, namespace `ThePirate.UI.HUD`.
To warstwa danych pod osobne, edytowalne elementy uGUI. Nie tworzy jeszcze grafiki na ekranie.

## Podłączenie

W `ShipSandbox` bootstrap dodaje podczas uruchomienia `PlayerHudController` i `ShipHudAdapter`
do jedynego aktywnego `CombatShip` drużyny 1. Nie zmienia zapisanej sceny ani prefabu.
Przy wielu takich statkach zgłasza ostrzeżenie: dodaj wtedy kontroler ręcznie do wybranego gracza.
W innej scenie dodaj `PlayerHudController` przez Add Component na głównym obiekcie statku.
Adapter dodaje się automatycznie. Źródła muszą być na tym samym obiekcie: Health,
Rigidbody, ShipMovementController (lub starszy ShipMovement) i ShipCombatController.
Opcjonalne źródła mogą być nieobecne. Refresh Interval domyślnie wynosi 0.1 s.

## Dane i odpowiedzialności

- `HudModels`: niezmienne snapshoty wytrzymałości, ruchu, ładowni, wiatru, baterii,
  celowania, wybranego celu, znaczników minimapy, zadania i komunikatu.
- `PlayerHudStateStore`: aktualny stan, oddzielne zdarzenia `...Changed` dla każdego obszaru.
  Wysyła je tylko po zmianie wartości. Listy kopiuje i udostępnia tylko do odczytu.
- `ShipHudAdapter`: odczytuje istniejące komponenty, bez zadawania obrażeń i poleceń ruchu/strzału.
- `PlayerHudController`: odświeża adapter w LateUpdate, według czasu nieskalowanego.
  Wyłączenie kontrolera czyści stan; ponowne włączenie odtwarza go z komponentów.
- `PlayerHudBootstrap`: podłączenie gracza w scenie testowej, bez obiektu DontDestroyOnLoad.

`Available = false` oznacza brak danych; nie należy wyświetlać wtedy zera jako prawdziwej wartości.
Wytrzymałość pochodzi wyłącznie z Health całego statku. Nie ma pasków załogi ani żagli.
Adapter nie zmienia istniejących reguł uszkodzeń modułów i ich wpływu na ruch.
Speed to długość poziomej prędkości Rigidbody w jednostkach Unity/s; RequestedSpeed
pochodzi ze sterownika i może być ujemna podczas cofania. Nie zakładamy jednostek węzłów.
Ładownia zawiera osobne HasCapacity, ponieważ konfiguracja pojemności jest opcjonalna.

Każdy wpis baterii odpowiada jednemu WeaponBattery/punktowi montażowemu, a nie pojedynczej lufie.
Kierunek jest wyznaczany względem osi statku: lewa, prawa, dziób, rufa.
Liczba dział pochodzi z listy używanej przez WeaponBattery. Ready oznacza gotowość baterii;
dopiero Aim.CanFire sprawdza możliwość strzału w aktualny punkt, łącznie z przeszkodami i pauzą.
ReloadProgress opisuje tylko czas (0..1); zniszczona bateria nie staje się gotowa po dojściu do 1.
Czas przeładowania uwzględnia karę za uszkodzenie z chwili rozpoczęcia salwy.
Obecny save przechowuje tylko czas pozostały: po wczytaniu jest on nowym czasem całkowitym
paska, więc pasek zaczyna od 0, ale moment gotowości jest zachowany.

Minimapa otrzymuje pozycje i kursy żywych aktywnych CombatShip w tej samej scenie,
oznaczone jako gracz/sojusznik/wróg/neutralny. Nie ma jeszcze mgły wojny, zasięgu radaru
ani portów. Identyfikatory ulong pochodzą z Unity EntityId i są ważne tylko w bieżącej sesji.

## Odbieranie zdarzeń przez przyszły widok

W OnEnable widoku subskrybuj np. `controller.State.DurabilityChanged += RenderDurability`,
następnie wykonaj `RenderDurability(controller.State.Durability)` dla stanu początkowego.
W OnDisable odepnij ten sam handler od tego samego magazynu. Tak samo obsłuż pozostałe sekcje.
Nie zastępuj instancji magazynu przy odświeżaniu. Zdarzenia są synchroniczne na głównym wątku Unity;
handler widoku powinien jedynie renderować dane, bez ponownego publikowania stanu.

Brakujące systemy przekazują dane jawnie:

```csharp
controller.State.SetWind(new HudWind(windVelocity));
controller.State.SetObjective(new HudObjective("Dopłyń do portu", false));
controller.SetTarget(enemyCombatShip); // null usuwa wybór
controller.State.Publish(new HudMessage("Cel poza zasięgiem", HudMessageKind.Warning));
```

Komunikaty są zdarzeniami jednorazowymi, bez historii. Adapter nie nadpisuje wiatru i zadania.
Punkt celowania nie wybiera automatycznie statku; system wyboru celu jest kolejnym etapem.
Stan HUD nie jest osobnym zapisem gry: po wczytaniu odtwarza się z systemów statku.

## Sprawdzenia i test w Unity

2026-09-29: kompilacja kontrolna całego Framework/ThePirate z bibliotekami Unity 6000.6.3f1
przeszła bez błędów; istnieją ostrzeżenia CS0649 o polach przypisywanych w Inspectorze.
19 asercji magazynu/modeli przeszło poza Unity: zmiany sekcji, odpinanie subskrypcji,
niezmienność list, postęp przeładowania, znaczniki i komunikaty. To nie jest test Play Mode.

1. Pozwól Unity zaimportować pliki i wygenerować .meta; sprawdź Console.
2. Otwórz ShipSandbox i uruchom Play. Na graczu powinny pojawić się dwa komponenty HUD.
3. Odbiornikiem zdarzeń/debuggerem sprawdź State: wytrzymałość, prędkość W/S/A/D,
   cztery baterie oraz znaczniki. Własności C# nie wyświetlają się automatycznie w Inspectorze.
4. Oddaj salwę: tylko właściwa bateria ma odliczać. Pauza zatrzymuje reload,
   a Aim.CanFire jest false. Zniszczenie baterii blokuje Ready.
5. Zadaj obrażenia i wczytaj zapis: stan HUD powinien dogonić Health i reload w maksymalnie 0.1 s.
6. Wybierz cel przez SetTarget, następnie zniszcz go: cel i znacznik powinny zniknąć.
7. Wyłącz/włącz kontroler oraz wróć do menu i rozpocznij nową grę: brak duplikatów kontrolera.

Import i Play Mode pozostają do potwierdzenia. Następny etap: widoki uGUI według ustalonego układu.
