using UnityEngine;

namespace CivClone.Presentation.Camera
{
    public sealed class IsometricCameraController : MonoBehaviour
    {
        public enum MapSize
        {
            Small,
            Medium,
            Large
        }

        [Header("Movement")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float minOrthoZoom = 5f;
        [SerializeField] private float maxOrthoZoom = 50f;
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 80f;

        [Header("Map Bounds")]
        [SerializeField] private MapSize mapSize = MapSize.Medium;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Vector2Int smallSize = new Vector2Int(40, 25);
        [SerializeField] private Vector2Int mediumSize = new Vector2Int(60, 40);
        [SerializeField] private Vector2Int largeSize = new Vector2Int(80, 60);

        [Header("Debug")]
        [SerializeField] private bool showDebugOverlay = false;

        private UnityEngine.Camera cachedCamera;
        private float lastHorizontal;
        private float lastVertical;
        private float lastScroll;
        private float lastUpdateTime;
        private Vector3 lastPosition;

        private void Awake()
        {
            cachedCamera = GetComponent<UnityEngine.Camera>();
        }

        private void Update()
        {
            HandlePan();
            HandleZoom();
            ClampToBounds();

            lastUpdateTime = Time.unscaledTime;
            lastPosition = transform.position;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay)
            {
                return;
            }

            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(10f, 10f, 460f, 180f), GUI.skin.box);
            GUILayout.Label("IsometricCameraController active");
            GUILayout.Label($"Last Update: {lastUpdateTime:F2}s");
            GUILayout.Label($"Pan Input H/V: {lastHorizontal:F2} / {lastVertical:F2}");
            GUILayout.Label($"Scroll: {lastScroll:F2}");
            if (cachedCamera != null)
            {
                GUILayout.Label($"Ortho: {cachedCamera.orthographic} | Size: {cachedCamera.orthographicSize:F2} | FOV: {cachedCamera.fieldOfView:F2}");
            }
            GUILayout.Label($"Position: {lastPosition.x:F2}, {lastPosition.y:F2}, {lastPosition.z:F2}");
            GUILayout.EndArea();
        }

        private void HandlePan()
        {
            float horizontal = GetAxisWithKeys(KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow, "Horizontal");
            float vertical = GetAxisWithKeys(KeyCode.S, KeyCode.W, KeyCode.DownArrow, KeyCode.UpArrow, "Vertical");

            lastHorizontal = horizontal;
            lastVertical = vertical;

            Vector3 input = new Vector3(horizontal, 0f, vertical);
            if (input.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 delta = (right * input.x + forward * input.z) * panSpeed * Time.deltaTime;
            transform.Translate(delta, Space.World);
        }

        private void HandleZoom()
        {
            if (cachedCamera == null)
            {
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            float legacyScroll = Input.GetAxis("Mouse ScrollWheel") * 10f;
            float combinedScroll = Mathf.Abs(scroll) > Mathf.Abs(legacyScroll) ? scroll : legacyScroll;

            lastScroll = combinedScroll;

            if (Mathf.Abs(combinedScroll) <= 0.001f)
            {
                return;
            }

            if (cachedCamera.orthographic)
            {
                float target = cachedCamera.orthographicSize - combinedScroll * zoomSpeed * Time.deltaTime;
                cachedCamera.orthographicSize = Mathf.Clamp(target, minOrthoZoom, maxOrthoZoom);
            }
            else
            {
                float target = cachedCamera.fieldOfView - combinedScroll * zoomSpeed;
                cachedCamera.fieldOfView = Mathf.Clamp(target, minFov, maxFov);
            }
        }

        private void ClampToBounds()
        {
            Vector2Int size = GetMapSize();
            float maxX = size.x * tileSize;
            float maxZ = size.y * tileSize;

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, 0f, maxX);
            position.z = Mathf.Clamp(position.z, 0f, maxZ);
            transform.position = position;
        }

        private Vector2Int GetMapSize()
        {
            return mapSize switch
            {
                MapSize.Small => smallSize,
                MapSize.Medium => mediumSize,
                MapSize.Large => largeSize,
                _ => mediumSize
            };
        }

        private float GetAxisWithKeys(KeyCode negativePrimary, KeyCode positivePrimary, KeyCode negativeAlt, KeyCode positiveAlt, string axis)
        {
            float value = 0f;

            if (Input.GetKey(negativePrimary) || Input.GetKey(negativeAlt))
            {
                value -= 1f;
            }

            if (Input.GetKey(positivePrimary) || Input.GetKey(positiveAlt))
            {
                value += 1f;
            }

            if (Mathf.Abs(value) > 0.001f)
            {
                return value;
            }

            return Input.GetAxisRaw(axis);
        }
    }
}
