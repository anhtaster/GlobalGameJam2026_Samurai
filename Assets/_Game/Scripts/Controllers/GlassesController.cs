using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    /// <summary>
    /// Controller/View: Xử lý input và trigger animation, bind với ViewModel
    /// </summary>
    public class GlassesController : MonoBehaviour
    {
        [Header("Model Reference")]
        [SerializeField] private GlassesModel model;

        [Header("Animation Settings")]
        [SerializeField] private string putOnGlassesTrigger = "PutOnGlasses";
        [SerializeField] private string putOutGlassesTrigger = "PutOutGlasses";
        [SerializeField] private string wearingGlassesBool = "WearingGlasses";

        [Header("Cross-Controller References")]
        [SerializeField] private MapController mapController;

        private Animator animator;
        private GlassesViewModel viewModel;

        void Start()
        {
            // Tìm Animator
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            if (animator == null)
            {
                animator = GetComponentInParent<Animator>();
            }
            if (animator == null)
            {
                Debug.LogWarning("[GlassesController] Animator not found! Animation will not play.");
            }

            // Tìm MapController nếu chưa gán
            if (mapController == null)
            {
                mapController = GetComponent<MapController>();
            }
            if (mapController == null)
            {
                mapController = GetComponentInChildren<MapController>();
            }
            if (mapController == null)
            {
                mapController = GetComponentInParent<MapController>();
            }
            if (mapController == null)
            {
                mapController = FindFirstObjectByType<MapController>();
            }

            // Tạo ViewModel từ Model
            if (model != null)
            {
                viewModel = new GlassesViewModel(model);
                
                // Subscribe vào events từ ViewModel
                viewModel.OnPutOnGlasses += HandlePutOnGlasses;
                viewModel.OnPutOutGlasses += HandlePutOutGlasses;
            }
            else
            {
                Debug.LogError("[GlassesController] GlassesModel is not assigned! Please create a GlassesModel asset and assign it.");
            }
        }

        void OnDestroy()
        {
            // Unsubscribe events
            if (viewModel != null)
            {
                viewModel.OnPutOnGlasses -= HandlePutOnGlasses;
                viewModel.OnPutOutGlasses -= HandlePutOutGlasses;
            }
        }

        void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// Xử lý input từ keyboard
        /// </summary>
        private void HandleInput()
        {
            if (Keyboard.current == null) return;

            // Bấm T để toggle kính
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (viewModel != null)
                {
                    viewModel.ToggleGlasses();
                }
            }
        }

        /// <summary>
        /// View handler: Trigger animation đeo kính
        /// </summary>
        private void HandlePutOnGlasses()
        {
            // Debug.Log("[GlassesController] Putting on glasses");
            
            if (animator != null)
            {
                animator.SetTrigger(putOnGlassesTrigger);
                animator.SetBool(wearingGlassesBool, true);
            }
        }

        /// <summary>
        /// View handler: Trigger animation tháo kính
        /// </summary>
        private void HandlePutOutGlasses()
        {
            // Debug.Log("[GlassesController] Taking off glasses");
            
            if (animator != null)
            {
                animator.SetTrigger(putOutGlassesTrigger);
                animator.SetBool(wearingGlassesBool, false);
            }
        }

        /// <summary>
        /// Public API: Bật kính từ code khác
        /// </summary>
        public void PutOnGlassesExternal()
        {
            if (viewModel != null && !viewModel.IsWearingGlasses)
            {
                viewModel.PutOnGlasses();
            }
        }

        /// <summary>
        /// Public API: Tháo kính từ code khác
        /// </summary>
        public void PutOutGlassesExternal()
        {
            if (viewModel != null && viewModel.IsWearingGlasses)
            {
                viewModel.PutOutGlasses();
            }
        }

        public bool IsWearingGlasses => viewModel != null && viewModel.IsWearingGlasses;
    }
}
