using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Sources.Entities.Player
{
    public class InputController : MonoBehaviour
    {
        private void Awake()
        {
            player = GetComponent<Player>();
            playerCamera = GetComponent<CameraController>();
            
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
                if (Mathf.Abs(moveValue.y) > 0.01f)
                {
                    Vector3 moveDirection = player.transform.forward * moveValue.y;
                    player.transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
                }
            }
        }
        
        private void HandleJumpAction()
        {
            if (jumpAction.triggered && isGrounded && player)
            {
                verticalVelocity = jumpForce;
                isGrounded = false;
            }
        }
        
        private void HandleZoomAction()
        {
            if (zoomAction == null || playerCamera == null)
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
        [SerializeField] private float groundY = 0f;
        private bool isGrounded;
        private float verticalVelocity = 0f;
        
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction zoomAction;

        private InputActionAsset inputActionMap;
        private CameraController playerCamera;
        private Player player;
    }
}
