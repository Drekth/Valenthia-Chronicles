using UnityEngine;
using UnityEngine.InputSystem;
using Sources;

namespace Sources.Entities.Player
{
    public enum CursorType
    {
        Default,
        Interact
    }

    public class InputController : MonoBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int Jump = Animator.StringToHash("Jump");

        private void Awake()
        {
            player = GetComponent<Player>();
            playerCamera = GetComponent<CameraController>();
            animator = GetComponent<Animator>();
            
            if (animator == null)
            {
                AstralisDebug.LogError("Player", "Animator component not found on " + gameObject.name + " or its children");
            }
            
            inputActionMap = InputSystem.actions;
            if (inputActionMap == null)
            {
                AstralisDebug.LogError("Input", "InputSystem.actions is null. Make sure Input Actions are properly configured.");
                return;
            }
        }
        
        private void Start()
        {
            // Initialize cursor to default
            SetCursorForObjectType(ObjectType.None);
        }

        private void OnEnable()
        {
            // Find the references to all the actions
            moveAction = inputActionMap.FindAction("Move");
            jumpAction = inputActionMap.FindAction("Jump");
            zoomAction = inputActionMap.FindAction("Zoom");
        }

        private void Update()
        {
            HandleMovementAction();
            HandleJumpAction();
            HandleZoomAction();
            DetectObjectUnderCursor();
            ApplyGravity();
        }

        private void HandleMovementAction()
        {
            Vector2 moveValue = moveAction.ReadValue<Vector2>();
            
            if (player)
            {
                // Rotation: left/right input rotates the player
                if (Mathf.Abs(moveValue.x) > 0.01f)
                {
                    float rotationAmount = moveValue.x * rotationSpeed * Time.deltaTime;
                    player.transform.Rotate(0, rotationAmount, 0);
                }
                
                // Movement: forward/backward input moves in the direction the player is facing
                bool isMoving = Mathf.Abs(moveValue.y) > 0.01f;
                if (isMoving)
                {
                    Vector3 moveDirection = player.transform.forward * moveValue.y;
                    player.transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
                }
                
                // Update animator: set isRunning parameter based on movement
                if (animator)
                {
                    animator.SetBool(IsRunning, isMoving);
                }
            }
        }
        
        private void HandleJumpAction()
        {
            if (jumpAction.triggered && isGrounded && player)
            {
                verticalVelocity = jumpForce;
                isGrounded = false;
                
                // Trigger jump animation
                if (animator)
                {
                    animator.SetTrigger(Jump);
                    AstralisDebug.Log("Input", "Jump triggered!");
                }
            }
        }
        
        private void HandleZoomAction()
        {
            if (zoomAction == null || !playerCamera)
                return;
            
            float scrollInput = zoomAction.ReadValue<float>();
            
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                playerCamera.AdjustZoom(scrollInput);
            }
        }

        private void ApplyGravity()
        {
            if (!isGrounded && player)
            {
                verticalVelocity += gravity * Time.deltaTime;
                player.transform.Translate(Vector3.up * (verticalVelocity * Time.deltaTime));

                if (player.transform.position.y <= groundY)
                {
                    Vector3 pos = player.transform.position;
                    pos.y = groundY;
                    player.transform.position = pos;
                    verticalVelocity = 0f;
                    isGrounded = true;
                }
            }
        }
        
        private void DetectObjectUnderCursor()
        {
            // Get camera from CameraController instead of using Camera.main
            Camera mainCamera = playerCamera != null ? playerCamera.GetCamera() : null;
            
            if (mainCamera == null || Mouse.current == null)
                return;
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
            {
                WorldObject worldObject = hit.collider.GetComponent<WorldObject>();
                
                if (worldObject != null && worldObject.CanInteract)
                {
                    // New object hovered
                    if (currentHoveredObject != worldObject)
                    {
                        OnObjectHoverExit();
                        currentHoveredObject = worldObject;
                        OnObjectHoverEnter(worldObject);
                    }
                }
                else
                {
                    // Hit something but not a WorldObject
                    OnObjectHoverExit();
                }
            }
            else
            {
                // No hit
                OnObjectHoverExit();
            }
            
            // Store ray for debug visualization
            lastRay = ray;
            lastRayHit = Physics.Raycast(ray, out hit, raycastDistance, interactableLayer);
            lastRayHitPoint = hit.point;
        }
        
        private void OnObjectHoverEnter(WorldObject worldObject)
        {
            SetCursorForObjectType(worldObject.Type);
        }
        
        private void OnObjectHoverExit()
        {
            if (currentHoveredObject != null)
            {
                currentHoveredObject = null;
                SetCursorForObjectType(ObjectType.None);
            }
        }
        
        private void SetCursorForObjectType(ObjectType objectType)
        {
            CursorType cursorType = GetCursorTypeForObject(objectType);
            Texture2D cursorTexture = GetCursorTexture(cursorType);
            
            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
        
        private CursorType GetCursorTypeForObject(ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.NPC:
                case ObjectType.GameObject:
                    return CursorType.Interact;
                case ObjectType.Door:
                case ObjectType.Item:
                    return CursorType.Interact;
                case ObjectType.None:
                default:
                    return CursorType.Default;
            }
        }
        
        private Texture2D GetCursorTexture(CursorType cursorType)
        {
            switch (cursorType)
            {
                case CursorType.Interact:
                    return interactCursor;
                case CursorType.Default:
                default:
                    return defaultCursor;
            }
        }
        
        private void OnDrawGizmos()
        {
            // Draw the raycast in the Scene view for debugging
            if (lastRay.direction != Vector3.zero)
            {
                Gizmos.color = lastRayHit ? Color.green : Color.red;
                
                if (lastRayHit)
                {
                    Gizmos.DrawLine(lastRay.origin, lastRayHitPoint);
                    Gizmos.DrawWireSphere(lastRayHitPoint, 0.1f);
                }
                else
                {
                    Gizmos.DrawRay(lastRay.origin, lastRay.direction * raycastDistance);
                }
            }
        }
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundY;
        private bool isGrounded;
        private float verticalVelocity;
        
        [Header("Cursor Detection Settings")]
        [SerializeField] private LayerMask interactableLayer = ~0;
        [SerializeField] private float raycastDistance = 100f;
        private WorldObject currentHoveredObject;
        
        [Header("Cursor Textures")]
        [SerializeField] private Texture2D defaultCursor;
        [SerializeField] private Texture2D interactCursor;
        private Vector2 cursorHotspot = new Vector2(0, 0);
        
        // Debug visualization
        private Ray lastRay;
        private bool lastRayHit;
        private Vector3 lastRayHitPoint;
        
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction zoomAction;

        private InputActionAsset inputActionMap;
        private CameraController playerCamera;
        private Player player;
        private Animator animator;
    }
}
