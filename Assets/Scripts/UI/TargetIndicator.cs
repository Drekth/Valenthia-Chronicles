using UnityEngine;
using UnityEngine.Rendering.Universal;

// Drives the on-ground halo: listens to the target channel and parks a URP DecalProjector
// under the current target unit, sized from a global default radius. Follows the target each
// LateUpdate (creatures move) and hides itself when there is no target or it was destroyed.
// The projector is a child of the persistent rig and starts disabled.
public class TargetIndicator : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // The projector's box is centered on its position, so we center it on the anchor and rely
    // on the depth to straddle the ground above and below the unit's feet.
    private const float ProjectionDepth = 4.0f;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        if (Projector != null)
        {
            Projector.enabled = false;
        }

        TargetChangedBinding = new EventBinding<TargetChangedEvent>(HandleTargetChanged);
        EventBus<TargetChangedEvent>.Register(TargetChangedBinding);
    }

    private void OnDisable()
    {
        EventBus<TargetChangedEvent>.Deregister(TargetChangedBinding);
    }

    private void LateUpdate()
    {
        if (Target == null || Projector == null)
        {
            // Target was destroyed without raising a change — hide the orphaned halo.
            if (Projector != null && Projector.enabled)
            {
                Projector.enabled = false;
            }

            return;
        }

        PositionProjector();
    }

    private void HandleTargetChanged(TargetChangedEvent Event)
    {
        Target = Event.Target;

        if (Projector == null)
        {
            return;
        }

        if (Target == null)
        {
            Projector.enabled = false;
            return;
        }

        float Diameter = HaloRadius * 2.0f;
        Projector.size = new Vector3(Diameter, Diameter, ProjectionDepth);
        PositionProjector();
        Projector.enabled = true;
    }

    private void PositionProjector()
    {
        // Center the projection box on the unit; the box depth covers the ground below the feet.
        Projector.transform.position = Target.transform.position;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Halo")]
    [SerializeField] private DecalProjector Projector;
    [SerializeField] private float HaloRadius = 1.0f;

    private Unit Target;
    private EventBinding<TargetChangedEvent> TargetChangedBinding;
}
