# Ocean 0.3 — grzbiety, piana, kierunki i kilwater

## Aktualizacja

Podmień zawartość dotychczasowego ArchipelagoKit plikami z `Assets/GameWork/ArchipelagoKit`
w archiwum. Jeśli paczka jest u Ciebie w `Assets/Models/ArchipelagoKit`, zachowaj tę lokalizację.
Nie twórz drugiej kopii skryptów. Zachowaj istniejące .meta i folder Generated.

1. Poczekaj na kompilację w Unity.
2. Zaznacz ocean w Hierarchy i wybierz **GameWork → Archipelago → Water → Upgrade Selected Ocean (Keep Colours)**.
   Ustawia nowe parametry grzbietów i piany w aktualnym materiale, pozostawiając dotychczasowe kolory i tekstury.
   Jeśli ten materiał jest współdzielony przez kilka oceanów, otrzymają te same ustawienia.
3. Zaznacz główny, poruszający się obiekt statku i wybierz **GameWork → Archipelago → Water → Add Wake To Selected Ship**.
4. W nowym komponencie **ShipWakeEmitter** przypisz ocean do pola **Ocean**.
5. Ustaw szerokość statku i położenie rufy według wskazówek poniżej. Zapisz scenę, uruchom Play i płyń.

## Nowa woda

- Trzy krzyżujące się układy fal o różnych długościach i szybkościach.
- Ich udział zmienia się płynnie w czasie, dzięki czemu zmienia się dominujący kierunek.
  Nie obraca się cała tekstura oceanu. Długość okresu podajesz w sekundach.
- Węższy profil grzbietów i ich pochylenie w normalnych powierzchni tworzą ostrzejszy wygląd czubków.
- Piana ma nieregularne krawędzie, zmienną gęstość, pozostałości za grzbietem i pulsowanie przy brzegu.

| Parametr materiału | Start | Co zmienia |
|---|---:|---|
| Main Wave Direction | 35° | Podstawowy kierunek w płaszczyźnie XZ: 0° = +X, 90° = +Z |
| Cross Wave Angle | 48° | Rozbieżność kierunków |
| Direction Change Period | 120 s | Czas cyklu zmiany udziału kierunków |
| Direction Variation | 0.65 | Intensywność zmiany; 0 utrzymuje stałe udziały |
| Crest Sharpness | 2.8 | Zawężenie grzbietu; zakres 1–5 |
| Crest Normal Strength | 1.8 | Wyrazistość stoków fali w oświetleniu |
| Foam Breakup | 0.55 | Rozbicie piany na nieregularne fragmenty |
| Foam Behind Crests | 0.65 | Ilość resztkowej piany za grzbietami |

Istniejące Whitecap Coverage i Whitecap Brightness nadal ustawiają ogólną ilość i jasność piany.
Skala i kolor z wersji 0.2 pozostają edytowalne. Zmiana grzbietów dotyczy wyglądu i normalnych,
nie przesuwania wierzchołków, wysokości fizycznej morza ani wyporności.

## Ustawienie kilwateru

Komponent nie wymaga konkretnego kontrolera ani Rigidbody. Mierzy przesunięcie statku w świecie
w płaszczyźnie XZ, wygładza prędkość i wytwarza ślad dopiero podczas ruchu.

Przykład dla statku długości około 10 m i szerokości 3 m:

| Pole | Wartość |
|---|---|
| Ocean | Obiekt Ocean_Stylized z komponentem StylizedOceanSurface |
| Ship Width | 3 (szerokość w jednostkach świata) |
| Local Stern Offset | X=0, Y=0, Z=-4 (przy dziobie skierowanym w lokalne +Z) |
| Minimum Speed | 0.3 jednostki/s |
| Speed For Full Wake | 9 jednostek/s |
| Lifetime | 10 s |
| Spread Speed | 0.9 jednostki/s |
| Sample Interval | 0.08 s |
| Maximum Samples | 160 |

Możesz zamiast Local Stern Offset utworzyć pusty obiekt **WakeOrigin** na rufie i przypisać go
w polu **Stern**. To najłatwiejsze przy modelach o innym kierunku lokalnym lub niestandardowej skali.
Po zaznaczeniu statku cyjanowy znacznik w Scene View pokazuje początek i szerokość śladu.
Wartości Local Stern Offset są w lokalnym układzie i podlegają skali obiektu; Ship Width jest w skali świata.
Jeśli Ocean nie jest przypisany, obowiązuje pole Sea Level (domyślnie Y=0).

Ślad składa się z piany pośrodku i dwóch rozchodzących się ramion za statkiem.
Pozostaje na przebytej trasie podczas skręcania, rozszerza się wraz z wiekiem i zanika.
Postój nie tworzy nowego śladu; stara piana wygasa. Wygładzanie prędkości może opóźnić wyłączenie
emisji o ułamek sekundy. Teleport większy niż Teleport Distance czyści ślad.
Przy cofaniu automatyczny punkt emisji przechodzi na przeciwny koniec; przypisany ręcznie Stern
zawsze pozostaje wskazanym punktem. Przy niestandardowym kierunku modelu ustaw punkt Stern ręcznie.

## Sprawdzenie w Unity

- Postój: brak nowej piany.
- Powolny ruch i pełna prędkość: odpowiednio słabszy i mocniejszy ślad.
- Skręt: stary kilwater pozostaje na poprzednim kursie.
- Zatrzymanie: po Lifetime cały ślad znika.
- Teleport: brak długiego pasa przez mapę.
- Wyłączenie i ponowne włączenie statku: stare obiekty efektu są usuwane.
- Brak piany: sprawdź Wake Material, poziom morza, skalę modelu i to, czy komponent jest na poruszającym się obiekcie.

Efekt działa w Play Mode. Nie dodaje fizyki, obrażeń, sił, dźwięku ani przestrzennych cząsteczek rozprysku.
Kilwater to animowane, przezroczyste wstęgi na powierzchni; nie symulacja płynu.
Na telefonie ogranicz Maximum Samples do 96 i Lifetime do 6 s; wydajność wymaga pomiaru na urządzeniu.

## Weryfikacja paczki

Sprawdzono numerycznie profil grzbietów, pochodne, ciągłość wag kierunków, zakresy indeksów siatki,
odwołania do parametrów materiałów i integralność archiwum. Nie uruchomiono kompilacji C#/HLSL
ani renderingu w Unity w środowisku przygotowania. Te próby nie zastępują testu Play Mode w projekcie.

Źródła API siatki:
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.MarkDynamic.html
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.SetVertices.html
