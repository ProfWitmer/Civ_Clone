using UnityEngine;

namespace CivClone.Presentation.Camera
{
    public sealed class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;

        private Camera cachedCamera;

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
            if (input.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 delta = input * panSpeed * Time.deltaTime;
            transform.Translate(delta, Space.World);
        }

        private void HandleZoom()
        {
            if (cachedCamera == null)
            {
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) <= 0.001f)
            {
                return;
            }

            if (cachedCamera.orthographic)
            {
                float target = cachedCamera.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
                cachedCamera.orthographicSize = Mathf.Clamp(target, minZoom, maxZoom);
            }
            else
            {
                float target = cachedCamera.fieldOfView - scroll * zoomSpeed;
                cachedCamera.fieldOfView = Mathf.Clamp(target, minZoom, maxZoom);
            }
        }
    }
}
