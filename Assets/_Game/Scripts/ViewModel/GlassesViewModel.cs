using System;

namespace GlobalGameJam
{
    /// <summary>
    /// ViewModel: Logic xử lý toggle kính, không biết về Unity Animation
    /// </summary>
    public class GlassesViewModel
    {
        private GlassesModel model;

        // Events để View subscribe
        public event Action OnPutOnGlasses;
        public event Action OnPutOutGlasses;

        public bool IsWearingGlasses => model != null && model.IsWearingGlasses;

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
    }
}
