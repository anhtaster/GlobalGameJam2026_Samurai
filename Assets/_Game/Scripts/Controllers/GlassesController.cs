using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    /// <summary>
    /// Controller/View: Xử lý input và trigger animation, bind với ViewModel
    /// Hỗ trợ đổi màu kính và hiệu ứng màn hình
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
        [SerializeField] private TutorialProgressionViewModel tutorialProgressionViewModel;
        [SerializeField] private ColorGroupController colorGroupController;

        [Header("Screen Tint (Optional)")]
        [SerializeField] private GlassesScreenTint screenTint;

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

            if (tutorialProgressionViewModel == null)
            {
                tutorialProgressionViewModel = FindFirstObjectByType<TutorialProgressionViewModel>();
            }

            // Tìm ColorGroupController nếu chưa gán
            if (colorGroupController == null)
            {
                colorGroupController = FindFirstObjectByType<ColorGroupController>();
            }

            // Tìm GlassesScreenTint nếu chưa gán
            if (screenTint == null)
            {
                screenTint = FindFirstObjectByType<GlassesScreenTint>();
                if (screenTint == null)
                {
                    Debug.LogWarning("[GlassesController] GlassesScreenTint not found! Screen overlay effect will not work. Create a UI Image with GlassesScreenTint component.");
                }
            }

            // Tạo ViewModel từ Model
            if (model != null)
            {
                viewModel = new GlassesViewModel(model);
                
                // Subscribe vào events từ ViewModel
                viewModel.OnPutOnGlasses += HandlePutOnGlasses;
                viewModel.OnPutOutGlasses += HandlePutOutGlasses;
                viewModel.OnGlassColorChanged += HandleGlassColorChanged;
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
                viewModel.OnGlassColorChanged -= HandleGlassColorChanged;
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
                // Check if Glasses is unlocked
                if (tutorialProgressionViewModel != null && !tutorialProgressionViewModel.IsGlassesUnlocked)
                {
                    Debug.Log("[GlassesController] Glasses are locked! Pick up the Glasses item first.");
                    return;
                }
                
                // Không cho bấm T khi đang mở map
                if (mapController != null && animator != null)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    
                    // Check nếu đang trong map animation thì không cho toggle glasses
                    if (stateInfo.IsName("PullOutMap") || 
                        stateInfo.IsName("HoldingMap") || 
                        stateInfo.IsName("PutAwayMap"))
                    {
                        Debug.Log("[GlassesController] Cannot toggle glasses while using map");
                        return;
                    }
                }

                if (viewModel != null)
                {
                    viewModel.ToggleGlasses();
                }
            }

            // Bấm C để cycle qua các màu kính (chỉ khi đang đeo kính)
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                if (viewModel != null && viewModel.IsWearingGlasses)
                {
                    viewModel.CycleGlassColor();
                }
                else
                {
                    Debug.Log("[GlassesController] Must be wearing glasses to change color! Press T first.");
                }
            }

            // Hoặc dùng số 1, 2, 3 để chọn màu cụ thể (khi đang đeo kính)
            if (viewModel != null && viewModel.IsWearingGlasses)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                {
                    viewModel.ChangeGlassColor(GlassColor.Red);
                }
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                {
                    viewModel.ChangeGlassColor(GlassColor.Green);
                }
                else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                {
                    viewModel.ChangeGlassColor(GlassColor.Blue);
                }
            }
        }

        /// <summary>
        /// View handler: Trigger animation đeo kính
        /// </summary>
        private void HandlePutOnGlasses()
        {
            Debug.Log($"[GlassesController] ✓ PUTTING ON GLASSES - Color: {viewModel.CurrentGlassColor}");
            
            if (animator != null)
            {
                animator.SetTrigger(putOnGlassesTrigger);
                animator.SetBool(wearingGlassesBool, true);
            }

            // Enable screen tint
            if (screenTint != null)
            {
                screenTint.ShowTint(viewModel.CurrentGlassColor);
            }

            // Notify color block controller to restore saved state
            if (colorGroupController != null)
            {
                colorGroupController.OnGlassesPutOn(viewModel.CurrentGlassColor);
            }
        }

        /// <summary>
        /// View handler: Trigger animation tháo kính
        /// </summary>
        private void HandlePutOutGlasses()
        {
            Debug.Log("[GlassesController] ✗ TAKING OFF GLASSES - No longer wearing glasses");
            
            if (animator != null)
            {
                animator.SetTrigger(putOutGlassesTrigger);
                animator.SetBool(wearingGlassesBool, false);
            }

            // Disable screen tint
            if (screenTint != null)
            {
                screenTint.HideTint();
            }

            // Notify color block controller to hide all blocks
            if (colorGroupController != null)
            {
                colorGroupController.OnGlassesPutOff();
            }
        }

        /// <summary>
        /// View handler: Handle glass color change
        /// </summary>
        private void HandleGlassColorChanged(GlassColor newColor)
        {
            Debug.Log($"[GlassesController] 🎨 GLASS COLOR CHANGED to {newColor}");

            // Update screen tint color
            if (screenTint != null)
            {
                screenTint.ChangeTintColor(newColor);
            }

            // Update visible platforms based on color
            if (colorGroupController != null)
            {
                colorGroupController.OnGlassColorChanged(newColor);
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

        /// <summary>
        /// Public API: Đổi màu kính từ code khác
        /// </summary>
        public void ChangeGlassColorExternal(GlassColor newColor)
        {
            if (viewModel != null)
            {
                viewModel.ChangeGlassColor(newColor);
            }
        }

        public bool IsWearingGlasses => viewModel != null && viewModel.IsWearingGlasses;
        public GlassColor CurrentGlassColor => viewModel != null ? viewModel.CurrentGlassColor : GlassColor.Red;
    }
}