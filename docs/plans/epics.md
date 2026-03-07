---
title: 'Development Epics'
project: 'Valenthia Chronicles'
date: '2026-02-24'
author: 'Drekth'
version: '1.0'
status: 'in_progress'
total_epics: 17
stories_defined: [1, 2, 3]
stories_pending: [4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17]
---

# Valenthia Chronicles - Development Epics

## Epic Overview

| #    | Epic Name                          | Phase                        | Dependencies | Est. Stories | Status      |
| ---- | ---------------------------------- | ---------------------------- | ------------ | ------------ | ----------- |
| E1   | Core Infrastructure & Bootstrap    | 1 — Fondations Techniques    | None         | 6            | defined     |
| E2   | Scene Management & Zone System     | 1 — Fondations Techniques    | E1           | 5            | defined     |
| E3   | Player Controller & Entity Framework | 1 — Fondations Techniques  | E1, E2       | 6            | defined     |
| E4   | Combat System                      | 2 — Gameplay Core            | E1, E3       | TBD          | placeholder |
| E5   | UI/HUD Foundation                  | 2 — Gameplay Core            | E1           | TBD          | placeholder |
| E6   | Character Progression              | 2 — Gameplay Core            | E1, E3, E4   | TBD          | placeholder |
| E7   | Save System                        | 3 — Persistance              | E1, E3       | TBD          | placeholder |
| E8   | Inventory & Equipment              | 3 — Persistance              | E1, E3, E5   | TBD          | placeholder |
| E9   | Quest System                       | 4 — Systèmes Narratifs       | E1, E3, E5   | TBD          | placeholder |
| E10  | Dialogue System                    | 4 — Systèmes Narratifs       | E1, E5, E9   | TBD          | placeholder |
| E11  | World State & Consequences         | 4 — Systèmes Narratifs       | E1, E7, E9   | TBD          | placeholder |
| E12  | NPC, Reputation & Companions       | 5 — Contenu & Avancés        | E3, E4, E11  | TBD          | placeholder |
| E13  | Character Customization            | 5 — Contenu & Avancés        | E6           | TBD          | placeholder |
| E14  | Economy                            | 5 — Contenu & Avancés        | E8, E12      | TBD          | placeholder |
| E15  | Dungeons & Raids                   | 5 — Contenu & Avancés        | E2, E4, E12  | TBD          | placeholder |
| E16  | Localisation                       | 6 — Polish & Finalisation    | E5, E9, E10  | TBD          | placeholder |
| E17  | Art, Audio & Polish                | 6 — Polish & Finalisation    | All          | TBD          | placeholder |

## Recommended Sequence

### Phase 1 — Fondations Techniques (E1 → E2 → E3)

Le squelette technique : DI, patterns, scene management, player controller, entity framework. Tout le reste repose sur cette base. Même en état minimal, ces systèmes doivent exister avant de construire des features.

### Phase 2 — Gameplay Core (E4, E5, E6)

Le combat, l'UI et la progression forment le cœur jouable. E5 (UI) peut avancer en parallèle de E4 (Combat). E6 (Progression) dépend du combat pour les récompenses XP.

### Phase 3 — Persistance (E7, E8)

Le save system est critique pour tester tout le reste. L'inventaire dépend de l'UI et des entités.

### Phase 4 — Systèmes Narratifs (E9, E10, E11)

Quêtes d'abord, puis dialogue (qui s'intègre aux quêtes), puis world state (qui réagit aux deux).

### Phase 5 — Contenu & Systèmes Avancés (E12, E13, E14, E15)

NPC/Reputation avant Economy (les marchands en dépendent). Character Customization indépendant. Dungeons en dernier (combine zones + combat + NPC).

### Phase 6 — Polish & Finalisation (E16, E17)

Localisation après que tout le texte joueur existe. Art/Audio en dernier pour polish final.

### Vertical Slice

**Premier milestone jouable :** E1 + E2 + E3 + E4 + E5 (partiellement) — Un joueur qui se déplace dans une zone, combat des ennemis avec des abilities hotbar, voit son HUD, et peut passer d'une zone à l'autre.

---

## Epic 1: Core Infrastructure & Bootstrap

### Goal

Mettre en place le squelette technique sur lequel tout le reste repose : DI, patterns de base, interfaces core, et scène de bootstrap.

### Scope

**Includes:**
- Init(args) DI setup et `GameManager` global `[Service]`
- Core patterns : `StateMachine<T>`, `ObjectPool<T>`, `ICommand`
- `GameLogger` avec tags système et niveaux (ERROR/WARN/INFO/DEBUG)
- `GameConstants` (MAX_LEVEL, INVENTORY_SLOTS, etc.)
- Toutes les interfaces service en stubs (`ICombatSystem`, `IQuestManager`, etc.)
- Assembly Definitions par domaine (Core, Player, Combat, Quest, World, NPC, Inventory, Progression, UI, Audio, Save)
- Structure de dossiers `Assets/Scripts/` complète
- Event data structs de base et interfaces d'événements (`ICombatEvents`, `IWorldStateEvents`, `IQuestEvents`)
- Bootstrap scene (application lifecycle)
- Debug console basique (tilde `~`) et performance overlay (`F3`)

**Excludes:**
- Toute implémentation de gameplay (combat, quêtes, etc.)
- Assets visuels ou audio
- UI joueur (HUD, menus)
- Contenu de jeu (items, abilities, ennemis)

### Dependencies

Aucune — c'est la fondation.

### Deliverable

Un projet Unity qui boot proprement via une bootstrap scene, avec le DI Init(args) fonctionnel, tous les patterns de base disponibles, et les interfaces service prêtes à être implémentées par les epics suivantes.

### Stories

**Story 1.1: Project Bootstrap & DI Setup**
Mettre en place Init(args), le `GameManager` global `[Service]`, et la scène de bootstrap qui initialise le jeu.

**Story 1.2: Core Patterns Implementation**
Implémenter `StateMachine<T>`, `ObjectPool<T>`, `ICommand`, `GameLogger`, `GameConstants` dans `Scripts/Core/`.

**Story 1.3: Core Interfaces Scaffolding**
Créer toutes les interfaces service en stubs dans `Scripts/Core/Interfaces/` : `ICombatSystem`, `IQuestManager`, `IWorldStateTracker`, `IZoneManager`, `IAudioManager`, `IInventoryManager`, `IProgressionSystem`, `IUIService`, `INpcManager`, `IReputationSystem`, `IEconomyService`, `ILocalisationService`, `ISaveSystem`, `IDialogueSystem`.

**Story 1.4: Assembly Definitions & Project Structure**
Créer la structure de dossiers `Assets/Scripts/` complète (Core/, Player/, Combat/, Quest/, World/, NPC/, Inventory/, Progression/, UI/, Audio/, Save/) avec les Assembly Definitions pour isoler les domaines.

**Story 1.5: Event System Foundation**
Implémenter les event data structs de base dans `Scripts/Core/Events/` et les interfaces d'événements (`ICombatEvents`, `IWorldStateEvents`, `IQuestEvents`) — le pattern Observer via DI.

**Story 1.6: Debug Tools Foundation**
`GameLogger` avec tags `[System]`, niveaux (ERROR/WARN/INFO/DEBUG), debug console basique (tilde `~`), performance overlay (`F3`).

---

## Epic 2: Scene Management & Zone System

### Goal

Gérer le chargement/déchargement des zones, les transitions, et le pipeline Addressables pour que le joueur puisse naviguer entre les zones du monde.

### Scope

**Includes:**
- Addressables configuration (Core persistent group + per-zone groups)
- Labels par zone (`zone_village_001`, `zone_forest_001`, etc.)
- `IAssetLoader` service implementation
- `IZoneManager` implémentation avec chargement async (additive scenes)
- Déchargement de zones et cleanup des handles Addressables
- Zone transitions (triggers, portails, spawn points destination)
- Gestion de l'état joueur pendant le chargement
- Loading screen avec barre de progression
- Infrastructure fast travel (découverte de points, téléportation)

**Excludes:**
- Contenu des zones (level design, placement NPCs)
- Fast travel UI complète (placeholder seulement)
- World state tracking (E11)
- Zone instanciation pour donjons (E15)

### Dependencies

- E1: Core Infrastructure & Bootstrap (DI, patterns, interfaces)

### Deliverable

Le joueur peut passer d'une zone à une autre via des portails/triggers, avec un loading screen, et les assets se chargent/déchargent proprement via Addressables. Le fast travel est prêt à recevoir du contenu.

### Stories

**Story 2.1: Addressables Setup & Asset Pipeline**
Configurer les Addressables groups (Core persistent + per-zone), labels par zone, `IAssetLoader` service implementation.

**Story 2.2: Zone Loading System**
`IZoneManager` implémentation — chargement async de scènes (additive), déchargement de la zone précédente, cleanup mémoire des handles Addressables.

**Story 2.3: Zone Transitions**
Système de transition entre zones (triggers, portails), passage de données entre scènes (spawn point destination), gestion de l'état joueur pendant le chargement.

**Story 2.4: Loading Screen**
UI de chargement avec barre de progression, affichée pendant les transitions de zone. Target : < 5s max, < 3s cible.

**Story 2.5: Fast Travel Foundation**
Infrastructure pour le fast travel (découverte de points, téléportation via `IZoneManager`), UI placeholder. Le contenu viendra plus tard.

---

## Epic 3: Player Controller & Entity Framework

### Goal

Le joueur se déplace dans le monde, la caméra suit, les entités (NPC, ennemis) existent et sont gérées selon le pattern TrinityCore.

### Scope

**Includes:**
- Player controller WASD (vitesse, rotation, collision PhysX)
- Animation states placeholder (idle, walk, run)
- Top-down camera follow (offset, zoom molette, smooth)
- Input System action maps (Movement, Combat, UI, Menu)
- `IInputManager` service implementation
- Entity base framework (`EntityBase` avec stats, state machine, DI)
- `INpcManager` implémentation TrinityCore (register, activate/deactivate, `EvaluateAll()`)
- `NpcInstance` component, `NpcDataSO`, `SpawnCondition[]`
- `IInteractionSystem` (détection proximité, prompt UI, dispatch)

**Excludes:**
- Combat (ciblage, abilities, dégâts — E4)
- Animations finales (E17)
- NPC dialogue et quêtes (E9, E10)
- Companion AI (E12)
- Rebinding UI (E5)

### Dependencies

- E1: Core Infrastructure & Bootstrap (DI, patterns, interfaces, StateMachine<T>)
- E2: Scene Management & Zone System (zones pour tester le player dans un environnement)

### Deliverable

Un joueur qui marche (WASD) dans une zone avec une caméra top-down qui suit, des entités NPC/ennemis placées dans la scène qui s'activent/désactivent via `INpcManager`, et un système d'interaction basique (approcher + touche pour interagir).

### Stories

**Story 3.1: Player Movement**
Player controller WASD, vitesse de déplacement, rotation vers la direction, collision PhysX. Animation states placeholder (idle, walk, run).

**Story 3.2: Top-Down Camera**
Caméra follow avec offset configurable, zoom (molette), rotation optionnelle. Smooth follow via PrimeTween ou lerp.

**Story 3.3: Input System Setup**
Action maps (Movement, Combat, UI, Menu), rebinding scaffold, `IInputManager` service implémentation. Keyboard + Mouse uniquement.

**Story 3.4: Entity Base Framework**
Classes de base pour les entités du jeu (player, NPCs, ennemis). `EntityBase` avec stats, state machine, références DI. Préparation pour le combat (E4) et les NPCs (E12).

**Story 3.5: NPC Manager (TrinityCore Pattern)**
`INpcManager` implémentation — `NpcInstance` component, `NpcDataSO`, `SpawnCondition[]`, register/unregister, `EvaluateAll()`, activation/désactivation basée sur conditions.

**Story 3.6: Interaction System**
`IInteractionSystem` — détection d'interactables à proximité, prompt UI (touche d'interaction), dispatch vers le système concerné (dialogue, marchand, coffre, etc.). Base extensible.

---

## Epic 4: Combat System

### Goal

Le combat fonctionne : ciblage, abilities hotbar, dégâts, status effects, IA ennemie basique.

### Scope

**Includes:**
- Targeting (click-to-target, tab-cycle)
- Hotbar abilities (1-9, cooldowns, mana/stamina)
- Damage system (formulas, strategies physical/magical/hybrid)
- Status effects (buffs, debuffs)
- Enemy AI (HSM basique)
- Death & respawn

**Excludes:**
- Companion AI (E12)
- Boss mechanics (E15)
- Ability balancing (E14)
- VFX finaux (E17)

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E3: Player Controller & Entity Framework

### Deliverable

Le joueur peut cibler des ennemis, utiliser des abilities via la hotbar, infliger et recevoir des dégâts, avec des ennemis qui combattent via une IA basique.

### Stories

_À définir_

---

## Epic 5: UI/HUD Foundation

### Goal

L'interface de jeu est fonctionnelle : HUD, menus, tooltips, notifications.

### Scope

**Includes:**
- HUD (health, mana, XP bar, minimap placeholder)
- Hotbar UI
- Main menu, pause menu, settings
- Tooltip framework
- Notification/popup system
- Quest tracker placeholder

**Excludes:**
- Inventory UI (E8)
- Quest log UI (E9)
- Dialogue UI (E10)
- Character sheet complet (E6)

### Dependencies

- E1: Core Infrastructure & Bootstrap

### Deliverable

Le joueur voit son HUD en jeu, peut naviguer dans les menus, et l'infrastructure tooltip/notification est prête pour les autres epics.

### Stories

_À définir_

---

## Epic 6: Character Progression

### Goal

Le joueur progresse : stats, XP, leveling, talent points.

### Scope

**Includes:**
- Stats (Endurance, Intelligence, Strength)
- XP & leveling (1-60, courbe logarithmique)
- Distribution de talent points
- Character sheet UI
- Equipment stat effects sur le personnage

**Excludes:**
- Talent trees complets (races, spécialisations — E13)
- Balance pass final (E14)

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E3: Player Controller & Entity Framework
- E4: Combat System (XP rewards from combat)

### Deliverable

Le joueur gagne de l'XP en combat, monte de niveau, distribue des points, et voit ses stats évoluer dans un character sheet.

### Stories

_À définir_

---

## Epic 7: Save System

### Goal

Le jeu se sauvegarde et se charge correctement, avec toute la progression joueur et l'état du monde.

### Scope

**Includes:**
- MemoryPack integration
- `SaveData` architecture (PlayerSaveData, WorldStateSaveData, QuestSaveData, ReputationSaveData)
- Save/load manuel
- Auto-save aux moments clés
- Schema versioning (`[MemoryPackable(GenerateType.VersionTolerant)]`)
- Save slot management
- Async save/load via UniTask

**Excludes:**
- Cloud saves (pas en v1.0)
- Save file encryption

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E3: Player Controller & Entity Framework

### Deliverable

Le joueur peut sauvegarder/charger sa progression (position, stats, inventaire, quêtes, world state) avec auto-save et multiple slots.

### Stories

_À définir_

---

## Epic 8: Inventory & Equipment

### Goal

Le joueur gère son inventaire, équipe des objets, et ramasse du loot.

### Scope

**Includes:**
- Inventaire slot-based
- Equipment system (weapon, armor, accessories)
- Item ScriptableObjects (5 rarity tiers)
- Loot system (drops, pickup)
- Inventory & equipment UI
- Item tooltips

**Excludes:**
- Crafting (pas en v1.0)
- Merchant buy/sell (E14)
- Loot tables de donjons (E15)

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E3: Player Controller & Entity Framework
- E5: UI/HUD Foundation (tooltip framework)

### Deliverable

Le joueur ramasse des objets, les range dans son inventaire, équipe armes/armures, et voit les stats changer.

### Stories

_À définir_

---

## Epic 9: Quest System

### Goal

Le système de quêtes fonctionne : accepter, tracker, compléter des quêtes avec des objectifs variés.

### Scope

**Includes:**
- Quest state machine
- Journal de quêtes & tracking
- Objectifs (kill, fetch, interact, talk)
- Rewards (XP, gold, items, reputation)
- Branching quest paths
- Quest UI (log, tracker)

**Excludes:**
- Dialogue branching (E10)
- World state consequences (E11)
- Contenu narratif (E17)

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E3: Player Controller & Entity Framework
- E5: UI/HUD Foundation

### Deliverable

Le joueur peut accepter des quêtes, suivre ses objectifs, et les compléter pour recevoir des récompenses.

### Stories

_À définir_

---

## Epic 10: Dialogue System

### Goal

Les NPC parlent : dialogues avec choix, mémoire par NPC, intégration avec les quêtes.

### Scope

**Includes:**
- Choix de technologie (Yarn Spinner / Ink / Custom)
- Dialogue trees avec branching
- NPC memory per-interaction
- Dialogue UI (choix, affichage texte)
- Intégration avec le quest system

**Excludes:**
- Voice acting (hors scope v1.0)
- Consequence propagation (E11)
- Contenu narratif complet (E17)

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E5: UI/HUD Foundation
- E9: Quest System

### Deliverable

Le joueur peut discuter avec des NPC via des dialogue trees, faire des choix qui affectent les quêtes, et les NPC se souviennent des interactions passées.

### Stories

_À définir_

---

## Epic 11: World State & Consequences

### Goal

Le monde réagit aux choix du joueur : état persistant, conséquences en cascade, mutations visibles.

### Scope

**Includes:**
- `IWorldStateTracker` implémentation complète
- Architecture choice (event-sourced / snapshot / hybrid)
- Consequence propagation (Act 1 → Act 3)
- Persistent world mutations (conquêtes, alliances, contrôle de faction)
- NPC spawn condition evaluation (intégration `INpcManager`)
- Intégration save/quest/dialogue

**Excludes:**
- Contenu narratif spécifique (E17)
- NPC AI avancée (E12)

### Dependencies

- E1: Core Infrastructure & Bootstrap
- E7: Save System (persistence du world state)
- E9: Quest System (quêtes déclenchent des changements)

### Deliverable

Les choix du joueur modifient le monde de manière persistante — villages conquis, alliances formées, NPC qui apparaissent/disparaissent selon l'état du monde. Tout est sauvegardé.

### Stories

_À définir_

---

## Epic 12: NPC, Reputation & Companions

### Goal

Systèmes sociaux : réputation par faction/NPC, marchands, compagnons recrutables avec IA de combat.

### Scope

**Includes:**
- Reputation system (faction + individuel)
- Merchant system (buy/sell, regional stock, commissions)
- Companion recruitment & management
- Companion AI (Tank/Healer/DPS strategies + Command pattern)
- Player orders (AttackTarget, HoldPosition, Follow)

**Excludes:**
- Behavior Designer migration (deferred)
- Companion dialogue complet (E10)
- Boss-specific companion tactics (E15)

### Dependencies

- E3: Player Controller & Entity Framework
- E4: Combat System
- E11: World State & Consequences

### Deliverable

Le joueur a une réputation avec les factions, peut acheter/vendre chez les marchands, recruter des compagnons qui combattent avec des rôles Tank/Healer/DPS.

### Stories

_À définir_

---

## Epic 13: Character Customization

### Goal

Races, spécialisations, et talent trees complets pour la personnalisation du personnage.

### Scope

**Includes:**
- Race system (talent trees raciaux)
- Specialization system (choix mid-game)
- Dual talent trees (racial + specialization)
- Specialization UI

**Excludes:**
- Balance final des talents (intégré dans le balance pass)
- Cosmetic customization (hors scope v1.0)

### Dependencies

- E6: Character Progression

### Deliverable

Le joueur choisit une race avec un arbre de talents racial, et sélectionne une spécialisation mid-game avec son propre arbre de talents.

### Stories

_À définir_

---

## Epic 14: Economy

### Goal

Système économique fonctionnel : gold, marchands régionaux, commissions, balance.

### Scope

**Includes:**
- Gold system (earn/spend)
- Regional economy (prix variables par zone)
- Commission system
- Balance pass (XP curve, difficulty, loot tables, gold economy)

**Excludes:**
- Auction house / trading (hors scope v1.0)
- Crafting economy

### Dependencies

- E8: Inventory & Equipment
- E12: NPC, Reputation & Companions (merchants)

### Deliverable

L'économie du jeu fonctionne : le joueur gagne et dépense de l'or, les marchands ont du stock régional, et la progression est équilibrée.

### Stories

_À définir_

---

## Epic 15: Dungeons & Raids

### Goal

Contenu end-game : zones instanciées, boss encounters avec mécaniques, raids.

### Scope

**Includes:**
- Zones instanciées (entrée/sortie donjon)
- Boss encounters (phases, mécaniques spéciales)
- Companion AI avancée pour donjons
- Loot tables spécifiques
- Raid encounters (plus gros, plus complexe)

**Excludes:**
- Multiplayer raids (hors scope — single-player only)
- Procedural generation

### Dependencies

- E2: Scene Management & Zone System
- E4: Combat System
- E12: NPC, Reputation & Companions

### Deliverable

Au moins un donjon jouable avec boss multi-phases, loot spécifique, et compagnons IA qui assistent le joueur.

### Stories

_À définir_

---

## Epic 16: Localisation

### Goal

Le jeu est bilingue français + anglais, avec un système de localisation extensible.

### Scope

**Includes:**
- `ILocalisationService` implémentation
- String key resolution system (`{domain}.{context}.{id}`)
- Translation files (JSON ou CSV)
- French + English
- UI text component integration
- Language selection & persistence (PlayerPrefs)

**Excludes:**
- Autres langues (post-v1.0)
- Voice-over localisation

### Dependencies

- E5: UI/HUD Foundation
- E9: Quest System
- E10: Dialogue System

### Deliverable

Tout le texte joueur est localisé via string keys, le joueur peut switcher entre français et anglais dans les settings.

### Stories

_À définir_

---

## Epic 17: Art, Audio & Polish

### Goal

Le jeu est visuellement et auditivement complet : assets 3D finaux, VFX, animations, musique, SFX.

### Scope

**Includes:**
- 3D assets finaux (characters, environment, props)
- Materials & textures
- VFX (abilities, environnement, UI)
- Animations (player, enemies, NPCs)
- `IAudioManager` implémentation
- Music system (tracks, crossfade)
- SFX (combat, UI, ambient)
- Visual polish pass global

**Excludes:**
- Gameplay changes
- New features

### Dependencies

- Toutes les epics précédentes (polish final)

### Deliverable

Le jeu est visuellement et auditivement complet, avec tous les assets finaux, VFX, animations et audio en place.

### Stories

_À définir_
