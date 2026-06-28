using System.Collections.Generic;
using UnityEngine;

// Micro occlusion layer: every frame, sweeps a sphere from the camera toward the follow target and
// fades out any occluder caught between them (a tree, a tower, a wall corner). Pairs with the macro
// BuildingZone layer, which handles whole-building roofs. Both drive the same OccluderFadeController,
// so an object hit by both simply stays faded until neither asks for it anymore.
//
// Lives on the same GameObject as the CameraController so transform.position is the eye position.
[RequireComponent(typeof(CameraTarget))]
public class CameraOcclusionFader : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // Upper bound on occluders handled in a single frame; keeps the cast allocation-free.
    private const int MaxHits = 32;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        if (Target == null)
        {
            Target = GetComponent<CameraTarget>();
        }

        HitBuffer = new RaycastHit[MaxHits];
        FadedNow = new HashSet<OccluderFadeController>();
        FadedPrevious = new HashSet<OccluderFadeController>();
    }

    private void LateUpdate()
    {
        if (Target == null || Target.Current == null)
        {
            return;
        }

        FadedNow.Clear();

        Vector3 Origin = transform.position;
        Vector3 ToTarget = Target.Current.position + Vector3.up * TargetHeightOffset - Origin;
        float Distance = ToTarget.magnitude;

        // Degenerate sweep (camera sitting on the target) — nothing to occlude.
        if (Distance > SphereRadius)
        {
            Vector3 Direction = ToTarget / Distance;
            SweepAndCollect(Origin, Direction, Distance - SphereRadius);
        }

        FadeNewlyOccluded();
        RestoreNoLongerOccluded();

        // Swap buffers: this frame's set becomes next frame's history (reuse both, no garbage).
        HashSet<OccluderFadeController> Swap = FadedPrevious;
        FadedPrevious = FadedNow;
        FadedNow = Swap;
    }

    private void SweepAndCollect(Vector3 Origin, Vector3 Direction, float Distance)
    {
        int Count = Physics.SphereCastNonAlloc(Origin, SphereRadius, Direction, HitBuffer, Distance, OccluderMask, QueryTriggerInteraction.Ignore);

        for (int Index = 0; Index < Count; Index++)
        {
            OccluderFadeController Occluder = HitBuffer[Index].collider.GetComponentInParent<OccluderFadeController>();
            if (Occluder != null)
            {
                FadedNow.Add(Occluder);
            }
        }
    }

    private void FadeNewlyOccluded()
    {
        foreach (OccluderFadeController Occluder in FadedNow)
        {
            Occluder.SetFaded(FadeSource.Camera, true);
        }
    }

    private void RestoreNoLongerOccluded()
    {
        foreach (OccluderFadeController Occluder in FadedPrevious)
        {
            if (!FadedNow.Contains(Occluder))
            {
                Occluder.SetFaded(FadeSource.Camera, false);
            }
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Cast")]
    [SerializeField] private LayerMask OccluderMask;
    [SerializeField] private float SphereRadius = 0.4f;

    // Aim the sweep at the character's torso rather than its feet, so low occluders are caught.
    [SerializeField] private float TargetHeightOffset = 1.0f;

    [Header("References")]
    [SerializeField] private CameraTarget Target;

    private RaycastHit[] HitBuffer;
    private HashSet<OccluderFadeController> FadedNow;
    private HashSet<OccluderFadeController> FadedPrevious;
}
