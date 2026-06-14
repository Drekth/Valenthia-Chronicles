using UnityEngine;

// Player brain (Unreal's APlayerController). Persistent across zone swaps: it owns no body
// of its own but possesses the PlayerCharacter the current zone announces via the OnSpawned
// channel, routes input from the PlayerInputController to it, and binds the camera to it.
public class PlayerController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void OnEnable()
    {
        if (OnSpawned != null)
        {
            OnSpawned.Subscribe(Possess);
        }
    }

    private void OnDisable()
    {
        if (OnSpawned != null)
        {
            OnSpawned.Unsubscribe(Possess);
        }
    }

    private void Update()
    {
        if (Character == null || Input == null)
        {
            return;
        }

        Vector2 MoveInput = Input.MoveInput;
        Vector3 Direction = new Vector3(MoveInput.x, 0.0f, MoveInput.y);
        Character.Move(Direction);

        if (Input.ConsumeJump())
        {
            Character.Jump();
        }
    }

    // Take control of the body the zone just spawned (or release it when null on despawn).
    private void Possess(PlayerCharacter NewCharacter)
    {
        Character = NewCharacter;

        if (CameraRig != null && Character != null)
        {
            CameraRig.SetTarget(Character.transform);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("References")]
    [SerializeField] private PlayerInputController Input;
    [SerializeField] private CameraController CameraRig;

    [Header("Possession")]
    [SerializeField] private PlayerCharacterEventChannel OnSpawned;

    private PlayerCharacter Character;
}
