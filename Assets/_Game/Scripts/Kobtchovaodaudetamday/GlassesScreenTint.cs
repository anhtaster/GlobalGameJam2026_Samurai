using UnityEngine;
using UnityEngine.UI;

namespace GlobalGameJam
{
    /// <summary>
    /// Tạo hiệu ứng màu overlay lên toàn màn hình khi đeo kính
    /// Giống như đeo kính râm có màu (red/green/blue tint)
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Tạo UI Image full screen trong Canvas
    /// 2. Add component này vào Image đó
    /// 3. Set Canvas Sort Order cao để overlay lên toàn bộ màn hình
    /// 4. Chỉnh màu trong Inspector nếu cần
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class GlassesScreenTint : MonoBehaviour
    {
        [Header("Tint Colors (Adjust alpha for intensity)")]
        [SerializeField] private Color redTint = new Color(1f, 0.2f, 0.2f, 0.25f);
        [SerializeField] private Color greenTint = new Color(0.2f, 1f, 0.2f, 0.25f);
        [SerializeField] private Color blueTint = new Color(0.2f, 0.4f, 1f, 0.25f);

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float colorTransitionDuration = 0.25f;

        private Image tintImage;
        private Color targetColor;
        private Color currentColor;
        private float transitionTimer;
        private bool isTransitioning;
        private float transitionDuration;

        void Awake()
        {
            tintImage = GetComponent<Image>();
            
            // Start invisible
            Color clearColor = redTint;
            clearColor.a = 0f;
            tintImage.color = clearColor;
            currentColor = clearColor;
            targetColor = clearColor;
        }

        void Update()
        {
            if (isTransitioning)
            {
                transitionTimer += Time.deltaTime;
                float t = Mathf.Clamp01(transitionTimer / transitionDuration);
                
                // Smooth lerp
                currentColor = Color.Lerp(currentColor, targetColor, t);
                tintImage.color = currentColor;

                if (t >= 1f)
                {
                    isTransitioning = false;
                    currentColor = targetColor; // Ensure exact target color
                    tintImage.color = currentColor;
                }
            }
        }

        /// <summary>
        /// Hiển thị tint với màu cụ thể
        /// </summary>
        public void ShowTint(GlassColor glassColor)
        {
            targetColor = GetColorForGlassType(glassColor);
            transitionDuration = fadeInDuration;
            StartTransition();
        }

        /// <summary>
        /// Ẩn tint (fade out)
        /// </summary>
        public void HideTint()
        {
            targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            transitionDuration = fadeOutDuration;
            StartTransition();
        }

        /// <summary>
        /// Đổi màu tint (khi đang đeo kính)
        /// </summary>
        public void ChangeTintColor(GlassColor glassColor)
        {
            targetColor = GetColorForGlassType(glassColor);
            transitionDuration = colorTransitionDuration;
            StartTransition();
        }

        private Color GetColorForGlassType(GlassColor glassColor)
        {
            return glassColor switch
            {
                GlassColor.Red => redTint,
                GlassColor.Green => greenTint,
                GlassColor.Blue => blueTint,
                _ => redTint
            };
        }

        private void StartTransition()
        {
            transitionTimer = 0f;
            isTransitioning = true;
        }

        /// <summary>
        /// Public API: Set custom colors if needed
        /// </summary>
        public void SetRedTint(Color color) => redTint = color;
        public void SetGreenTint(Color color) => greenTint = color;
        public void SetBlueTint(Color color) => blueTint = color;
    }
}