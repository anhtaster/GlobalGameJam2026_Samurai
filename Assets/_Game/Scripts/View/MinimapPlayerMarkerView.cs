using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    public class MinimapPlayerMarkerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image markerImage;
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private MinimapGridViewModel viewModel;

        [Header("Settings")]
        [SerializeField] private MinimapColorConfig colorConfig;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool smoothMovement = true;
        [SerializeField] private bool hideOnWall = true;

        private RectTransform markerRect; // Cache RectTransform
        private Vector3 targetPosition;
        private bool hasTarget;

        private void Awake()
        {
            if (markerImage == null)
            {
                markerImage = GetComponent<Image>();
            }

            // Cache RectTransform once
            markerRect = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (colorConfig != null && markerImage != null)
            {
                markerImage.color = colorConfig.PlayerMarkerColor;
            }
        }

        private void Update()
        {
            if (!hasTarget || markerRect == null)
                return;

            if (smoothMovement)
            {
                markerRect.position = Vector3.Lerp(
                    markerRect.position,
                    targetPosition,
                    Time.deltaTime * smoothSpeed
                );
            }
        }

        private void OnEnable()
        {
            if (viewModel == null)
            {
                viewModel = FindFirstObjectByType<MinimapGridViewModel>();
            }

            if (gridView == null)
            {
                gridView = GetComponentInParent<MinimapGridView>();
            }

            if (viewModel == null || gridView == null)
            {
                Debug.LogError("[MinimapPlayerMarkerView] Missing ViewModel or GridView reference!");
                return;
            }

            viewModel.PlayerViewPositionChanged += UpdateMarkerPosition;
            viewModel.ViewportDataChanged += OnViewportDataChanged;
            viewModel.ForceRefresh();
        }

        private void OnDisable()
        {
            if (viewModel != null)
            {
                viewModel.PlayerViewPositionChanged -= UpdateMarkerPosition;
                viewModel.ViewportDataChanged -= OnViewportDataChanged;
            }
        }

        private void OnViewportDataChanged(MinimapViewportData data)
        {
            if (data == null || !hideOnWall)
                return;

            // Hide marker when player is on Wall
            if (data.PlayerCellType == CellType.Wall)
            {
                if (markerImage != null)
                {
                    markerImage.enabled = false;
                }
                hasTarget = false;
            }
        }

        private void UpdateMarkerPosition(Vector2Int viewPosition)
        {
            if (gridView == null)
                return;

            if (viewPosition.x < 0 || viewPosition.y < 0)
            {
                hasTarget = false;
                if (markerImage != null)
                {
                    markerImage.enabled = false;
                }
                return;
            }

            MinimapCellView cellView = gridView.GetCellView(viewPosition.x, viewPosition.y);
            if (cellView == null)
                return;

            RectTransform targetRect = cellView.GetComponent<RectTransform>();
            if (targetRect == null || markerRect == null)
                return;

            targetPosition = targetRect.position;
            hasTarget = true;

            if (markerImage != null)
            {
                markerImage.enabled = true;
            }

            if (!smoothMovement)
            {
                markerRect.position = targetPosition;
            }
        }

        public void SetGridView(MinimapGridView view)
        {
            gridView = view;
        }

        public void SetViewModel(MinimapGridViewModel model)
        {
            viewModel = model;
        }

        public void SetColorConfig(MinimapColorConfig config)
        {
            colorConfig = config;
            if (markerImage != null && colorConfig != null)
            {
                markerImage.color = colorConfig.PlayerMarkerColor;
            }
        }
    }
}
