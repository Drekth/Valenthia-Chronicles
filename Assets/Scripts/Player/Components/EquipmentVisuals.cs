using System;
using System.Collections.Generic;
using UnityEngine;

// Shows the player's worn gear on the body. Lives on the player rig next to the model and maps
// each equipment slot to a bone socket (the rig already exposes hand sockets). Whenever the
// equipment changes it reconciles the spawned models against the Equipment model — resolved
// through the ServiceLocator — instantiating the worn item's WorldModel under its socket and
// destroying the one that is no longer worn. It never drives equips itself; it only reacts to
// the shared changed channel, so no per-frame polling.
public class EquipmentVisuals : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Subscribe(HandleEquipmentChanged);
        }

        // Show whatever is already worn when the rig comes alive.
        Refresh();
    }

    private void OnDisable()
    {
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Unsubscribe(HandleEquipmentChanged);
        }

        ClearSpawned();
    }

    private void HandleEquipmentChanged()
    {
        Refresh();
    }

    // Brings the attached models in line with the Equipment model: for every socket-bound slot,
    // spawns the worn item's WorldModel when it differs from what is currently shown and removes
    // the stale one. Slots whose item carries no WorldModel simply end up empty.
    private void Refresh()
    {
        if (!ServiceLocator.TryGet<Equipment>(out Equipment Gear))
        {
            return;
        }

        for (int I = 0; I < Sockets.Length; I++)
        {
            SocketBinding Binding = Sockets[I];
            if (Binding.Socket == null)
            {
                continue;
            }

            ItemData Worn = Gear.Get(Binding.Slot);
            GameObject Desired = Worn != null ? Worn.WorldModel : null;

            Spawned.TryGetValue(Binding.Slot, out SpawnedModel Current);
            if (Current.Source == Desired)
            {
                continue;
            }

            if (Current.Instance != null)
            {
                Destroy(Current.Instance);
            }
            Spawned.Remove(Binding.Slot);

            if (Desired == null)
            {
                continue;
            }

            GameObject Instance = Instantiate(Desired, Binding.Socket);
            Instance.transform.localPosition = Worn.AttachPosition;
            Instance.transform.localRotation = Quaternion.Euler(Worn.AttachEuler);
            Spawned[Binding.Slot] = new SpawnedModel(Desired, Instance);
        }
    }

    private void ClearSpawned()
    {
        foreach (KeyValuePair<EquipmentSlot, SpawnedModel> Pair in Spawned)
        {
            if (Pair.Value.Instance != null)
            {
                Destroy(Pair.Value.Instance);
            }
        }

        Spawned.Clear();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Events")]
    [SerializeField] private VoidEventChannel OnEquipmentChanged;

    [Header("Sockets")]
    [SerializeField] private SocketBinding[] Sockets = Array.Empty<SocketBinding>();

    // Live models keyed by slot, with the prefab each was spawned from so a change can tell
    // whether the worn item actually differs before rebuilding.
    private readonly Dictionary<EquipmentSlot, SpawnedModel> Spawned = new Dictionary<EquipmentSlot, SpawnedModel>();

    ////////////////////////////////////////////////////////////
    /// Types                                                ///
    ////////////////////////////////////////////////////////////

    // Binds an equipment slot to the bone socket its worn model hangs from (e.g. MainHand to the
    // right-hand bone).
    [Serializable]
    private struct SocketBinding
    {
        public EquipmentSlot Slot   => BoundSlot;
        public Transform     Socket => BoundSocket;

        [SerializeField] private EquipmentSlot BoundSlot;
        [SerializeField] private Transform BoundSocket;
    }

    // A spawned model paired with the prefab it came from.
    private readonly struct SpawnedModel
    {
        public SpawnedModel(GameObject Source, GameObject Instance)
        {
            this.Source = Source;
            this.Instance = Instance;
        }

        public GameObject Source   { get; }
        public GameObject Instance { get; }
    }
}
