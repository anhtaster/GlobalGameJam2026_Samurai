using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Trigger zone that plays a narrator line when player enters.
    /// Used for tutorial checkpoints and follow-up guidance.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TutorialTriggerZone : MonoBehaviour
    {
        [Header("Narrator Configuration")]
        [SerializeField] private string narratorText = "Good work! Continue exploring to find your escape.";
        [SerializeField] private AudioClip narratorClip;
        [SerializeField] private float narratorDuration = 4f;

        [Header("Dependencies")]
        [SerializeField] private TutorialViewModel tutorialViewModel;

        [Header("Trigger Settings")]
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private bool disableAfterTrigger = true;

        private bool hasTriggered = false;

        private void Start()
        {
            // Auto-find TutorialViewModel if not assigned
            if (tutorialViewModel == null)
            {
                tutorialViewModel = FindFirstObjectByType<TutorialViewModel>();
            }

            if (tutorialViewModel == null)
            {
                Debug.LogWarning("[TutorialTriggerZone] TutorialViewModel not found!");
            }

            // Ensure this has a trigger collider
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"[TutorialTriggerZone] Collider on {gameObject.name} is not set as trigger! Setting isTrigger = true.");
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if already triggered
            if (triggerOnce && hasTriggered)
                return;

            // Check if player
            if (!other.CompareTag("Player"))
                return;

            Debug.Log($"[TutorialTriggerZone] Player entered trigger zone: {gameObject.name}");

            // Play narrator
            if (tutorialViewModel != null && !string.IsNullOrEmpty(narratorText))
            {
                tutorialViewModel.PlayNarrative(narratorText, narratorClip, narratorDuration);
                Debug.Log($"[TutorialTriggerZone] Playing narrator: '{narratorText}'");
            }

            hasTriggered = true;

            // Optionally disable this GameObject after trigger
            if (disableAfterTrigger)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            // Draw trigger zone bounds in editor
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = hasTriggered ? new Color(0, 1, 0, 0.3f) : new Color(1, 1, 0, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;

                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
