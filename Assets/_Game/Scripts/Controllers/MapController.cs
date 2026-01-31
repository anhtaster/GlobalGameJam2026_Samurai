using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    public class MapController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private string pullOutMapTrigger = "PullOutMap";
        [SerializeField] private string holdingMapParameter = "HoldingMap";
        [SerializeField] private string putAwayMapTrigger = "PutAwayMap";

        [Header("Cross-Controller References")]
        [SerializeField] private GlassesController glassesController;

        private Animator animator;
        private bool isMapOpen = false;
        private bool isTransitioning = false;

        private void Start()
        {
            // Tìm Animator trên GameObject hiện tại
            animator = GetComponent<Animator>();

            // Nếu không có, tìm trên child objects
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Nếu vẫn không có, tìm trên parent objects
            if (animator == null)
            {
                animator = GetComponentInParent<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError("[MapController] Không tìm thấy Animator! Hãy chắc chắn GameObject này hoặc các child/parent của nó có Animator component.");
            }

            // Tìm GlassesController nếu chưa gán
            if (glassesController == null)
            {
                glassesController = GetComponent<GlassesController>();
            }
            if (glassesController == null)
            {
                glassesController = GetComponentInChildren<GlassesController>();
            }
            if (glassesController == null)
            {
                glassesController = GetComponentInParent<GlassesController>();
            }
            if (glassesController == null)
            {
                glassesController = FindFirstObjectByType<GlassesController>();
            }
        }

        private void Update()
        {
            // Disabled - handled by MinimapInteractionController
        }

        // Public methods để MinimapInteractionController gọi
        public void OpenMapExternal()
        {
            // Không cho mở map khi đang trong glasses animation
            if (glassesController != null && animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                
                if (stateInfo.IsName("PutOnGlasses") || stateInfo.IsName("PutOutGlasses"))
                {
                    // Debug.Log("[MapController] Cannot open map while toggling glasses");
                    return;
                }
            }

            // Debug.Log("[MapController] OpenMapExternal called");
            if (!isMapOpen && !isTransitioning)
            {
                OpenMap();
            }
            else
            {
                // Debug.LogWarning($"[MapController] Cannot open - isMapOpen={isMapOpen}, isTransitioning={isTransitioning}");
            }
        }

        public void CloseMapExternal()
        {
            // Không cho đóng map khi đang trong glasses animation
            if (glassesController != null && animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                
                if (stateInfo.IsName("PutOnGlasses") || stateInfo.IsName("PutOutGlasses"))
                {
                    // Debug.Log("[MapController] Cannot close map while toggling glasses");
                    return;
                }
            }

            // Debug.Log("[MapController] CloseMapExternal called");
            if (isMapOpen && !isTransitioning)
            {
                CloseMap();
            }
            else
            {
                // Debug.LogWarning($"[MapController] Cannot close - isMapOpen={isMapOpen}, isTransitioning={isTransitioning}");
            }
        }

        private void OpenMap()
        {
            if (animator == null)
            {
                Debug.LogError("[MapController] Animator is NULL!");
                return;
            }

            // Debug.Log("[MapController] Opening Map - Triggering PullOutMap animation");
            isMapOpen = true;
            isTransitioning = true;

            // Trigger PullOutMap animation
            animator.SetTrigger(pullOutMapTrigger);

            // After a short delay, set HoldingMap to true to enter the loop
            Invoke(nameof(StartHoldingMap), 0.1f);
        }

        private void StartHoldingMap()
        {
            if (animator == null) return;
            // Debug.Log("[MapController] Starting HoldingMap state");
            animator.SetBool(holdingMapParameter, true);
            isTransitioning = false;
        }

        private void CloseMap()
        {
            if (animator == null)
            {
                Debug.LogError("[MapController] Animator is NULL!");
                return;
            }

            // Debug.Log("[MapController] Closing Map - Triggering PutAwayMap animation");
            isMapOpen = false;
            isTransitioning = true;

            // Stop holding the map
            animator.SetBool(holdingMapParameter, false);

            // Trigger PutAwayMap animation
            animator.SetTrigger(putAwayMapTrigger);

            // Reset transition flag after animation completes
            Invoke(nameof(ResetTransition), 0.5f);
        }

        private void ResetTransition()
        {
            isTransitioning = false;
        }
    }
}
