# Menu gry — etap podstawowy

## Utworzenie zasobów

Po imporcie kodu w Unity 6000.6.3f1 użyj **GameWork > Menu > Utwórz menu**. Narzędzie tworzy:

- `Assets/ThePirate/Scenes/MainMenu.unity` — ekran startowy;
- `Assets/ThePirate/Resources/GameWork/PauseMenu.prefab` — osobne, edytowalne obiekty uGUI;
- wpisy `MainMenu` i `ShipSandbox` w Build Profiles / Scene List, w tej kolejności.

Nie zapisuje otwartej sceny `ShipSandbox`. Menu pauzy jest tworzone z prefabu w czasie uruchamiania tej sceny. Ponowne użycie narzędzia zachowuje istniejącą scenę menu i prefab, więc własne poprawki wyglądu pozostają. Jeżeli trzeba je utworzyć od nowa, najpierw zachowaj własne zmiany.

Gdy otwierasz `ShipSandbox` bezpośrednio w edytorze, menu pauzy także się uruchamia. W zbudowanej grze pierwszą sceną jest `MainMenu`.

## Zachowanie

- **Nowa gra** ładuje czysty `ShipSandbox`; nie ma jeszcze zapisu postępu.
- **Esc** otwiera menu pauzy. Wznów przywraca poprzednią skalę czasu, stan dźwięku i kursora.
- **Powrót do menu** i **Wyjdź z gry** wymagają potwierdzenia, ponieważ bieżący postęp rozgrywki nie jest zapisywany.
- Pauza blokuje polecenia statku, strzał LPM oraz zoom. Strzał nad UI również jest blokowany.
- W edytorze przycisk Wyjdź kończy Play Mode, w wersji gry zamyka aplikację.
- Lista scen jest sprawdzana przed ładowaniem. Przy błędzie ekran wyświetla komunikat i pozwala ponowić próbę.

## Inspektor i dalsza edycja

Scena `MainMenu` zawiera `MainMenu`, `MenuCanvas` i `EventSystem`. Prefab `PauseMenu` zawiera `PauseService`, `GameFlow`, `PauseMenuView` i `MenuCanvas`. W Canvas są osobne obiekty tła, karty, etykiet, przycisków i panelu potwierdzenia. Można zmieniać kolory, teksty, położenie i grafiki w Inspectorze. Przyciski mają przypisane odpowiednie metody w On Click.

W `GameFlow` pola `Main Menu Scene` i `Gameplay Scene` wskazują pełne ścieżki zasobów. Jeśli zmienisz nazwy lub ścieżki scen, zaktualizuj oba komponenty `GameFlow` i stałe w `PirateMenuBootstrap`.

## Test w Unity

1. Po imporcie sprawdź Console i uruchom `MainMenu` w Play Mode.
2. Kliknij **Nowa gra**. Sprawdź, czy otwiera się `ShipSandbox` i statek reaguje na W/S/A/D, LPM oraz kółko myszy.
3. W czasie ruchu naciśnij **Esc**. Statek i pociski powinny stanąć, a kliknięcia i kółko nie mogą sterować grą.
4. Kliknij **Wznów**. Statek może płynąć dalej; kliknięcie przycisku nie oddaje strzału.
5. Otwórz pauzę, wybierz **Powrót do menu**, potem **Anuluj**; następnie potwierdź powrót.
6. Sprawdź ponowne **Nowa gra**: scena zaczyna od początku.
7. Z menu głównego kliknij **Wyjdź z gry**; Play Mode powinien się zatrzymać.
8. Powtórz start w wersji zbudowanej, żeby sprawdzić kolejność scen i wyjście z aplikacji.

Zapis i wczytywanie opisuje SETUP_SAVE.md. Ustawienia gry pozostają następnym etapem.
