using UnityEngine;
using UnityEngine.InputSystem;

namespace Sources.Entities.Player
{
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
                Debug.LogError("Animator component not found on " + gameObject.name + " or its children");
            }
            
            inputActionMap = InputSystem.actions;
            if (inputActionMap == null)
            {
                Debug.LogError("InputSystem.actions is null. Make sure Input Actions are properly configured.");
                return;
            }
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
                    Debug.Log($"Movement - isMoving: {isMoving}, moveValue.y: {moveValue.y}");
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
                    Debug.Log("Jump triggered!");
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
        
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundY;
        private bool isGrounded;
        private float verticalVelocity;
        
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction zoomAction;

        private InputActionAsset inputActionMap;
        private CameraController playerCamera;
        private Player player;
        private Animator animator;
    }
}
