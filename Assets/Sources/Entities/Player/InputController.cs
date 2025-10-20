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
        }

        private void Update()
        {
            HandleMovementAction();
            HandleJumpAction();
            ApplyGravity();
        }

        private void HandleMovementAction()
        {
            Vector2 moveValue = moveAction.ReadValue<Vector2>();
            
            if (player)
            {
                player.transform.Translate(new Vector3(moveValue.x, 0, moveValue.y) * (moveSpeed * Time.deltaTime));
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
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundY = 0f;
        private bool isGrounded;
        private float verticalVelocity = 0f;
        
        private InputAction moveAction;
        private InputAction jumpAction;

        private InputActionAsset inputActionMap;
        private Player player;
    }
}