using System;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Start()
    {
        foreach (SpawnEntry Entry in Entries)
        {
            SpawnCreature(Entry);
        }
    }

    private void SpawnCreature(SpawnEntry Entry)
    {
        if (Entry.Prefab == null || Entry.Point == null)
        {
            return;
        }

        Instantiate(Entry.Prefab, Entry.Point.position, Entry.Point.rotation);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private SpawnEntry[] Entries;

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
