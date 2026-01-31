using UnityEngine;
using UnityEngine.InputSystem;

namespace GlobalGameJam
{
    public class MapDisplayView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private GameObject minimapUI;
        [SerializeField] private GameObject fullMapUI;
        
        [Header("Animation Parameters")]
        [SerializeField] private string pullOutMapTrigger = "PullOutMap";
        [SerializeField] private string putAwayMapTrigger = "PutAwayMap";
        [SerializeField] private string isHoldingMapBool = "IsHoldingMap";
        
        [Header("Settings")]
        [SerializeField] private float delayBeforeShowingUI = 0.5f;
        
        private MapViewModel viewModel;

        private void Awake()
        {
            viewModel = new MapViewModel();
            
            // Subscribe to ViewModel events
            viewModel.OnMapOpened += HandleMapOpened;
            viewModel.OnMapClosed += HandleMapClosed;
            
            // Initialize UI state
            if (minimapUI != null)
            {
                minimapUI.SetActive(true);
            }
            else
            {
                Debug.LogError("[MapDisplayView] minimapUI is not assigned!");
            }

            if (fullMapUI != null)
            {
                fullMapUI.SetActive(false);
            }
            
            if (playerAnimator == null)
            {
                Debug.LogError("[MapDisplayView] playerAnimator is not assigned!");
            }
            
            Debug.Log("[MapDisplayView] Initialized successfully");
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            viewModel.OnMapOpened -= HandleMapOpened;
            viewModel.OnMapClosed -= HandleMapClosed;
        }

        private void Update()
        {
            // Handle input (New Input System)
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                Debug.Log("[MapDisplayView] M key pressed!");
                viewModel.ToggleMap();
            }
        }

        private void HandleMapOpened()
        {
            Debug.Log("[MapDisplayView] HandleMapOpened called");
            
            // Play animation
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(pullOutMapTrigger);
                playerAnimator.SetBool(isHoldingMapBool, true);
            }
            
            // Show UI after delay
            Invoke(nameof(ShowMapUI), delayBeforeShowingUI);
        }

        private void HandleMapClosed()
        {
            Debug.Log("[MapDisplayView] HandleMapClosed called");
            
            // Play animation
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(putAwayMapTrigger);
                playerAnimator.SetBool(isHoldingMapBool, false);
            }
            
            // Hide full map immediately
            if (fullMapUI != null)
            {
                fullMapUI.SetActive(false);
            }
            
            // Cancel pending ShowMapUI if map closed quickly
            CancelInvoke(nameof(ShowMapUI));
        }

        private void ShowMapUI()
        {
            Debug.Log($"[MapDisplayView] ShowMapUI called. IsMapOpen: {viewModel.IsMapOpen}");
            
            if (viewModel.IsMapOpen && fullMapUI != null)
            {
                fullMapUI.SetActive(true);
                Debug.Log("[MapDisplayView] Full map UI activated!");
            }
        }
        
        // Public API for external calls (optional)
        public void OpenMap() => viewModel.OpenMap();
        public void CloseMap() => viewModel.CloseMap();
    }
}
