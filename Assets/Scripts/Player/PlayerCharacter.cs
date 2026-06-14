using UnityEngine;

// Player body (Pawn) for the top-down ARPG. Passive in-game entity driven by the
// PlayerController: it never reads input nor touches the camera, it only obeys the
// Move() / Jump() commands routed to it. Movement happens on the XZ plane via a
// CharacterController, with a persistent vertical velocity that handles gravity and jump.
[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const float InputDeadzoneSquared = 0.01f;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Horizontal move command on the XZ plane (Direction.y is ignored).
    public void Move(Vector3 Direction)
    {
        UpdateRotation(Direction);

        // Keep the controller stuck to the ground while grounded, then accumulate gravity.
        if (Character.isGrounded && VerticalVelocity < 0.0f)
        {
            VerticalVelocity = -StickForce;
        }
        VerticalVelocity -= Gravity * Time.deltaTime;

        Vector3 Velocity = Direction * MoveSpeed;
        Velocity.y = VerticalVelocity;

        Character.Move(Velocity * Time.deltaTime);
    }

    // Jump command — only takes off when grounded (no air jump).
    public void Jump()
    {
        if (Character.isGrounded)
        {
            VerticalVelocity = JumpSpeed;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Character = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        // Announce this body so the persistent PlayerController can possess it.
        if (OnSpawned != null)
        {
            OnSpawned.Raise(this);
        }
    }

    private void OnDisable()
    {
        // Despawn (e.g. zone unload): tell the brain there is no body left to drive.
        if (OnSpawned != null)
        {
            OnSpawned.Raise(null);
        }
    }

    private void UpdateRotation(Vector3 Direction)
    {
        if (Direction.sqrMagnitude < InputDeadzoneSquared)
        {
            return;
        }

        Quaternion TargetRotation = Quaternion.LookRotation(Direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, TargetRotation, RotationSpeed * Time.deltaTime);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Possession")]
    [SerializeField] private PlayerCharacterEventChannel OnSpawned;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed = 6.0f;
    [SerializeField] private float RotationSpeed = 720.0f;
    [SerializeField] private float Gravity = 20.0f;
    [SerializeField] private float JumpSpeed = 8.0f;
    [SerializeField] private float StickForce = 2.0f;

    private CharacterController Character;
    private float VerticalVelocity;
}
