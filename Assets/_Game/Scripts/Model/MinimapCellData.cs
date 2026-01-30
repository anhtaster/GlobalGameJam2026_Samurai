using UnityEngine;

namespace GlobalGameJam
{
    /// <summary>
    /// Represents the type of cell in the minimap grid
    /// </summary>
    public enum CellType
    {
        Empty,   // No object (outside playable area)
        Floor,   // Walkable floor
        Wall     // Wall or obstacle (includes doors)
    }

    /// <summary>
    /// Data for a single cell in the minimap grid
    /// </summary>
    [System.Serializable]
    public class MinimapCellData
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private CellType cellType;
        [SerializeField] private GameObject worldObject; // Reference to 3D object

        public Vector2Int GridPosition => gridPosition;
        public CellType CellType => cellType;
        public GameObject WorldObject => worldObject;

        public MinimapCellData(Vector2Int position, CellType type, GameObject worldObj = null)
        {
            gridPosition = position;
            cellType = type;
            worldObject = worldObj;
        }

        public void SetCellType(CellType type)
        {
            cellType = type;
        }

        public void SetWorldObject(GameObject obj)
        {
            worldObject = obj;
        }
    }
}
