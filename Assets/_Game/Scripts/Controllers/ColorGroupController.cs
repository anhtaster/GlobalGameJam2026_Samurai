using UnityEngine;
using UnityEngine.InputSystem; // Bắt buộc cho Input System mới

namespace GlobalGameJam
{
    public class ColorGroupController : MonoBehaviour
    {
        [Header("Color blocks")]
        public GameObject rGroup;
        public GameObject gGroup;
        public GameObject bGroup;

        [Header("Minimap References")]
        [SerializeField] private MinimapTextureRenderer minimapTextureRenderer;
        [SerializeField] private GridScanner gridScanner; // Thêm tham chiếu đến Scanner

        void Start()
        {
            if(rGroup) rGroup.SetActive(false);
            if(gGroup) gGroup.SetActive(false);
            if(bGroup) bGroup.SetActive(false);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.rKey.wasPressedThisFrame) ToggleAndRefresh(rGroup);
            if (keyboard.gKey.wasPressedThisFrame) ToggleAndRefresh(gGroup);
            if (keyboard.bKey.wasPressedThisFrame) ToggleAndRefresh(bGroup);
        }

        public void ToggleR()
        {
            ToggleAndRefresh(rGroup);
        }

        private void ToggleAndRefresh(GameObject group)
        {
            if (group == null) return;

            group.SetActive(!group.activeSelf);

            if (gridScanner != null)
            {
                gridScanner.ScanScene();
            }

            // Gọi Renderer để vẽ lại tấm ảnh bản đồ mới dựa trên dữ liệu vừa quét
            if (minimapTextureRenderer != null)
            {
                // Thông thường hàm trong Renderer sẽ là Render(), Refresh() hoặc UpdateTexture()
                // Bạn hãy kiểm tra xem trong file MinimapTextureRenderer.cs có hàm nào tương tự không
                minimapTextureRenderer.RenderFullMap(); 
            }
        }
    }
}