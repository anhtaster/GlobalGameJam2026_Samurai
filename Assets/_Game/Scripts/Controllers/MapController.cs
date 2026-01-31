using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace GlobalGameJam
{
    public class MapController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private string pullOutMapTrigger = "PullOutMap";
        [SerializeField] private string holdingMapParameter = "HoldingMap";
        [SerializeField] private string putAwayMapTrigger = "PutAwayMap";
        [SerializeField] private string zoomInMapTrigger = "ZoomInMap";
        [SerializeField] private string zoomOutMapTrigger = "ZoomOutMap";

        [Header("Map 3D Model")]
        [SerializeField] private GameObject mapObject; // Reference đến map 3D model (nhắc trong Editor)
        [SerializeField] private string mapObjectNameHint = "Map";
        [SerializeField] private string[] mapObjectNameHints = new[] { "Map", "Paper", "MapPaper", "Map_Paper" };
        [SerializeField] private bool forceMapLayerToDefault = false;
        [SerializeField] private bool forceTreatAsLevel2 = false;

        [Header("Cross-Controller References")]
        [SerializeField] private GlassesController glassesController;

        [Header("3D Map Sheet")]
        [SerializeField] private MapSheetRenderer mapSheetRenderer;
        [SerializeField] private float showMapDelay = 0.08f;

        [Header("Mode")]
        [SerializeField] private bool alwaysHoldingMap = true;
        [SerializeField] private GameObject arm2Model;
        [SerializeField] private TutorialProgressionModel progressionModel;

        private Animator animator;
        private bool isMapOpen = false;
        private bool isTransitioning = false;
        private Renderer[] cachedMapRenderers;

        /// <summary>
        /// Kiểm tra xem đang ở level 1 hay không
        /// Level 1 có build index = 1 (0 là main menu)
        /// </summary>
        public bool IsTutorialLevel()
        {
            var scene = SceneManager.GetActiveScene();
            string sceneName = scene.name.ToLowerInvariant();

            // Nếu scene là Level 1/tutorial thì luôn coi là tutorial (bỏ qua override)
            if (sceneName.Contains("level1") || sceneName.Contains("tutorial") || scene.buildIndex == 1)
            {
                return true;
            }

            // Cho phép override chỉ ở Level 2+
            if (forceTreatAsLevel2) return false;

            // Nếu build index >= 2 thì chắc chắn là Level 2+
            if (scene.buildIndex >= 2)
            {
                return false;
            }

            // Fallback: main menu không phải tutorial
            return false;
        }

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

            // Auto-find map object nếu chưa gán (hữu ích khi copy arm sang level khác)
            if (mapObject == null)
            {
                mapObject = AutoFindMapObject();
            }

            if (mapObject != null)
            {
                cachedMapRenderers = mapObject.GetComponentsInChildren<Renderer>(true);
                LogMapState("Start");
            }

            // Luôn ẩn map cho đến khi nhặt item
            if (mapObject != null)
            {
                mapObject.SetActive(false);
                DisableMapRenderers(mapObject);
                Debug.Log("[MapController] Map object hidden initially (wait for pickup)");
            }
            else
            {
                Debug.LogWarning("[MapController] Map object chưa được gán và không tự tìm thấy. Hãy kéo map 3D object vào field 'Map Object' trong Inspector.");
            }
        }

        private void LateUpdate()
        {
            // No auto-forcing visibility. Map shows only after pickup.
        }

        // Removed auto-force visibility methods (map appears only after pickup)

        private void Update()
        {
            SyncMapSheetVisibility();
        }

        private void SyncMapSheetVisibility()
        {
            if (mapSheetRenderer == null || animator == null) return;

            bool shouldShow;
            if (alwaysHoldingMap)
            {
                shouldShow = true;
            }
            else
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                shouldShow = stateInfo.IsName("PullOutMap") ||
                              stateInfo.IsName("HoldingMap") ||
                              stateInfo.IsName("PutAwayMap");
            }

            if (shouldShow)
            {
                if (!mapVisible)
                {
                    if (showAtTime < 0f)
                    {
                        showAtTime = Time.time + showMapDelay;
                    }

                    if (Time.time >= showAtTime)
                    {
                        mapSheetRenderer.ShowMap();
                        mapVisible = true;
                        showAtTime = -1f;
                    }
                }
            }
            else
            {
                if (mapVisible)
                {
                    mapSheetRenderer.HideMap();
                    mapVisible = false;
                }
                showAtTime = -1f;
            }
        }

        // Public methods để MinimapInteractionController gọi
        public void OpenMapExternal()
        {
            LogMapState("OpenMapExternal");
            // Đảm bảo map object được tìm thấy nếu chưa gán
            if (mapObject == null)
            {
                mapObject = AutoFindMapObject();
            }

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
                if (IsTutorialLevel())
                {
                    // Level 1: Play animation lấy map
                    OpenMap();
                }
                else
                {
                    // Level 2+: Chỉ zoom in (map đã được giữ sẵn)
                    ZoomInMap();
                }
            }
            else
            {
                // Debug.LogWarning($"[MapController] Cannot open - isMapOpen={isMapOpen}, isTransitioning={isTransitioning}");
            }
        }

        public void CloseMapExternal()
        {
            if (alwaysHoldingMap) return;
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
                if (IsTutorialLevel())
                {
                    // Level 1: Play animation cất map
                    CloseMap();
                }
                else
                {
                    // Level 2+: Chỉ zoom out (map vẫn giữ)
                    ZoomOutMap();
                }
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

            // Show map sẽ được gọi từ Animation Event trong PullOutMap animation

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

            // Hide map sẽ được gọi từ Animation Event trong PutAwayMap animation

            // Reset transition flag after animation completes
            Invoke(nameof(ResetTransition), 0.5f);
        }

        private void ResetTransition()
        {
            isTransitioning = false;
        }

        // ===== PUBLIC METHODS =====
        
        /// <summary>
        /// Hiển thị map 3D object (gọi khi nhặt map item)
        /// </summary>
        public void ShowMapObject()
        {
            if (mapObject != null && !mapObject.activeSelf)
            {
                mapObject.SetActive(true);
                EnsureMapVisible(mapObject);
                Debug.Log("[MapController] Map object shown (picked up item)");
            }
        }

        /// <summary>
        /// Hiển thị map lần đầu tiên với animation (khi nhặt map item)
        /// </summary>
        public void ShowMapForFirstTime()
        {
            // Hiển thị map 3D object
            ShowMapObject();
            
            // Trigger animation lấy map ra
            if (animator != null)
            {
                Debug.Log("[MapController] First time picking up map - Playing PullOutMap animation");
                isMapOpen = true;
                isTransitioning = true;
                
                // Trigger PullOutMap animation
                animator.SetTrigger(pullOutMapTrigger);
                
                // Sau đó giữ map
                Invoke(nameof(StartHoldingMap), 0.1f);
            }
        }

        private void EnsureMapVisible(GameObject target)
        {
            if (target == null) return;

            // Bật toàn bộ parent chain để tránh bị tắt từ root
            Transform current = target.transform;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    current.gameObject.SetActive(true);
                }
                current = current.parent;
            }

            // Bật tất cả Renderer để chắc chắn hiển thị
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = true;
            }

            if (forceMapLayerToDefault)
            {
                SetLayerRecursively(target, 0);
            }

            // Log nhanh để debug
            Debug.Log($"[MapController] EnsureMapVisible -> renderers: {renderers.Length}, activeSelf: {target.activeSelf}");
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                if (child != null) SetLayerRecursively(child.gameObject, layer);
            }
        }

        private void DisableMapRenderers(GameObject target)
        {
            if (target == null) return;
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = false;
            }
        }


        private void LogMapState(string context)
        {
            if (mapObject == null)
            {
                Debug.LogWarning($"[MapController] {context}: mapObject is NULL");
                return;
            }

            Camera cam = Camera.main;
            int layer = mapObject.layer;
            bool camRendersLayer = cam != null && ((cam.cullingMask & (1 << layer)) != 0);
            int rendererCount = mapObject.GetComponentsInChildren<Renderer>(true).Length;

            Debug.Log($"[MapController] {context}: mapObject='{mapObject.name}', activeSelf={mapObject.activeSelf}, layer={layer}, camRendersLayer={camRendersLayer}, renderers={rendererCount}");
        }

        private GameObject AutoFindMapObject()
        {
            // Chuẩn hóa danh sách hint
            List<string> hints = new List<string>();
            if (!string.IsNullOrEmpty(mapObjectNameHint)) hints.Add(mapObjectNameHint);
            if (mapObjectNameHints != null)
            {
                foreach (var h in mapObjectNameHints)
                {
                    if (!string.IsNullOrEmpty(h)) hints.Add(h);
                }
            }

            // Ưu tiên tìm theo tên hint trong children (kể cả inactive)
            if (hints.Count > 0)
            {
                Transform[] children = GetComponentsInChildren<Transform>(true);
                foreach (var child in children)
                {
                    if (child != null)
                    {
                        foreach (var h in hints)
                        {
                            if (child.name.Contains(h))
                            {
                                Debug.Log($"[MapController] AutoFindMapObject found in children: {child.name}");
                                return child.gameObject;
                            }
                        }
                    }
                }
            }

            // Tìm theo tên hint trong toàn scene (kể cả inactive)
            if (hints.Count > 0)
            {
                Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var t in all)
                {
                    if (t != null)
                    {
                        foreach (var h in hints)
                        {
                            if (t.name.Contains(h))
                            {
                                Debug.Log($"[MapController] AutoFindMapObject found in scene: {t.name}");
                                return t.gameObject;
                            }
                        }
                    }
                }
            }

            // Fallback: tìm child có Renderer để hiển thị (tránh root)
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null && r.gameObject != this.gameObject)
                {
                    Debug.Log($"[MapController] AutoFindMapObject fallback renderer: {r.gameObject.name}");
                    return r.gameObject;
                }
            }

            return null;
        }

        // ===== ZOOM METHODS (Level 2+) =====
        
        private void ZoomInMap()
        {
            if (animator == null)
            {
                Debug.LogError("[MapController] Animator is NULL!");
                return;
            }

            Debug.Log("[MapController] Zooming In Map (Level 2+)");
            isMapOpen = true;
            isTransitioning = true;

            // Trigger ZoomInMap animation
            animator.SetTrigger(zoomInMapTrigger);

            // Reset transition flag after animation
            Invoke(nameof(ResetTransition), 0.5f);
        }

        private void ZoomOutMap()
        {
            if (animator == null)
            {
                Debug.LogError("[MapController] Animator is NULL!");
                return;
            }

            Debug.Log("[MapController] Zooming Out Map (Level 2+)");
            isMapOpen = false;
            isTransitioning = true;

            // Trigger ZoomOutMap animation
            animator.SetTrigger(zoomOutMapTrigger);

            // Reset transition flag after animation
            Invoke(nameof(ResetTransition), 0.5f);
        }
    }
}
