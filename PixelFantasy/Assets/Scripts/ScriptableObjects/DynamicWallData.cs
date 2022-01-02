using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "DynamicWallData", menuName = "DynamicWallData", order = 1)]
    public class DynamicWallData : ScriptableObject
    {
        public Sprite solo;
        public Sprite vert;
        public Sprite hor;
        public Sprite inter;
        public Sprite cornerTR;
        public Sprite cornerBR;
        public Sprite cornerTL;
        public Sprite cornerBL;
        public Sprite endB;
        public Sprite endR;
        public Sprite endT;
        public Sprite endL;
        public Sprite interR;
        public Sprite interT;
        public Sprite interL;
        public Sprite interB;

        public Sprite GetWallSprite(WallNeighbourConnectionInfo neighbours)
        {
            // solo
            if (!neighbours.Top && !neighbours.Bottom && !neighbours.Left && !neighbours.Right)
            {
                return solo;
            }
            
            // vert
            if (neighbours.Top && neighbours.Bottom && !neighbours.Left && !neighbours.Right)
            {
                return vert;
            }
            
            // hor
            if (!neighbours.Top && !neighbours.Bottom && neighbours.Left && neighbours.Right)
            {
                return hor;
            }
            
            // inter
            if (neighbours.Top && neighbours.Bottom && neighbours.Left && neighbours.Right)
            {
                return inter;
            }
            
            // cornerTR
            if (!neighbours.Top && neighbours.Bottom && neighbours.Left && !neighbours.Right)
            {
                return cornerTR;
            }
            
            // cornerBR
            if (neighbours.Top && !neighbours.Bottom && neighbours.Left && !neighbours.Right)
            {
                return cornerBR;
            }
            
            // cornerTL
            if (!neighbours.Top && neighbours.Bottom && !neighbours.Left && neighbours.Right)
            {
                return cornerTL;
            }
            
            // cornerBL
            if (neighbours.Top && !neighbours.Bottom && !neighbours.Left && neighbours.Right)
            {
                return cornerBL;
            }
            
            // endB
            if (neighbours.Top && !neighbours.Bottom && !neighbours.Left && !neighbours.Right)
            {
                return endB;
            }
            
            // endR
            if (!neighbours.Top && !neighbours.Bottom && neighbours.Left && !neighbours.Right)
            {
                return endR;
            }
            
            // endT
            if (!neighbours.Top && neighbours.Bottom && !neighbours.Left && !neighbours.Right)
            {
                return endT;
            }
            
            // endL
            if (!neighbours.Top && !neighbours.Bottom && !neighbours.Left && neighbours.Right)
            {
                return endL;
            }
            
            // interR
            if (neighbours.Top && neighbours.Bottom && neighbours.Left && !neighbours.Right)
            {
                return interR;
            }
            
            // interT
            if (!neighbours.Top && neighbours.Bottom && neighbours.Left && neighbours.Right)
            {
                return interT;
            }
            
            // interL
            if (neighbours.Top && neighbours.Bottom && !neighbours.Left && neighbours.Right)
            {
                return interL;
            }
            
            // interB
            if (neighbours.Top && !neighbours.Bottom && neighbours.Left && neighbours.Right)
            {
                return interB;
            }

            // None determined, send error
            Debug.LogError($"Unknown wall orientation:\nTop: {neighbours.Top}\nBottom: {neighbours.Bottom}\nLeft: {neighbours.Left}\nRight: {neighbours.Right}");
            return null;
        }
    }

    /// <summary>
    /// Directional bools if walls are present
    /// </summary>
    public class WallNeighbourConnectionInfo
    {
        public bool Top;
        public bool Bottom;
        public bool Left;
        public bool Right;
    }
}
