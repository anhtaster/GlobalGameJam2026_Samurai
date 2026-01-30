using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    public class MinimapPlayerMarkerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image markerImage;
        [SerializeField] private MinimapGridView gridView;
        [SerializeField] private Transform playerTransform;

        [Header("Settings")]
        [SerializeField] private MinimapColorConfig colorConfig;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool smoothMovement = true;

        private Vector2Int currentGridPosition;
        private Vector2Int previousGridPosition;
        private RectTransform markerRect; // Cache RectTransform

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
            if (playerTransform == null || gridView == null || gridView.GridModel == null)
                return;

            UpdatePlayerMarker();
        }

        private void UpdatePlayerMarker()
        {
            MinimapGridModel gridModel = gridView.GridModel;

            // Convert player world position to grid position
            currentGridPosition = GridCoordinateConverter.WorldToGrid(
                playerTransform.position,
                gridModel.GridOrigin,
                gridModel.CellSize
            );

            // Only update if position changed
            if (currentGridPosition != previousGridPosition)
            {
                UpdateMarkerPosition();
                previousGridPosition = currentGridPosition;
            }
        }

        private void UpdateMarkerPosition()
        {
            MinimapCellView cellView = gridView.GetCellView(currentGridPosition);
            if (cellView != null)
            {
                RectTransform targetRect = cellView.GetComponent<RectTransform>();
                if (targetRect != null && markerRect != null)
                {
                    if (smoothMovement)
                    {
                        // Smooth movement
                        markerRect.position = Vector3.Lerp(
                            markerRect.position,
                            targetRect.position,
                            Time.deltaTime * smoothSpeed
                        );
                    }
                    else
                    {
                        // Instant movement
                        markerRect.position = targetRect.position;
                    }
                }
            }
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void SetGridView(MinimapGridView view)
        {
            gridView = view;
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
