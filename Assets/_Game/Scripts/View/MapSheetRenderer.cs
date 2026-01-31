using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Quản lý việc hiện/ẩn model bản đồ 3D trên tay.
    /// Map luôn active và được animate trong các animation clips,
    /// script này chỉ điều khiển visibility khi cần (vd: đeo kính)
    /// </summary>
    public class MapSheetRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject mapSheetObject;
        [SerializeField] private Renderer[] mapRenderers;

        [Header("Minimap Texture (optional)")]
        [SerializeField] private MinimapTextureRenderer minimapRenderer;
        [SerializeField] private string texturePropertyName = "_MainTex";
        [SerializeField] private bool applyMinimapTexture = true;

        private void Start()
        {
            // Tự động tìm nếu chưa gán
            if (mapSheetObject == null)
            {
                mapSheetObject = gameObject;
            }

            // Tìm tất cả renderer để hide/show
            if (mapRenderers == null || mapRenderers.Length == 0)
            {
                mapRenderers = mapSheetObject.GetComponentsInChildren<Renderer>();
            }

            if (minimapRenderer == null)
            {
                minimapRenderer = FindFirstObjectByType<MinimapTextureRenderer>();
            }

            if (applyMinimapTexture)
            {
                RefreshTexture();
            }

            // Map luôn active, chỉ ẩn renderer
            HideMap();
        }

        public void ShowMap()
        {
            // Bật renderer thay vì SetActive để animation vẫn chạy
            foreach (var renderer in mapRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }

            if (applyMinimapTexture)
            {
                RefreshTexture();
            }
        }

        public void HideMap()
        {
            // Tắt renderer thay vì SetActive để animation vẫn chạy
            foreach (var renderer in mapRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        public void RefreshTexture()
        {
            if (minimapRenderer == null) return;
            if (minimapRenderer.MapTexture == null) return;

            foreach (var renderer in mapRenderers)
            {
                if (renderer == null) continue;
                var material = renderer.material;
                if (material != null)
                {
                    material.SetTexture(texturePropertyName, minimapRenderer.MapTexture);
                }
            }
        }
    }
}
