using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Script này đặt trực tiếp vào object có Animator để nhận Animation Events
    /// </summary>
    public class MapAnimationEvents : MonoBehaviour
    {
        [SerializeField] private MapController mapController;

        private void Start()
        {
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
        }

        // Gọi từ Animation Event trong PullOutMap
        public void ShowMapSheet()
        {
            if (mapController != null)
            {
                mapController.ShowMapSheet();
            }
        }

        // Gọi từ Animation Event trong PutAwayMap
        public void HideMapSheet()
        {
            if (mapController != null)
            {
                mapController.HideMapSheet();
            }
        }
    }
}
