using System;

namespace GlobalGameJam
{
    /// <summary>
    /// ViewModel: Logic xử lý toggle kính và đổi màu, không biết về Unity Animation
    /// </summary>
    public class GlassesViewModel
    {
        private GlassesModel model;

        // Events để View subscribe
        public event Action OnPutOnGlasses;
        public event Action OnPutOutGlasses;
        public event Action<GlassColor> OnGlassColorChanged;

        public bool IsWearingGlasses => model != null && model.IsWearingGlasses;
        public GlassColor CurrentGlassColor => model != null ? model.CurrentGlassColor : GlassColor.Red;

        public GlassesViewModel(GlassesModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Toggle trạng thái đeo/tháo kính
        /// </summary>
        public void ToggleGlasses()
        {
            if (model == null)
            {
                UnityEngine.Debug.LogError("[GlassesViewModel] Model is NULL!");
                return;
            }

            if (!model.IsWearingGlasses)
            {
                PutOnGlasses();
            }
            else
            {
                PutOutGlasses();
            }
        }

        /// <summary>
        /// Đeo kính
        /// </summary>
        public void PutOnGlasses()
        {
            if (model == null) return;

            model.IsWearingGlasses = true;
            OnPutOnGlasses?.Invoke();
        }

        /// <summary>
        /// Tháo kính
        /// </summary>
        public void PutOutGlasses()
        {
            if (model == null) return;

            model.IsWearingGlasses = false;
            OnPutOutGlasses?.Invoke();
        }

        /// <summary>
        /// Đổi màu kính (chỉ có hiệu lực khi đang đeo kính)
        /// </summary>
        public void ChangeGlassColor(GlassColor newColor)
        {
            if (model == null) return;

            if (model.CurrentGlassColor != newColor)
            {
                model.CurrentGlassColor = newColor;
                
                // Chỉ trigger event nếu đang đeo kính
                if (model.IsWearingGlasses)
                {
                    OnGlassColorChanged?.Invoke(newColor);
                }
            }
        }

        /// <summary>
        /// Cycle through colors: Red -> Green -> Blue -> Red
        /// </summary>
        public void CycleGlassColor()
        {
            if (model == null) return;

            GlassColor nextColor = model.CurrentGlassColor switch
            {
                GlassColor.Red => GlassColor.Green,
                GlassColor.Green => GlassColor.Blue,
                GlassColor.Blue => GlassColor.Red,
                _ => GlassColor.Red
            };

            ChangeGlassColor(nextColor);
        }
    }
}