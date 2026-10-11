# Złoto i nagrody — etap 1

## Co zostało dodane

- Assets/Framework/Economy/GoldWallet.cs: saldo, dodawanie, wydawanie i odtwarzanie.
- Assets/ThePirate/Scripts/Economy/PlayerGold.cs: portfel na graczu i Current Gold w Inspectorze podczas gry.
- Assets/ThePirate/Scripts/Economy/ShipGoldReward.cs: nagroda na przeciwniku.
- CombatShip.Awake: automatyczne dodawanie odpowiedniego komponentu.
- VoyageSaveData/Runtime: zapis, walidacja i odtwarzanie salda.

Istniejące sceny i prefaby nie wymagają przebudowy. Unity wygeneruje .meta nowych skryptów i folderów. Componenty dodawane w Play Mode znikają po zatrzymaniu sceny — to prawidłowe zachowanie.

## Inspector

Domyślna nagroda wynosi 100. Aby ustawić inną, poza Play Mode dodaj Ship Gold Reward na głównym obiekcie przeciwnika z CombatShip i Health; ustaw Gold Reward i zapisz scenę/prefab. Zero wyłącza nagrodę. Bez komponentu automatycznie obowiązuje 100.
Na graczu Player Gold pojawia się automatycznie podczas uruchomienia. Current Gold pokazuje saldo; ręczna zmiana tego podglądu nie jest operacją portfela. Nie dodano jeszcze wskaźnika złota w HUD ani menu zakupów. Wynik nagrody jest również w Console.

## Test w Unity (do wykonania)

1. Poczekaj na import i sprawdź Console; uruchom nową grę. PlayerShip → Player Gold → Current Gold = 0.
2. Zadaj przeciwnikowi nieśmiertelne obrażenia: nadal 0. Zatop go: dokładnie jeden komunikat +100, saldo 100. Obserwuj zatapianie i usunięcie; nie ma kolejnej nagrody.
3. Zapisz grę w trakcie zatapiania, wczytaj: saldo 100, brak dodatkowej nagrody. Powtórz zapis/wczytanie po usunięciu przeciwnika.
4. Zapisz przed zatopieniem przy saldzie 0, zatop, wczytaj zapis: saldo wraca do 0 i żywy przeciwnik wraca. Ponowne zatopienie daje 100 w odtworzonej historii.
5. Nowa gra ma saldo 0; poprzedni plik zapisu nadal można wczytać. Stary zapis v1 bez pola gold powinien odtworzyć saldo 0.
6. Ustaw Gold Reward = 25 poza Play Mode: nagroda 25. Ustaw 0: brak nagrody. Sprawdź zderzenie i moździerz; źródło śmiertelnych obrażeń nie zmienia reguły.
7. Uszkodzenie modułu bez zatopienia kadłuba nie daje pieniędzy. Śmierć gracza i neutralny statek nie dają nagrody.

Aktualny zapis wspiera gracza i co najwyżej jednego przeciwnika. Nagrody obsługują wiele obiektów, ale ten etap nie rozszerza mapowania zapisu do wielu wrogów. Ponowne wczytanie wcześniejszego zapisu cofa całą historię, w tym złoto — nie jest to trwała nagroda konta.
