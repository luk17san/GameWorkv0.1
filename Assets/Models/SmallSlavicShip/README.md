# Small Slavic Ship — Unity prototype

Mały stylizowany statek (12 m), przygotowany dla kamery 3D top-down. Oś Z wskazuje dziób, Y górę, a jednostka modelu odpowiada 1 metrowi w Unity.

## Import

1. Skopiuj cały folder do `Assets/ThePirate/SmallSlavicShip`.
2. W Unity wybierz `The Pirate > Create Small Slavic Ship Prefab`.
3. Prefab pojawi się w `Assets/ThePirate/SmallSlavicShip/Prefabs`.

Prefab otrzyma Rigidbody, uproszczony BoxCollider, po dwa stanowiska dział na burtę, środkowy punkt moździerza, punkty efektów kilwateru oraz cztery punkty wyporności. Model OBJ zachowuje osobne grupy dla kadłuba, pokładu, masztu, żagla, steru, tarcz i ozdób.

Wersja 4 rozwija model według referencji stylizowanego statku słowiańskiego: większy prostokątny żagiel z bordiurą i symbolem, rzeźbiona głowa na wysokim dziobie, proporczyk, masztowa korona, koło sterowe, latarnia, beczki z metalowymi obręczami oraz dodatkowe ornamenty burtowe. Zachowano zaokrąglony kadłub, profilowany pokład i punkty integracyjne Unity.

To nadal zoptymalizowany prototyp low-poly. Przed wersją produkcyjną warto wykonać UV, tekstury PBR, LOD-y oraz dokładniejsze collidery.
