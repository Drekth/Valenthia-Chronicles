using UnityEngine;

// Player body (Pawn) for the top-down ARPG. Passive in-game entity driven by the
// PlayerController: it never reads input nor touches the camera, it only obeys the
// Move() / Jump() / Attack() commands routed to it. Movement happens on the XZ plane via a
// CharacterController, with a persistent vertical velocity that handles gravity and jump.
[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const float InputDeadzoneSquared = 0.01f;
    private const float LocomotionDampTime = 0.1f;

    private static readonly int MoveXHash  = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash  = Animator.StringToHash("MoveZ");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Horizontal move command on the XZ plane (Direction.y is ignored).
    public void Move(Vector3 Direction)
    {
        // Keyboard diagonals yield a magnitude > 1; clamp so diagonal speed and the
        // locomotion blend never exceed the straight-line run value.
        if (Direction.sqrMagnitude > 1.0f)
        {
            Direction.Normalize();
        }

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
        DriveLocomotion(Direction);
    }

    // Jump command — only takes off when grounded (no air jump).
    public void Jump()
    {
        if (Character.isGrounded)
        {
            VerticalVelocity = JumpSpeed;
        }
    }

    public void Attack()
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetTrigger(AttackHash);
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

    // Converts world-space Direction to local space so the blend tree works correctly
    // when strafing is introduced (target-lock, facing cursor, etc.).
    private void DriveLocomotion(Vector3 Direction)
    {
        if (PlayerAnimator == null)
        {
            return;
        }

        Vector3 Local = transform.InverseTransformDirection(Direction);
        PlayerAnimator.SetFloat(MoveXHash, Local.x, LocomotionDampTime, Time.deltaTime);
        PlayerAnimator.SetFloat(MoveZHash, Local.z, LocomotionDampTime, Time.deltaTime);
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

    [Header("Animation")]
    [SerializeField] private Animator PlayerAnimator;

    private CharacterController Character;
    private float VerticalVelocity;
}
