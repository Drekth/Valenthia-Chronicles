# Init(args) — Guide d'Utilisation pour Valenthia Chronicles

**Source:** https://docs.sisus.co/init-args/

## Vue d'Ensemble

Init(args) est le framework d'injection de dépendances (DI) du projet. Il remplace les singletons et les `FindObjectOfType` par un système type-safe, performant (3ns par injection, sans reflection) et intégré à l'Inspector Unity.

**Principe fondamental :** Les composants ne récupèrent jamais leurs dépendances eux-mêmes. Elles leur sont fournies via la fonction `Init()`.

---

## 1. Services Globaux — `[Service]`

Les services globaux sont disponibles pour tous les clients pendant toute la durée de vie de l'application. Ils sont initialisés **avant le chargement de la première scène**, dans l'ordre optimal basé sur leurs dépendances.

### Déclaration

```csharp
using Sisus.Init;

[Service]
public class GameManager { }
```

### Injection service-à-service

Via constructeur :
```csharp
[Service]
public class AudioManager
{
    public AudioManager(GameManager gameManager)
        => Debug.Log($"AudioManager received {gameManager}.");
}
```

Via `IInitializable<T...>` (supporte les dépendances circulaires, contrairement aux constructeurs) :
```csharp
[Service]
public class InputManager : IInitializable<GameManager>
{
    public void Init(GameManager gameManager)
        => Debug.Log($"InputManager received {gameManager}.");
}
```

### Règles importantes

- **JAMAIS** de dépendances circulaires via constructeurs (A→B→A = impossible à construire)
- `IInitializable<T...>` ne souffre PAS de cette limitation
- Les services globaux doivent vivre pour toute la durée de l'application — ne jamais les détruire

### Services globaux recommandés pour Valenthia Chronicles

| Service | Interface | Rôle |
|---|---|---|
| `GameManager` | — | État global, transitions de jeu |
| `InputManager` | `IInputManager` | Abstraction du New Input System |
| `CombatSystem` | `ICombatSystem` | Logique de combat, calculs de dégâts |
| `QuestManager` | `IQuestManager` | Suivi des quêtes, branches narratives |
| `DialogueManager` | `IDialogueManager` | Système de dialogue, choix joueur |
| `WorldStateTracker` | `IWorldStateTracker` | Conséquences, état persistant du monde |
| `ReputationSystem` | `IReputationSystem` | Réputation joueur/factions/NPCs |
| `SaveSystem` | `ISaveSystem` | Sauvegarde et chargement |
| `InventoryManager` | `IInventoryManager` | Inventaire du joueur |
| `AudioManager` | `IAudioManager` | Musique et effets sonores |
| `ProgressionSystem` | `IProgressionSystem` | XP, leveling, talent points |

---

## 2. Services Locaux — Service Tags

Les services locaux vivent dans une scène ou un prefab et ne sont disponibles que pour les clients dans leur scope.

### Configuration

1. Sélectionner un composant dans l'Inspector
2. Clic droit → **Make Service Of Type...**
3. Choisir le type définissant du service
4. Configurer l'availability via **Set Availability...**

### Niveaux d'availability

| Scope | Description | Cas d'usage Valenthia |
|---|---|---|
| **In GameObject** | Même GameObject uniquement | Composants internes d'un prefab |
| **In Children** | GameObject + enfants | Merchant avec sous-systèmes |
| **In Parents** | GameObject + parents | Composant enfant accédant au parent |
| **In Hierarchy Root Children** | Racine de hiérarchie + enfants | Prefab complexe multi-niveaux |
| **In Scene** | Toute la scène | ZoneController, EnemySpawner |
| **Everywhere** | Global (comme [Service]) | Alternative Inspector au [Service] |

### Services locaux recommandés pour Valenthia Chronicles

| Service | Scope | Rôle |
|---|---|---|
| `ZoneController` | In Scene | Gestion de la zone courante (ennemis, POIs, état) |
| `EnemySpawner` | In Scene | Spawning d'ennemis spécifique à la zone |
| `MerchantService` | In Children | Inventaire et prix d'un marchand spécifique |
| `DungeonManager` | In Scene | Logique de donjon (boss, progression, loot) |
| `TownServices` | In Children | Services disponibles dans une ville (forgerons, auberges) |
| `DialogueContext` | In Scene | Contexte narratif local à la scène |

---

## 3. Clients — `MonoBehaviour<T...>`

Le pattern principal pour recevoir des dépendances dans un composant.

### Pattern de base

```csharp
using Sisus.Init;
using UnityEngine;

public class Player : MonoBehaviour<IInputManager, ICombatSystem>
{
    IInputManager inputManager;
    ICombatSystem combatSystem;

    protected override void Init(IInputManager inputManager, ICombatSystem combatSystem)
    {
        this.inputManager = inputManager;
        this.combatSystem = combatSystem;
    }

    // Utiliser OnAwake au lieu de Awake
    protected override void OnAwake()
    {
        Debug.Log($"Player initialized with {inputManager} and {combatSystem}");
    }

    // OnEnable, Start, Update etc. fonctionnent normalement
    void Update()
    {
        var moveInput = inputManager.MoveInput;
        // ...
    }
}
```

### Règles critiques pour les agents

- **`Init()` est appelé AVANT `Awake`/`OnEnable`** — les dépendances sont toujours disponibles
- **Utiliser `OnAwake()` au lieu de `Awake()`** — le MonoBehaviour<T> définit déjà Awake
- **Utiliser `OnReset()` au lieu de `Reset()`** — même raison
- **Jusqu'à 12 arguments** supportés via les génériques
- **Initialisation asynchrone :** si un service dépendant n'est pas encore disponible, les événements lifecycle (Awake, OnEnable, Start) sont **différés automatiquement** jusqu'à ce qu'il le soit
- Désactiver avec `[Init(WaitForServices = false)]` si nécessaire

### Résolution automatique des arguments Init

Quand un client `MonoBehaviour<T...>` est chargé, chaque argument est résolu dans cet ordre :
1. Arguments passés via code (`AddComponent`, `Instantiate`)
2. Services globaux `[Service]` correspondants
3. Services locaux (Service Tags) correspondants
4. Initializer attaché au composant

---

## 4. Instanciation en Code

### AddComponent avec arguments

```csharp
using Sisus.Init;

// Explicite
gameObject.AddComponent<Player, IInputManager, Camera>(inputManager, Camera.main);

// Avec inférence de types via out
gameObject.AddComponent(out Player player, inputManager, Camera.main);
```

### Instantiate avec arguments

```csharp
using Sisus.Init;

// Cloner un prefab avec injection
Player player = playerPrefab.Instantiate(inputManager, Camera.main);
```

### new GameObject avec arguments

```csharp
using Sisus.Init;

// Créer un GameObject avec composant initialisé
var go = new GameObject<Player>("Player", inputManager, Camera.main);
```

---

## 5. ScriptableObject<T...>

Pour les ScriptableObjects qui ont besoin de dépendances.

```csharp
using Sisus.Init;
using UnityEngine;

public class GameEvent : ScriptableObject<string, GameObject>
{
    public string Id { get; private set; }
    public GameObject Target { get; private set; }

    protected override void Init(string id, GameObject target)
    {
        Id = id;
        Target = target;
    }

    // Utiliser OnAwake au lieu de Awake
    // Utiliser OnReset au lieu de Reset
}

// Création d'instance
var evt = Create.Instance<GameEvent, string, GameObject>("quest_complete", targetGO);
```

---

## 6. Interfaces — Support Natif

Init(args) supporte les interfaces comme types de service, contrairement aux champs sérialisés Unity standard.

```csharp
// Le service est enregistré par son interface
[Service(typeof(IInputManager))]
public class InputManager : IInputManager { }

// Le client reçoit l'interface
public class Player : MonoBehaviour<IInputManager>
{
    IInputManager inputManager;
    protected override void Init(IInputManager inputManager)
        => this.inputManager = inputManager;
}
```

### Avantages pour Valenthia Chronicles

- **Testabilité :** Injecter des mocks dans les tests unitaires
- **Flexibilité :** Swapper `PCInputManager` / `MobileInputManager` sans toucher aux clients
- **Extensibilité :** Ajouter de nouvelles implémentations sans modifier le code existant
- **Open-source future :** Les moddeurs peuvent fournir des implémentations alternatives

---

## 7. Wrappers — Plain C# Classes dans Unity

Pour les systèmes de données qui sont des classes C# pures (pas des MonoBehaviour).

```csharp
using System;
using UnityEngine;

[Serializable]
public class WorldState
{
    [SerializeField] private string regionId;
    [SerializeField] private FactionControl factionControl;

    public WorldState(string regionId, FactionControl factionControl)
    {
        this.regionId = regionId;
        this.factionControl = factionControl;
    }
}

// Le Wrapper auto-généré ou créé manuellement
[AddComponentMenu("Wrapper/WorldState")]
public class WorldStateComponent : Wrapper<WorldState> { }
```

### Cas d'usage Valenthia Chronicles

- `WorldState` — État persistant du monde (villages conquis, alliances)
- `ConsequenceTracker` — Suivi des choix narratifs (Mass Effect-style)
- `CharacterBuild` — Stats, talents, équipement du personnage
- `QuestState` — État d'avancement des quêtes

---

## 8. Cross-Scene References

Essentielles pour l'architecture par zones de Valenthia Chronicles.

### Usage

1. Ouvrir plusieurs scènes dans l'éditeur
2. Glisser-déposer un objet d'une scène dans un champ Init d'un composant d'une autre scène
3. Init(args) génère automatiquement un Cross-Scene Id
4. La référence est résolue automatiquement au runtime

### Any<T> pour les références cross-scene dans les champs

```csharp
using Sisus.Init;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] Any<Waypoint> previous; // Peut être dans une autre scène
    [SerializeField] Any<Waypoint> next;

    public Waypoint Previous => previous.Value;
    public Waypoint Next => next.Value;
}
```

### Règles pour le système de zones

- Charger la scène contenant l'objet référencé **AVANT** la scène du client
- Si le client est chargé avant sa dépendance, il est automatiquement désactivé jusqu'à résolution
- Utiliser uniquement quand la scène référencée reste chargée plus longtemps que la scène du client

---

## 9. Initializers — Configuration Instance-Spécifique

Quand un composant a besoin de valeurs spécifiques (pas des services globaux) :

### Quand utiliser

- Arguments spécifiques par instance (pas un service partagé)
- Remplacer un service global par un objet différent pour une instance
- Cross-scene references
- Prefab instance references
- Value providers pour résolution dynamique
- Validation Edit Mode des arguments null

### Création

1. Composant dérivant de `MonoBehaviour<T...>` → Section Init visible dans Inspector
2. Cliquer sur **+** → **Generate Initializer**
3. Configurer les arguments dans l'Inspector

---

## 10. Null Argument Guard

Détection automatique des dépendances manquantes.

### Niveaux de protection

| Niveau | Quand | Action |
|---|---|---|
| Inspector Warnings | Edit Mode, Inspector | Champs surlignés en jaune |
| Edit Mode Warnings | Edit Mode, Console | Log de warning |
| Runtime Exceptions | Play Mode | Exception à l'init |

### Configuration

```csharp
// Désactiver les warnings Edit Mode si nécessaire
[Init(NullArgumentGuard = NullArgumentGuard.RuntimeException)]
public class Player : MonoBehaviour<IInputManager, ICombatSystem> { }
```

---

## 11. Find API — Utilitaire de Recherche

Alternative type-safe aux `FindObjectOfType` avec support interfaces.

```csharp
using Sisus.Init;

// Trouver par interface
Find.Any<IInputManager>();
Find.All<IEnemy>();

// Trouver dans la hiérarchie
Find.InParents<IHealthSystem>(gameObject);
Find.InChildren<IInteractable>(gameObject);

// Trouver par tag
Find.WithTag("MainCamera");
```

---

## 12. `[InitOnReset]` — Auto-Init en Edit Mode

Quand un composant est ajouté à un GameObject en éditeur, ses dépendances sont automatiquement recherchées et injectées.

```csharp
[InitOnReset]
public class Player : MonoBehaviour<Collider, Animator>
{
    [SerializeField] Collider collider;
    [SerializeField] Animator animator;

    protected override void Init(Collider collider, Animator animator)
    {
        this.collider = collider;
        this.animator = animator;
    }
}
```

**Ordre de recherche :** Même GameObject → Enfants → Parents → Scène entière.

Personnalisable :
```csharp
[InitOnReset(From.Children, From.GetOrAddComponent, From.Scene)]
public class Player : MonoBehaviour<Collider, Animator, Camera> { }
```

---

## Anti-Patterns à Éviter

| Anti-Pattern | Alternative Init(args) |
|---|---|
| `Singleton.Instance` | `[Service]` + `MonoBehaviour<T>` |
| `FindObjectOfType<T>()` | Service injection ou `Find.Any<T>()` |
| `GetComponent<T>()` dans Awake pour des dépendances externes | `MonoBehaviour<T>` avec Init |
| Champs `public` pour l'Inspector | `[SerializeField]` privés ou Init arguments |
| Execution order via Script Execution Order | Services globaux auto-ordonnés |
| Coroutine pour attendre une dépendance | Async initialization automatique |

---

## Checklist Agent IA — Avant Chaque Implémentation

1. **Le composant a-t-il des dépendances externes ?** → Hériter de `MonoBehaviour<T...>`
2. **La dépendance est-elle un système global ?** → `[Service]` sur la classe
3. **La dépendance est-elle locale à une scène ?** → Service Tag
4. **Le type est-il une interface ?** → Support natif, toujours préférer les interfaces
5. **C'est une classe C# pure ?** → Utiliser un Wrapper si elle doit vivre sur un GameObject
6. **Références entre scènes ?** → Cross-Scene References ou `Any<T>`
7. **Override `OnAwake()` PAS `Awake()`**
8. **Override `OnReset()` PAS `Reset()`**
9. **Namespace `using Sisus.Init;`** toujours nécessaire
