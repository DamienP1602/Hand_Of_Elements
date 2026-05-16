# Hand of Elements
## Français
Hand of Elements est un projet de jeu de carte inspiré de Hearthstone et Magic the Gathering où l'on joue contre un autre joueur avec un deck de carte préfait ou fait par le joueur.
Chaque cartes possèdents un élément propre : Feu/Eau/Air/Terre. Chaque cartes possèdent des effets et certaines peuvent avoir des effets supplémentaire si la carte précédement joué est du même type.
Les joueurs peuvent poser des cartes sur le plateau et ces cartes peuvent soit attaquer d'autres cartes adverses soit attaquer l'adversaire directement.
Ils peuvent également jouer des cartes sorts, qui auront un effet immédiat sur la partie. Par exemple : Infliger des dégâts, soigner ou donner un effet bénéfique ou néfaste sur un joueur ou une carte sur le terrain.
Les joueurs possèdents des points de vies, si ces points de vies tombent à 0, le joueur perd et son adversaire gagne la partie.

Ce jeu est un projet personnel fait pendant mes années d'études.
Mon but était de travailler avec le plugin Netcode pour pouvoir faire du multijoueur et aussi de pouvoir se connecter à des sessions créés par les joueurs et pouvoir jouer sur des réseaux différents.
Voici les éléments principaux/importants sur les quels j'ai travaillé sur ce projet :
### Scriptable Objects
- Cela m'a permit de créer et éditer les données de mes cartes que les joueurs pourront jouer
- Un principe d'ID est utilisé pour envoyer les informations aux joueurs, les cartes seront automatiquement généré par ce simple ID que se soit dans la main du joueur ou une fois posé sur son terrain.
### UI
- J'ai créé plusieurs outils de debug nécessaire pour pouvoir afficher les informations envoyés depuis le server à un client.
### Multijoueur
- Le système de connection peut se faire de deux manières : Local et Online.
- - Le Local permet de créer une session simplement avec Netcode. Il permet de se connecter depuis un même ordinateur à une session lancé (principalement fait pour le développement)
- - Le Online utilise le Unity Relay (Unity Gaming Services). Il va dans un premier temps se connecter aux servers puis générer un code temporaire que le client pourra rentrer une fois s'être connecté aux servers d'Unity pour pouvoir rejoindre la session.
- Les Network Variables sont beaucoup utilisés pour pouvoir synchroniser des valeurs importantes comme : Quel joueur peut jouer selon le tour, quel carte le joueur a selectioné ou encore quel ID possède la carte.
- Le joueur est un objet important qui permet de faire le relay entre une action client et le server. Lorsqu'un objet doit executer du code du côté server, le joueur me permet de faire la passerèle.
### Managers
- Le jeu tourne principalement sur un Game Manager, j'y regroupe les passages des executions servers pour éviter de disséminer le code dans les divers components.
- Le système d'effets des cartes ainsi que les sorts passent par un Spell Manager, il détermine les cibles et effectue les changements d'états ou de statistiques des cartes du côté server qu'il va transmètre automatiquement aux joueurs avec les Network Varaibles.

Ce que j'aimerai continuer et faire pendant ce projet
### IA
- Pouvoir jouer contre une IA avec plusieurs niveaux de difficultés.
### Multijoueur
- Ajouter des informations propres aux joueur comme un pseudo en jeu, le nombre de partie et victoire. Les données seront stockés directement sur l'ordinateur des joueurs puisque je n'ai pas de server qui pourrait garder ces informations.
- Un principe alternatif où les joueurs seront sélectionnés par rapport à un MMR. Même si la fonctionnalité ne sera pas implémentée, j'aimerai m'y renseigner.
