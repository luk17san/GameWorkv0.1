# Zapis i wczytywanie — jeden slot

## Co robi menu

- `MainMenu`: **Nowa gra** zaczyna od początku i zachowuje dotychczasowy zapis. **Wczytaj** uruchamia zapisaną rozgrywkę.
- `PauseMenu`: **Zapisz / nadpisz** zapisuje bieżący stan do jednego slotu. **Wczytaj** prosi o potwierdzenie, a potem wczytuje slot od nowa, odrzucając stan niezapisany.
- Przycisk Wczytaj jest nieaktywny, kiedy nie ma poprawnego pliku ani poprawnej kopii zapasowej.
- Plik `gamework-save-v1.json` znajduje się w `Application.persistentDataPath`. Przy nadpisaniu poprzedni poprawny zapis przechodzi do `.bak`; niepełny plik `.tmp` nie jest używany do wczytywania.

Zapis zawiera pozycję, obrót, prędkość i ładunek statku gracza, zdrowie kadłuba i modułów oraz czasy przeładowania baterii i moździerza. Jeśli w scenie znajduje się przeciwnik drużyny 2, jego stan także zostanie zapisany. Pociski będące w locie i rozpoczęte salwy nie są odtwarzane. Gracz musi żyć przy zapisywaniu. Po zmianie liczby statków lub układu ich modułów starszy zapis może zostać odrzucony z komunikatem.

## Konfiguracja w Unity

Po imporcie skryptów narzędzie **GameWork > Menu > Dodaj zapis i wczytywanie** dodaje przyciski do istniejącej sceny `MainMenu` i prefabu `PauseMenu`, zachowując pozostałe elementy. Przyciski i komponent `SaveMenuActions` pozostają osobnymi obiektami do edycji w Inspectorze. Nie trzeba dodawać komponentów do prefabu statku. Ponowne użycie narzędzia nie dodaje duplikatów przycisków.

## Test w Play Mode

1. Uruchom `MainMenu`: przed pierwszym zapisem Wczytaj powinno być nieaktywne.
2. Rozpocznij grę, przepłyń kawałek, zadaj obrażenia statkowi i oddaj salwę. Jeśli dodałeś przeciwnika do sceny, uszkodź również jego. Otwórz pauzę i wybierz Zapisz / nadpisz.
3. Wznów, zmień pozycję i zadaj kolejne obrażenia. Wróć do menu, kliknij Wczytaj. Sprawdź pozycję, zdrowie, moduły i przeładowanie każdego statku obecnego podczas zapisu.
4. Rozpocznij Nową grę: statek ma stan początkowy. Wróć do menu: Wczytaj nadal jest dostępne.
5. Sprawdź wczytanie z pauzy oraz ponowny zapis po wczytaniu.
6. W kopii plików testowych uszkodź główny plik JSON i sprawdź odczyt `.bak`. Gdy oba pliki są niepoprawne, gra ma pokazać błąd i nie stosować częściowego stanu.
7. Po zbudowaniu gry wykonaj ponownie kroki 2–4, by sprawdzić ścieżkę `persistentDataPath` poza edytorem.

Nie testuj uszkodzeń na jedynej kopii ważnej rozgrywki. Wynik Play Mode wpisz do STATUS.md.
