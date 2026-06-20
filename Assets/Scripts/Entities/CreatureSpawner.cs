using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantiates creatures at designer-placed spawn points and handles the full lifecycle:
// initial spawn → death → despawn delay (lets the death animation play) → respawn delay → respawn.
// Creatures without a Unit component are spawned but not tracked for respawn.
public class CreatureSpawner : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        DiedBinding = new EventBinding<UnitDiedEvent>(HandleUnitDied);
        EventBus<UnitDiedEvent>.Register(DiedBinding);
    }

    private void OnDisable()
    {
        EventBus<UnitDiedEvent>.Deregister(DiedBinding);
    }

    private void Start()
    {
        foreach (SpawnEntry Entry in Entries)
        {
            SpawnCreature(Entry);
        }
    }

    private void HandleUnitDied(UnitDiedEvent Event)
    {
        if (!TrackedUnits.TryGetValue(Event.Unit, out SpawnEntry Entry))
        {
            return;
        }

        TrackedUnits.Remove(Event.Unit);
        StartCoroutine(DespawnAndRespawn(Event.Unit.gameObject, Entry));
    }

    private IEnumerator DespawnAndRespawn(GameObject Dead, SpawnEntry Entry)
    {
        yield return new WaitForSeconds(DespawnDelay);
        Destroy(Dead);
        yield return new WaitForSeconds(RespawnDelay);
        SpawnCreature(Entry);
    }

    private void SpawnCreature(SpawnEntry Entry)
    {
        if (Entry.Prefab == null || Entry.Point == null)
        {
            return;
        }

        GameObject Spawned = Instantiate(Entry.Prefab, Entry.Point.position, Entry.Point.rotation);

        Unit SpawnedUnit = Spawned.GetComponent<Unit>();
        if (SpawnedUnit != null)
        {
            TrackedUnits[SpawnedUnit] = Entry;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Timing")]
    [SerializeField] private float DespawnDelay = 2.0f;
    [SerializeField] private float RespawnDelay = 30.0f;

    [SerializeField] private SpawnEntry[] Entries;

    private EventBinding<UnitDiedEvent> DiedBinding;
    private readonly Dictionary<Unit, SpawnEntry> TrackedUnits = new Dictionary<Unit, SpawnEntry>();

    ////////////////////////////////////////////////////////////
    /// Types                                               ///
    ////////////////////////////////////////////////////////////

    [Serializable]
    private struct SpawnEntry
    {
        public GameObject Prefab;
        public Transform  Point;
    }
}
