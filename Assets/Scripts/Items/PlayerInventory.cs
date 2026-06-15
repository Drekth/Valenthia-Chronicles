using UnityEngine;

// Identity marker for the player's bag. Holds no storage logic of its own — it tags the
// Container that belongs to the player and publishes it through the ServiceLocator so UI
// and gameplay can reach the inventory without scene lookups. Lives on the persistent
// player rig so the bag survives zone swaps.
[RequireComponent(typeof(Container))]
public class PlayerInventory : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public Container Bag => OwnedBag;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        OwnedBag = GetComponent<Container>();
        ServiceLocator.Register<PlayerInventory>(this);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Current) && Current == this)
        {
            ServiceLocator.Unregister<PlayerInventory>();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private Container OwnedBag;
}
