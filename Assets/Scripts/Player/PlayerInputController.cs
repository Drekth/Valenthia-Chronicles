using UnityEngine;
using UnityEngine.InputSystem;

// Player input source (Unreal's PlayerInput). The single place that knows about the
// Input System: it enables/disables the actions and exposes them to the PlayerController.
// Limited on purpose to movement and jump.
public class PlayerInputController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Continuous movement value — read each frame by the PlayerController.
    public Vector2 MoveInput
    {
        get
        {
            if (MoveAction == null)
            {
                return Vector2.zero;
            }

            return MoveAction.action.ReadValue<Vector2>();
        }
    }

    // True only on the frame the jump button is pressed (rising edge, no manual state).
    public bool ConsumeJump()
    {
        return JumpAction != null && JumpAction.action.WasPressedThisFrame();
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        if (MoveAction != null)
        {
            MoveAction.action.Enable();
        }

        if (JumpAction != null)
        {
            JumpAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (MoveAction != null)
        {
            MoveAction.action.Disable();
        }

        if (JumpAction != null)
        {
            JumpAction.action.Disable();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Input")]
    [SerializeField] private InputActionReference MoveAction;
    [SerializeField] private InputActionReference JumpAction;
}
