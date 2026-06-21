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

    // Casts the spell bound to the given action-bar slot on the current target. Triggered by the
    // action-bar keys (ActionBarInput). The animation is only played when the cast is accepted; the
    // damage itself lands later, on the clip's impact frame (SpellAnimationRelay).
    public void CastSlot(int Slot)
    {
        SpellData Spell = ResolveSlot(Slot);
        if (Spell == null || Caster == null)
        {
            return;
        }

        if (!ServiceLocator.TryGet<SelectionManager>(out SelectionManager Selection))
        {
            return;
        }

        SpellCastResult Result = Caster.TryCast(Spell, Selection.CurrentTarget);
        if (Result == SpellCastResult.Success && PlayerAnimator != null)
        {
            PlayerAnimator.SetTrigger(AttackHash);
        }
    }

    // Toggles the WoW-style auto-attack on the currently selected target. Triggered by the
    // right-click input (AutoAttackInput). While on, Update keeps swinging on the weapon's attack
    // speed; the actual weapon damage lands on the clip's impact frame like any other cast.
    public void ToggleAutoAttack()
    {
        AutoAttacking = !AutoAttacking;
    }

    // Hard stop, used when the body is despawned or auto-attack must be cancelled externally.
    public void StopAutoAttack()
    {
        AutoAttacking = false;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Character = GetComponent<CharacterController>();
        Caster = GetComponent<SpellCaster>();
    }

    private void OnEnable()
    {
        // Announce this body so the persistent PlayerController can possess it.
        EventBus<PlayerSpawnedEvent>.Raise(new PlayerSpawnedEvent { Character = this });

        // Publish the action-bar loadout so the HUD can paint the slot icons. Single basic attack
        // in slot 0 for now; a future PlayerHotbar would back ResolveSlot with a real spellbook.
        PublishHotbar(false);
    }

    private void OnDisable()
    {
        // Despawn (e.g. zone unload): tell the brain there is no body left to drive.
        EventBus<PlayerSpawnedEvent>.Raise(new PlayerSpawnedEvent { Character = null });

        // Clear every action-bar slot this body owned.
        PublishHotbar(true);

        // Drop any running auto-attack so a re-spawned body starts idle.
        AutoAttacking = false;
    }

    // Drives the auto-attack loop: while active, swings at the weapon's attack speed on the current
    // target. Cadence and range are owned by SpellCaster — CanCast returns OnCooldown between swings
    // and OutOfRange when too far, both silently, so the loop never spams SpellCastFailedEvent. The
    // target is re-read each frame so the auto-attack follows the selection, WoW-style.
    private void Update()
    {
        if (!AutoAttacking || AutoAttack == null || Caster == null)
        {
            return;
        }

        if (!ServiceLocator.TryGet<SelectionManager>(out SelectionManager Selection))
        {
            return;
        }

        Unit Target = Selection.CurrentTarget;
        if (Target == null || Target.IsDead)
        {
            AutoAttacking = false;
            return;
        }

        if (Caster.CanCast(AutoAttack, Target) != SpellCastResult.Success)
        {
            return;
        }

        Caster.TryCast(AutoAttack, Target);
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetTrigger(AttackHash);
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

    // The spell bound to an action-bar slot. Slot 0 is the basic attack; the rest are empty until a
    // real spellbook exists. Single place that maps slots to spells, shared by casting and the HUD.
    private SpellData ResolveSlot(int Slot)
    {
        return Slot == 0 ? BasicAttack : null;
    }

    // Raises one HotbarSlotAssignedEvent per slot so the HUD mirrors the loadout exactly. When
    // Clear is true every slot is emptied (despawn); otherwise each slot reflects ResolveSlot.
    private void PublishHotbar(bool Clear)
    {
        for (int Slot = 0; Slot < HUDActionBar.SlotCount; Slot++)
        {
            EventBus<HotbarSlotAssignedEvent>.Raise(new HotbarSlotAssignedEvent
            {
                Slot  = Slot,
                Spell = Clear ? null : ResolveSlot(Slot),
            });
        }
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

    [Header("Movement")]
    [SerializeField] private float MoveSpeed = 6.0f;
    [SerializeField] private float RotationSpeed = 720.0f;
    [SerializeField] private float Gravity = 20.0f;
    [SerializeField] private float JumpSpeed = 8.0f;
    [SerializeField] private float StickForce = 2.0f;

    [Header("Animation")]
    [SerializeField] private Animator PlayerAnimator;

    [Header("Combat")]
    [SerializeField] private SpellData BasicAttack;
    [SerializeField] private SpellData AutoAttack;

    private CharacterController Character;
    private SpellCaster Caster;
    private float VerticalVelocity;
    private bool AutoAttacking;
}
