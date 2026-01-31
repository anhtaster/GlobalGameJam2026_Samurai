using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Model: Chứa state của kính (đang đeo hay không)
    /// </summary>
    [CreateAssetMenu(fileName = "GlassesModel", menuName = "GlobalGameJam/Player/Glasses Model")]
    public class GlassesModel : ScriptableObject
    {
        [SerializeField] private bool isWearingGlasses = false;

        public bool IsWearingGlasses
        {
            get => isWearingGlasses;
            set => isWearingGlasses = value;
        }

        /// <summary>
        /// Reset state về mặc định (không đeo kính)
        /// </summary>
        public void ResetState()
        {
            isWearingGlasses = false;
        }
    }
}
