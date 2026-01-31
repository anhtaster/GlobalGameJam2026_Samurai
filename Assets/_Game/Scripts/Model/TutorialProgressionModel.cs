using UnityEngine;

namespace GlobalGameJam
{
    [CreateAssetMenu(fileName = "TutorialProgressionModel", menuName = "GlobalGameJam/Tutorial/Progression Model")]
    public class TutorialProgressionModel : ScriptableObject
    {
        [Header("Unlock States")]
        public bool isMinimapUnlocked = false;
        public bool isMapToggleUnlocked = false;
        public bool isGlassesUnlocked = false;

        /// <summary>
        /// Resets all unlock states to false. Call this at the start of a new game/level.
        /// </summary>
        public void ResetProgress()
        {
            isMinimapUnlocked = false;
            isMapToggleUnlocked = false;
            isGlassesUnlocked = false;
        }
    }
}
