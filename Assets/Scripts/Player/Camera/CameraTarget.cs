using UnityEngine;

// Shared follow target for the camera rig. Both the CameraController (which positions the rig)
// and the CameraOcclusionFader (which raycasts toward the target) read the same Transform from
// here, so the body is wired in one place. The target is pushed in by the player via SetTarget —
// the rig never searches the scene for it.
public class CameraTarget : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // The Transform the rig currently follows, or null before a body is possessed.
    public Transform Current
    {
        get { return Target; }
    }

    // Called by the player so the whole rig knows what to follow.
    public void SetTarget(Transform NewTarget)
    {
        Target = NewTarget;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private Transform Target;
}
