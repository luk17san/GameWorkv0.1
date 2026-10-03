# Port i cumowanie

## Dodanie portu do sceny

Po imporcie kodu Unity tworzy brakujący edytowalny prefab `Assets/ThePirate/Resources/GameWork/PortDock.prefab`. Istniejącego prefabu nie nadpisuje.

1. Otwórz ShipSandbox poza Play Mode i poczekaj na kompilację.
2. Wybierz **Tools > GameWork > Ports > Place port in current scene**. Powstaje instancja przy (-60, 0, -60), z własnym trwałym Port Id. Zapisz scenę.
3. Przesuń lub obróć port według potrzeb. Berth jest punktem zatrzymania środka statku; zielona strzałka Gizmos wskazuje jego kierunek. Pomost i ląd mają zwykłe collidery. Zachowaj wolną przestrzeń wokół Berth.
4. PortDock: Approach Radius = 12 m, Maximum Dock Speed = 1 m/s. Display Name i elementy modelu można edytować. Dla kolejnych portów używaj menu umieszczania lub nadaj każdej instancji inne Port Id; nie zmieniaj ID portu użytego w zapisie.

## Zachowanie

ShipDocking jest automatycznie podłączany do statku gracza (Team 1). E cumuje w najbliższym porcie w strefie, przy małej prędkości fizycznej i prędkości napędu. Sprawdzane jest wolne miejsce i droga kadłuba do Berth. Statek ustawia się w punkcie i kierunku Berth, zachowując wysokość na wodzie. Po zacumowaniu ruch i wszystkie rodzaje uzbrojenia są blokowane. E odcumowuje; statek rusza dopiero po ponownym zadaniu prędkości. Port ma jedno miejsce cumowania. Komunikat nad polem rozgrywki pokazuje dostępność/przyczynę blokady i nie przechwytuje myszy.

Cumowanie jest niemożliwe, gdy wrogie AI ma gracza jako aktywny cel pościgu/walki. Ostrzał gracza, rozpoczęcie skierowanej w niego salwy (także pudła), ogień załogi i faktyczne obrażenia kadłuba/modułów odnawiają blokadę. CombatShip / Docking Combat Delay domyślnie wynosi 10 sekund czasu gry; pauza zatrzymuje odliczanie. Przy ponownym zagrożeniu cumy są zwalniane — port nie zapewnia nietykalności. Naprawa i odtwarzanie zdrowia z zapisu nie są nowymi obrażeniami.

Zapis przechowuje stan zacumowania, ID portu i pozostały czas blokady walki. Format v1 zachowuje zgodność ze starszymi zapisami (nowe pola domyślnie oznaczają brak zacumowania i brak blokady). Brak portu, niejednoznaczne ID lub przemieszczenie jego punktu względem zapisanej pozycji powodują odmowę wczytania przed zmianą statków. Umieszczony port musi być zapisany w scenie poza Play Mode, aby przetrwać ponowne wczytanie sceny.

## Test Play Mode

1. Dodaj port menu i zapisz scenę. Uruchom świeżą grę; podpłyń do pierścienia i wyhamuj S. E: statek stoi przy pomoście, komunikat pokazuje odcumowanie; W/S/A/D, LPM, PPM i automatyczny ogień nie uruchamiają ruchu/ataków.
2. E odcumowuje, W rozpędza. Przy prędkości powyżej 1 m/s odmowa cumowania. Poza promieniem brak podpowiedzi.
3. Przeciwnik ściga/strzela, lecz pudłuje: E nadal zablokowane. Oderwij się od pościgu, przestań strzelać i odczekaj 10 s; sprawdź możliwość cumowania. Oddzielnie sprawdź obrażenia kadłuba i modułu.
4. W czasie blokady włącz pauzę; odliczanie nie powinno postępować. Zagrożenie po zacumowaniu zwalnia cumy, a obrażenia nadal działają.
5. Postaw przeszkodę na Berth lub podejściu: odmowa. Zmień położenie portu/rozmiar kadłuba i sprawdź kolizje wizualnie. Kierunek Berth nie może prowadzić do nakładania się kadłuba na pomost.
6. Zapisz zacumowanego gracza, odcumuj, wczytaj: ponownie stoi przy tym samym porcie i E pozwala odpłynąć. Zapis w trakcie blokady przywraca pozostały czas. Sprawdź też starszy zapis bez nowych pól.
7. Po kopii sceny/zapisu zmień ID lub usuń port: wczytanie ma odmówić i pokazać błąd. Przywróć oryginalną scenę. Nie duplikuj ID portów.

## Zakres sprawdzeń

Kontrolna kompilacja kodu oraz testy logiki poza Unity nie potwierdzają generacji prefabu, importu, wyglądu, wejścia z klawiatury, fizyki ani pełnego zapisu/wczytania w Play Mode. Handel, naprawy, magazyn i ekran portu nie należą do tego etapu.
