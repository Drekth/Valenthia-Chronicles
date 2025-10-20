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
        
        private void InitializeCamera()
        {
            if (!player)
                return;

            Vector3 playerPosition = player.GetPosition();

            // Calculate camera height based on distance and angle
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float height = cameraDistance * Mathf.Sin(angleRad);
            float horizontalDistance = cameraDistance * Mathf.Cos(angleRad);

            Vector3 offset = new Vector3(0, height, -horizontalDistance);
            
            playerCamera.transform.position = playerPosition + offset;
            playerCamera.transform.LookAt(playerPosition);
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
        
        [Header("Dependencies")]
        [SerializeField] private Camera playerCamera;
        private Player player;
    }
}