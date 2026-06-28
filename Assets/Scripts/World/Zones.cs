using UnityEngine;

// Macro occlusion layer: a trigger volume covering a building's interior. When the player enters,
// the roof occluders fade out and an InteriorEnteredEvent is published; on exit they fade back and
// an InteriorExitedEvent is published. Pairs with the micro CameraOcclusionFader, which handles
// incidental occluders outside buildings.
//
// Multi-floor note: today this fades a flat list of roof occluders. To support walkable floors
// later, RoofOccluders becomes a per-floor array and only floors above the player's current floor
// are faded — the trigger/event contract here does not change, so callers stay untouched.
[RequireComponent(typeof(Collider))]
public class Zones : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Reset()
    {
        // Authoring convenience: a building zone is always a trigger.
        Collider Volume = GetComponent<Collider>();
        Volume.isTrigger = true;
    }

    private void OnTriggerEnter(Collider Other)
    {
        if (!IsPlayer(Other))
        {
            return;
        }

        SetRoofFaded(true);
        EventBus<InteriorEnteredEvent>.Raise(new InteriorEnteredEvent { BuildingId = BuildingId });
    }

    private void OnTriggerExit(Collider Other)
    {
        if (!IsPlayer(Other))
        {
            return;
        }

        SetRoofFaded(false);
        EventBus<InteriorExitedEvent>.Raise(new InteriorExitedEvent { BuildingId = BuildingId });
    }

    private bool IsPlayer(Collider Other)
    {
        return (PlayerMask.value & (1 << Other.gameObject.layer)) != 0;
    }

    private void SetRoofFaded(bool Faded)
    {
        if (RoofOccluders == null)
        {
            return;
        }

        foreach (OccluderFadeController Occluder in RoofOccluders)
        {
            if (Occluder != null)
            {
                Occluder.SetFaded(FadeSource.Interior, Faded);
            }
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private int BuildingId;

    [Header("Detection")]
    [SerializeField] private LayerMask PlayerMask;

    [Header("Roof / Upper Occluders")]
    [SerializeField] private OccluderFadeController[] RoofOccluders;
}
