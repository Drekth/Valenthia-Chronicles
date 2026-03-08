using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private void Awake()
    {
        player = GetComponent<Player>();
        cameraController = GetComponent<CameraController>();
        spellCaster = GetComponent<Spell>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        playerRootTransform = player.GetPlayerTransform();
        playerModelTransform = player.GetPlayerModelTransform();

        if (playerModelTransform != null)
        {
            animator = playerModelTransform.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("[InputController] No Animator found on player model!");
            }
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Move");
            interactAction = inputActions.FindAction("Interact");
            advanceDialogueAction = inputActions.FindAction("AdvanceDialogue");
            selectAction = inputActions.FindAction("Select");
            attackAction = inputActions.FindAction("Attack");
            zoomAction = inputActions.FindAction("Zoom");

            if (interactAction != null)
            {
                interactAction.Enable();
                interactAction.performed += OnInteractPerformed;
            }

            if (advanceDialogueAction != null)
            {
                advanceDialogueAction.Enable();
                advanceDialogueAction.performed += OnAdvanceDialoguePerformed;
            }

            if (selectAction != null)
            {
                selectAction.Enable();
                selectAction.performed += OnSelectPerformed;
            }

            if (attackAction != null)
            {
                attackAction.Enable();
                attackAction.performed += OnAttackPerformed;
            }

            if (zoomAction != null)
            {
                zoomAction.Enable();
            }

            spellSlotActions = new InputAction[5];
            for (int i = 0; i < 5; i++)
            {
                spellSlotActions[i] = inputActions.FindAction($"SpellSlot{i + 1}");
                if (spellSlotActions[i] != null)
                {
                    spellSlotActions[i].Enable();
                    int slotIndex = i;
                    spellSlotActions[i].performed += _ => OnSpellSlotPerformed(slotIndex);
                }
            }
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction.Disable();
        }

        if (advanceDialogueAction != null)
        {
            advanceDialogueAction.performed -= OnAdvanceDialoguePerformed;
            advanceDialogueAction.Disable();
        }

        if (selectAction != null)
        {
            selectAction.performed -= OnSelectPerformed;
            selectAction.Disable();
        }

        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
            attackAction.Disable();
        }

        if (zoomAction != null)
        {
            zoomAction.Disable();
        }

        if (spellSlotActions != null)
        {
            foreach (var action in spellSlotActions)
            {
                action?.Disable();
            }
        }
    }

    private void Update()
    {
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        if (zoomAction != null && cameraController != null)
        {
            float zoomInput = zoomAction.ReadValue<float>();
            cameraController.ProcessZoom(zoomInput);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    #region Movement and Animation methods

    private void HandleMovement()
    {
        if (playerRootTransform == null || playerModelTransform == null)
            return;

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 move = moveDirection * (moveSpeed * Time.fixedDeltaTime);
            playerRootTransform.position += move;

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            playerModelTransform.rotation = Quaternion.Slerp(playerModelTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        float speed = moveInput.magnitude * moveSpeed;
        animator.SetFloat(SpeedHash, speed);
    }

    #endregion

    #region Conversation and Interaction

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        if (mainCamera == null)
        {
            Debug.LogError("[InputController] Main camera is null!");
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer, QueryTriggerInteraction.Collide))
        {
            Debug.Log($"[InputController] Raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            ISelectable selectable = hit.collider.GetComponent<ISelectable>();

            if (selectable != null && selectable.IsSelectable())
            {
                player.SelectUnit(selectable);
            }
            else
            {
                player.DeselectCurrentUnit();
            }
        }
        else
        {
            player.DeselectCurrentUnit();
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (mainCamera == null)
        {
            Debug.LogError("[InputController] Main camera is null!");
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer, QueryTriggerInteraction.Collide))
        {
            Debug.Log($"[InputController] Hit: {hit.collider.gameObject.name}");

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                interactable.OnInteract();
            }
        }
    }

    private void OnAdvanceDialoguePerformed(InputAction.CallbackContext context)
    {
        if (ConversationUI.Instance != null && ConversationUI.Instance.IsDialogueActive())
        {
            ConversationUI.Instance.HideDialogue();
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (animator == null)
            return;

        if (isLeftPunch)
        {
            animator.SetTrigger(PunchLeftHash);
        }
        else
        {
            animator.SetTrigger(PunchRightHash);
        }

        StartCoroutine(PerformPunchDetection());

        isLeftPunch = !isLeftPunch;
    }

    private void OnSpellSlotPerformed(int slotIndex)
    {
        if (spellCaster != null)
        {
            spellCaster.CastSpellBySlot(slotIndex);
        }
    }

    private System.Collections.IEnumerator PerformPunchDetection()
    {
        yield return new WaitForSeconds(punchDelay);

        if (playerModelTransform == null)
            yield break;

        Vector3 punchOrigin = playerModelTransform.position + Vector3.up * 1f;
        Vector3 punchDirection = playerModelTransform.forward;

        lastPunchPosition = punchOrigin;
        lastPunchDirection = punchDirection;
        showPunchGizmo = true;

        RaycastHit[] hits = new RaycastHit[10];
        int hitCount = Physics.SphereCastNonAlloc(punchOrigin, punchRadius, punchDirection, hits, punchDistance, enemyLayer);

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                Unit enemy = hits[i].collider.GetComponent<Unit>();
                if (enemy != null && enemy != player)
                {
                    player.SetCurrentTarget(enemy);
                    Debug.Log($"[InputController] Punch hit enemy: {enemy.gameObject.name}");
                    break;
                }
            }
        }

        yield return new WaitForSeconds(0.1f);
        showPunchGizmo = false;
    }

    #endregion methods

    private void OnDrawGizmos()
    {
        if (showPunchGizmo && playerModelTransform != null)
        {
            Gizmos.color = Color.yellow;

            Vector3 startPos = lastPunchPosition;
            Vector3 endPos = startPos + lastPunchDirection * punchDistance;

            Gizmos.DrawWireSphere(startPos, punchRadius);
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawWireSphere(endPos, punchRadius);
        }
    }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("Combat Settings")]
    [SerializeField] private float punchDistance = 1.5f;
    [SerializeField] private float punchRadius = 0.5f;
    [SerializeField] private float punchDelay = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Editor References")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Camera mainCamera;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int PunchLeftHash = Animator.StringToHash("PunchLeft");
    private static readonly int PunchRightHash = Animator.StringToHash("PunchRight");

    private Player player;
    private CameraController cameraController;
    private Animator animator;
    private Transform playerRootTransform;
    private Transform playerModelTransform;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction advanceDialogueAction;
    private InputAction selectAction;
    private InputAction attackAction;
    private InputAction zoomAction;
    private InputAction[] spellSlotActions;
    private Spell spellCaster;
    private Vector2 moveInput;
    private Vector3 lastPunchPosition;
    private Vector3 lastPunchDirection;
    private bool isLeftPunch = true;
    private bool showPunchGizmo;
}
