using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace GlobalGameJam
{
    public class TutorialPickupView : MonoBehaviour
    {
        public enum PickupType
        {
            MapModel,
            Minimap,
            MapToggle,
            Glasses
        }

        [Header("Pickup Configuration")]
        [SerializeField] private PickupType pickupType;
        [SerializeField] private string narratorText;
        [SerializeField] private AudioClip narratorClip;

        [Header("Visual")]
        [SerializeField] private GameObject visualObject;

        [Header("Dependencies")]
        [SerializeField] private TutorialProgressionViewModel progressionViewModel;

        [Header("Highlight Effect")]
        [SerializeField] private Outline outlineComponent;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float outlineWidth = 5f;

        [Header("Pickup Prompt UI")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private string promptMessage = "Press F to pick up";

        [Header("Minimap Marker")]
        [SerializeField] private MinimapPickupMarker minimapMarker;

        private bool hasBeenPickedUp = false;
        private bool playerInRange = false;

        private void Start()
        {
            // Auto-find ViewModel if not assigned
            if (progressionViewModel == null)
            {
                progressionViewModel = FindFirstObjectByType<TutorialProgressionViewModel>();
            }

            if (progressionViewModel == null)
            {
                Debug.LogError($"[TutorialPickupView] TutorialProgressionViewModel not found for {pickupType}!");
            }

            // Auto-find MinimapPickupMarker if not assigned
            if (minimapMarker == null)
            {
                minimapMarker = FindFirstObjectByType<MinimapPickupMarker>();
            }

            // Subscribe to unlock events for sequential marker appearance
            if (progressionViewModel != null && minimapMarker != null)
            {
                // MapToggle marker appears when Minimap is unlocked
                if (pickupType == PickupType.MapToggle)
                {
                    progressionViewModel.OnMinimapUnlocked += ShowMarkerOnMap;
                    Debug.Log($"[TutorialPickupView] {pickupType} subscribed to OnMinimapUnlocked event");
                }
                // Glasses marker appears when MapToggle is unlocked
                else if (pickupType == PickupType.Glasses)
                {
                    progressionViewModel.OnMapToggleUnlocked += ShowMarkerOnMap;
                    Debug.Log($"[TutorialPickupView] {pickupType} subscribed to OnMapToggleUnlocked event");
                }
            }
            else
            {
                if (progressionViewModel == null)
                    Debug.LogWarning($"[TutorialPickupView] {pickupType} - ProgressionViewModel is null, cannot subscribe to events!");
                if (minimapMarker == null)
                    Debug.LogWarning($"[TutorialPickupView] {pickupType} - MinimapMarker is null, cannot register markers!");
            }

            // Setup Outline component
            if (outlineComponent == null)
            {
                outlineComponent = GetComponent<Outline>();
            }

            if (outlineComponent == null)
            {
                // Auto-add if not present
                outlineComponent = gameObject.AddComponent<Outline>();
            }

            // Configure outline
            if (outlineComponent != null)
            {
                outlineComponent.OutlineMode = Outline.Mode.OutlineVisible;
                outlineComponent.OutlineColor = highlightColor;
                outlineComponent.OutlineWidth = outlineWidth;
                outlineComponent.enabled = true;
            }

            // Setup prompt UI
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            if (promptText != null && string.IsNullOrEmpty(promptText.text))
            {
                promptText.text = promptMessage;
            }
        }

        private void Update()
        {
            if (playerInRange && !hasBeenPickedUp)
            {
                // Check for F key press
                if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                {
                    Debug.Log($"[TutorialPickupView] F key pressed for {pickupType}");
                    PickUp();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenPickedUp) return;

            Debug.Log($"[TutorialPickupView] Trigger entered by: {other.name}, Tag: {other.tag}");

            // Check if player
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[TutorialPickupView] Player entered range of {pickupType}");
                playerInRange = true;
                ShowPrompt();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"[TutorialPickupView] Player exited range of {pickupType}");
                playerInRange = false;
                HidePrompt();
            }
        }

        private void ShowPrompt()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(true);
                Debug.Log($"[TutorialPickupView] Prompt shown for {pickupType}");
            }
            else
            {
                Debug.LogWarning($"[TutorialPickupView] Prompt UI is not assigned for {pickupType}!");
            }
        }

        private void HidePrompt()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(false);
                Debug.Log($"[TutorialPickupView] Prompt hidden for {pickupType}");
            }
        }

        private void PickUp()
        {
            if (progressionViewModel == null) return;

            hasBeenPickedUp = true;
            playerInRange = false;

            // Hide prompt
            HidePrompt();

            // Disable outline
            if (outlineComponent != null)
            {
                outlineComponent.enabled = false;
            }

            // Remove minimap marker
            if (minimapMarker != null && (pickupType == PickupType.MapToggle || pickupType == PickupType.Glasses))
            {
                minimapMarker.RemovePickup(transform.position);
            }

            // Call appropriate unlock method
            switch (pickupType)
            {
                case PickupType.MapModel:
                    progressionViewModel.UnlockMapModel(narratorText, narratorClip);
                    // Enable Arm2 model
                    var mapController = FindFirstObjectByType<MapController>();
                    if (mapController != null)
                    {
                        mapController.EnableMapModel();
                    }
                    break;

                case PickupType.Minimap:
                    progressionViewModel.UnlockMinimap(narratorText, narratorClip);
                    break;

                case PickupType.MapToggle:
                    progressionViewModel.UnlockMapToggle(narratorText, narratorClip);
                    break;

                case PickupType.Glasses:
                    progressionViewModel.UnlockGlasses(narratorText, narratorClip);
                    break;
            }

            // Hide visual
            if (visualObject != null)
            {
                visualObject.SetActive(false);
            }
            else
            {
                // If no specific visual object, hide self
                gameObject.SetActive(false);
            }

            Debug.Log($"[TutorialPickupView] Picked up {pickupType}");
        }

        private void ShowMarkerOnMap()
        {
            Debug.Log($"[TutorialPickupView] ShowMarkerOnMap called for {pickupType}. HasBeenPickedUp: {hasBeenPickedUp}, MinimapMarker null: {minimapMarker == null}");
            
            if (minimapMarker != null && !hasBeenPickedUp)
            {
                minimapMarker.RegisterPickup(transform.position);
                Debug.Log($"[TutorialPickupView] {pickupType} marker registered on minimap at position {transform.position}");
            }
            else
            {
                if (minimapMarker == null)
                    Debug.LogWarning($"[TutorialPickupView] {pickupType} - Cannot show marker, MinimapMarker is null!");
                if (hasBeenPickedUp)
                    Debug.LogWarning($"[TutorialPickupView] {pickupType} - Cannot show marker, already picked up!");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (progressionViewModel != null)
            {
                if (pickupType == PickupType.MapToggle)
                {
                    progressionViewModel.OnMinimapUnlocked -= ShowMarkerOnMap;
                }
                else if (pickupType == PickupType.Glasses)
                {
                    progressionViewModel.OnMapToggleUnlocked -= ShowMarkerOnMap;
                }
            }
        }
    }
}
