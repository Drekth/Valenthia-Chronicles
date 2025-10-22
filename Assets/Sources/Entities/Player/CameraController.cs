using UnityEngine;

namespace Sources.Entities.Player
{
    //[RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        private void Awake()
        {
            player = FindFirstObjectByType<Player>();
            
            if (player == null)
                Debug.LogError("Player not found in the scene for CameraController.");
            
            if (playerCamera == null)
                playerCamera = GetComponent<Camera>();
        }
        
        private void Start()
        {
            InitializeCamera();
        }

        private void LateUpdate()
        {
            UpdateCameraPosition();
        }
        
        private void InitializeCamera()
        {
            currentDistance = cameraDistance;
            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            if (!player || !playerCamera)
                return;

            Vector3 playerPosition = player.GetPosition();

            // Calculate camera height based on distance and angle
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float height = currentDistance * Mathf.Sin(angleRad);
            float horizontalDistance = currentDistance * Mathf.Cos(angleRad);

            Vector3 offset = new Vector3(0, height, -horizontalDistance);
            
            playerCamera.transform.position = playerPosition + offset;
            playerCamera.transform.LookAt(playerPosition);
        }

        public void AdjustZoom(float scrollInput)
        {
            currentDistance -= scrollInput * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
        
        private void OnDrawGizmos()
        {
            // In edition mode, Awake() is not called, so we need to recover the references
            if (playerCamera == null)
                playerCamera = GetComponent<Camera>();
            
            if (player == null)
                player = FindFirstObjectByType<Player>();
            
            // Display the camera arm in the Scene view
            if (playerCamera != null && player != null)
            {
                Vector3 cameraPosition = playerCamera.transform.position;
                Vector3 playerPosition = player.GetPosition();
                
                // Draw the camera arm line
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cameraPosition, playerPosition);
                
                // Draw a sphere at the player position
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerPosition, 0.3f);
            }
        }
        
        [Header("Settings")]
        [SerializeField] private float cameraDistance = 15f;
        [SerializeField] private float cameraAngle = 45f;
        
        [Header("Zoom Settings")]
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private float zoomSpeed = 50f;
        
        [Header("Dependencies")]
        [SerializeField] private Camera playerCamera;
        private Player player;
        private float currentDistance;
    }
}

