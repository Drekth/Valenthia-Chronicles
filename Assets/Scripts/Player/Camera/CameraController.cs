using UnityEngine;
using UnityEngine.InputSystem;

// Top-down player camera (Diablo-like). Fixed-angle perspective rig that smoothly
// follows a target on the ground plane, with mouse-wheel zoom.
// The follow target is pushed in by the player via SetTarget — the camera never
// searches the scene for it.
public class CameraController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Called by the player so the camera knows what to follow.
    public void SetTarget(Transform NewTarget)
    {
        Target = NewTarget;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        TargetDistance = StartDistance;
        CurrentDistance = StartDistance;
    }

    private void OnEnable()
    {
        if (ZoomAction != null)
        {
            ZoomAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (ZoomAction != null)
        {
            ZoomAction.action.Disable();
        }
    }

    private void LateUpdate()
    {
        if (Target == null)
        {
            return;
        }

        UpdateZoom();

        Quaternion Rotation = Quaternion.Euler(Pitch, Yaw, 0.0f);
        Vector3 DesiredPosition = Target.position + Rotation * new Vector3(0.0f, 0.0f, -CurrentDistance);

        transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition, ref FollowVelocity, FollowSmoothTime);
        transform.rotation = Rotation;
    }

    private void UpdateZoom()
    {
        float ZoomInput = ReadZoomInput();

        // Scroll is a per-notch impulse; advance one step regardless of platform magnitude.
        // Positive scroll (up) zooms in, so it shortens the distance.
        if (!Mathf.Approximately(ZoomInput, 0.0f))
        {
            TargetDistance -= Mathf.Sign(ZoomInput) * ZoomStep;
            TargetDistance = Mathf.Clamp(TargetDistance, MinDistance, MaxDistance);
        }

        CurrentDistance = Mathf.MoveTowards(CurrentDistance, TargetDistance, ZoomSpeed * Time.deltaTime);
    }

    private float ReadZoomInput()
    {
        if (ZoomAction == null)
        {
            return 0.0f;
        }

        return ZoomAction.action.ReadValue<float>();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Input")]
    [SerializeField] private InputActionReference ZoomAction;

    [Header("Rig Angle")]
    [SerializeField] private float Pitch = 50.0f;
    [SerializeField] private float Yaw = 0.0f;

    [Header("Distance / Zoom")]
    [SerializeField] private float StartDistance = 12.0f;
    [SerializeField] private float MinDistance = 6.0f;
    [SerializeField] private float MaxDistance = 20.0f;
    [SerializeField] private float ZoomStep = 1.5f;
    [SerializeField] private float ZoomSpeed = 12.0f;

    [Header("Follow")]
    [SerializeField] private float FollowSmoothTime = 0.15f;

    private Transform Target;
    private float TargetDistance;
    private float CurrentDistance;
    private Vector3 FollowVelocity;
}
