using UnityEngine;

namespace Entities
{
    public class CameraController : MonoBehaviour
    {
        [Header("Editor References")]
        [SerializeField] private Camera playerCamera;

        [Header("Camera Settings")]
        [SerializeField, Range(0f, 89f)]
        private float angle = 45f;

        [SerializeField, Range(1f, 100f)]
        private float distance = 20f;

        [Header("Zoom Settings")]
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float zoomSmoothTime = 0.1f;

        private float targetDistance;
        private float currentZoomVelocity;

        private void Awake()
        {
            targetDistance = distance;
        }

        public void AttachToPlayer(Transform playerTransform)
        {
            if (playerCamera != null)
            {
                playerCamera.transform.SetParent(playerTransform);
                UpdateCameraPosition();
            }
        }

        public void ProcessZoom(float zoomInput)
        {
            if (Mathf.Approximately(zoomInput, 0f))
                return;

            targetDistance -= zoomInput * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        private void LateUpdate()
        {
            distance = Mathf.SmoothDamp(distance, targetDistance, ref currentZoomVelocity, zoomSmoothTime);
            UpdateCameraPosition();
        }

        private void OnValidate()
        {
            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            if (playerCamera == null || playerCamera.transform.parent == null)
                return;

            float angleInRadians = angle * Mathf.Deg2Rad;

            float height = distance * Mathf.Sin(angleInRadians);
            float horizontalDistance = distance * Mathf.Cos(angleInRadians);

            playerCamera.transform.localPosition = new Vector3(0f, height, -horizontalDistance);
            playerCamera.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }
}