using System.Collections.Generic;
using UnityEngine;

// The set of spells the player knows. Authored on the persistent player rig and published through
// the ServiceLocator so the spellbook window can list them without scene lookups — like
// PlayerInventory does for the bag. Lives on the persistent rig (not the per-zone body) so the
// known set survives zone swaps. Dynamic learning (quests, level-ups) is out of scope for now: the
// list is filled in the Inspector.
public class PlayerSpellbook : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public IReadOnlyList<SpellData> KnownSpells => Spells;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<PlayerSpellbook>(this);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryGet<PlayerSpellbook>(out PlayerSpellbook Current) && Current == this)
        {
            ServiceLocator.Unregister<PlayerSpellbook>();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private List<SpellData> Spells = new List<SpellData>();
}
