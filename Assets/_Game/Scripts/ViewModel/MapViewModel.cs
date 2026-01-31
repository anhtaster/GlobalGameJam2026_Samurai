using System;

namespace GlobalGameJam
{
    public class MapViewModel
    {
        public bool IsMapOpen { get; private set; }
        
        public event Action OnMapOpened;
        public event Action OnMapClosed;
        
        public void ToggleMap()
        {
            IsMapOpen = !IsMapOpen;
            
            if (IsMapOpen)
            {
                OnMapOpened?.Invoke();
            }
            else
            {
                OnMapClosed?.Invoke();
            }
        }
        
        public void OpenMap()
        {
            if (!IsMapOpen)
            {
                IsMapOpen = true;
                OnMapOpened?.Invoke();
            }
        }
        
        public void CloseMap()
        {
            if (IsMapOpen)
            {
                IsMapOpen = false;
                OnMapClosed?.Invoke();
            }
        }
    }
}
