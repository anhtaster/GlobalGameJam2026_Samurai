using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Model: Data representation for color block state
    /// Immutable data structure following SOLID principles
    /// </summary>
    [CreateAssetMenu(fileName = "ColorBlockModel", menuName = "GlobalGameJam/ColorBlockModel")]
    public class ColorBlockModel : ScriptableObject
    {
        [Header("Initial State")]
        [Tooltip("Which color block is active by default (0=none, 1=red, 2=green, 3=blue)")]
        [SerializeField] private int defaultActiveBlock = 0;

        /// <summary>
        /// Get the default active block index
        /// </summary>
        public int DefaultActiveBlock => defaultActiveBlock;
    }
}
