# Zasady pracy nad GameWorkv0.1

- Pracuj małymi krokami: jedno zadanie, weryfikacja, opis wyniku.
- Wyjaśniaj po polsku cel zmian, zmienione pliki i konfigurację w Inspectorze. Użytkownik uczy się Unity.
- Przed zmianami przeczytaj PLAN.md i STATUS.md oraz sprawdź aktualny kod i stan Git.
- Nie nadpisuj niezwiązanych zmian użytkownika. Nie edytuj tych samych plików równolegle z innym asystentem.
- Uniwersalne moduły umieszczaj w Assets/Framework, elementy gry w Assets/ThePirate. Framework nie może zależeć od ThePirate.
- Dodawaj abstrakcje dopiero wtedy, gdy wymaga ich działająca funkcja gry.
- Zachowuj pliki .meta. Nowe zasoby i foldery w Assets powinny otrzymać meta wygenerowane przez Unity.
- Używaj zainstalowanego Input System. Nie zmieniaj wersji Unity ani pakietów bez konkretnej potrzeby zadania.
- Kamera pozostaje nad statkiem, może być lekko pochylona, płynnie śledzi statek i nie obraca się wraz z nim.
- Docelowo LPM obsługuje ostrzał burtowy; pociski jednej burty lecą równolegle. PPM pozostaje na przyszłą broń specjalną.
- Po pracy aktualizuj STATUS.md: wykonane zmiany, przeprowadzone sprawdzenia, ograniczenia i następny krok.
- Nie deklaruj testów Unity, jeśli nie uruchomiono edytora. Oddziel sprawdzenie kodu od testu rozgrywki.
- Zmiany przygotowane w chmurze przekazuj do przeglądu na osobnej gałęzi. Nie łącz ich automatycznie z main.

