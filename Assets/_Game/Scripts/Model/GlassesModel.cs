using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Model: Chứa state của kính (đang đeo hay không) và màu kính
    /// </summary>
    [CreateAssetMenu(fileName = "GlassesModel", menuName = "GlobalGameJam/Player/Glasses Model")]
    public class GlassesModel : ScriptableObject
    {
        [SerializeField] private bool isWearingGlasses = false;
        [SerializeField] private GlassColor currentGlassColor = GlassColor.Red;

        public bool IsWearingGlasses
        {
            get => isWearingGlasses;
            set => isWearingGlasses = value;
        }

        public GlassColor CurrentGlassColor
        {
            get => currentGlassColor;
            set => currentGlassColor = value;
        }

        /// <summary>
        /// Reset state về mặc định (không đeo kính)
        /// </summary>
        public void ResetState()
        {
            isWearingGlasses = false;
            currentGlassColor = GlassColor.Red;
        }
    }

    /// <summary>
    /// Enum cho các màu kính có thể đeo
    /// </summary>
    public enum GlassColor
    {
        Red,
        Green,
        Blue
    }
}