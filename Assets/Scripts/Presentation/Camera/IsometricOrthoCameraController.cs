using CivClone.Presentation;
using UnityEngine;

namespace CivClone.Presentation.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class IsometricOrthoCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private MapPresenter mapPresenter;

        [Header("Movement")]
        [SerializeField] private float panSpeed = 8f;
        [SerializeField] private float edgeScrollSpeed = 10f;
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float dragSpeed = 1f;
        [SerializeField] private float minOrthoSize = 4f;
        [SerializeField] private float maxOrthoSize = 16f;
        [SerializeField] private float edgePadding = 12f;
        [SerializeField] private float boundsPadding = 0.5f;
        [SerializeField] private int dragMouseButton = 2;
        [SerializeField] private bool allowRightMouseDrag = true;

        private UnityEngine.Camera cachedCamera;
        private bool centeredOnStart;
        private bool isDragging;
        private Vector3 lastDragWorld;

        public void Bind(MapPresenter presenter)
        {
            mapPresenter = presenter;
        }

        private void Awake()
        {
            cachedCamera = GetComponent<UnityEngine.Camera>();
        }

        private void Start()
        {
            if (mapPresenter == null)
            {
                mapPresenter = FindObjectOfType<MapPresenter>();
            }

            CenterOnMap();
        }

        private void Update()
        {
            HandleDrag();
            HandlePan();
            HandleEdgeScroll();
            HandleZoom();
            ClampToBounds();
        }

        private void CenterOnMap()
        {
            if (centeredOnStart || mapPresenter == null)
            {
                return;
            }

            if (mapPresenter.TryGetWorldBounds(out var bounds))
            {
                var position = transform.position;
                position.x = bounds.center.x;
                position.y = bounds.center.y;
                transform.position = position;
                centeredOnStart = true;
            }
        }

        private void HandleDrag()
        {
            if (cachedCamera == null || !cachedCamera.orthographic)
            {
                return;
            }

            if (Input.GetMouseButtonDown(dragMouseButton) || (allowRightMouseDrag && Input.GetMouseButtonDown(1)))
            {
                isDragging = true;
                lastDragWorld = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
                return;
            }

            if (Input.GetMouseButtonUp(dragMouseButton) || (allowRightMouseDrag && Input.GetMouseButtonUp(1)))
            {
                isDragging = false;
                return;
            }

            if (!isDragging)
            {
                return;
            }

            var currentWorld = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
            var delta = lastDragWorld - currentWorld;
            delta.z = 0f;
            transform.position += delta * dragSpeed;
        }

        private void HandlePan()
        {
            if (isDragging)
            {
                return;
            }

            float horizontal = GetAxisWithKeys(KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow, "Horizontal");
            float vertical = GetAxisWithKeys(KeyCode.S, KeyCode.W, KeyCode.DownArrow, KeyCode.UpArrow, "Vertical");

            Vector3 input = new Vector3(horizontal, vertical, 0f);
            if (input.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.position += input * panSpeed * Time.unscaledDeltaTime;
        }

        private void HandleEdgeScroll()
        {
            if (!Application.isFocused || isDragging)
            {
                return;
            }

            var mouse = Input.mousePosition;
            float horizontal = 0f;
            float vertical = 0f;

            if (mouse.x <= edgePadding)
            {
                horizontal = -1f;
            }
            else if (mouse.x >= Screen.width - edgePadding)
            {
                horizontal = 1f;
            }

            if (mouse.y <= edgePadding)
            {
                vertical = -1f;
            }
            else if (mouse.y >= Screen.height - edgePadding)
            {
                vertical = 1f;
            }

            if (Mathf.Abs(horizontal) <= 0.001f && Mathf.Abs(vertical) <= 0.001f)
            {
                return;
            }

            var input = new Vector3(horizontal, vertical, 0f);
            transform.position += input * edgeScrollSpeed * Time.unscaledDeltaTime;
        }

        private void HandleZoom()
        {
            if (cachedCamera == null || !cachedCamera.orthographic)
            {
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            float legacyScroll = Input.GetAxis("Mouse ScrollWheel") * 10f;
            float combined = Mathf.Abs(scroll) > Mathf.Abs(legacyScroll) ? scroll : legacyScroll;

            if (Mathf.Abs(combined) <= 0.001f)
            {
                return;
            }

            float target = cachedCamera.orthographicSize - combined * zoomSpeed * Time.unscaledDeltaTime;
            cachedCamera.orthographicSize = Mathf.Clamp(target, minOrthoSize, maxOrthoSize);
        }

        private void ClampToBounds()
        {
            if (mapPresenter == null || !mapPresenter.TryGetWorldBounds(out var bounds))
            {
                return;
            }

            if (cachedCamera == null || !cachedCamera.orthographic)
            {
                return;
            }

            float halfHeight = cachedCamera.orthographicSize;
            float halfWidth = halfHeight * cachedCamera.aspect;

            var position = transform.position;
            float minX = bounds.min.x + halfWidth - boundsPadding;
            float maxX = bounds.max.x - halfWidth + boundsPadding;
            float minY = bounds.min.y + halfHeight - boundsPadding;
            float maxY = bounds.max.y - halfHeight + boundsPadding;

            if (minX > maxX)
            {
                position.x = bounds.center.x;
            }
            else
            {
                position.x = Mathf.Clamp(position.x, minX, maxX);
            }

            if (minY > maxY)
            {
                position.y = bounds.center.y;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, minY, maxY);
            }

            transform.position = position;
        }

        private static float GetAxisWithKeys(KeyCode negativePrimary, KeyCode positivePrimary, KeyCode negativeAlt, KeyCode positiveAlt, string axis)
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
