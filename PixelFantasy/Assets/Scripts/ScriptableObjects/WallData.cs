using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "WallData", menuName = "WallData", order = 1)]
    public class WallData : ScriptableObject
    {
        public string Name;
        public float WorkCost;
        public ProfessionData CraftersProfession;
        
        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle", "Structure" };
        
        public WallPiece Horizontal;
        public WallPiece HorizontalLeftEndLeftEdge;
        public WallPiece HorizontalLeftEndRightEdge;
        public WallPiece HorizontalRightEndLeftEdge;
        public WallPiece HorizontalRightEndRightEdge;
        public WallPiece VerticalLeftEdge;
        public WallPiece VerticalRightEdge;
        public WallPiece Solo;
        public WallPiece HorizontalT;
        
        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>();
            foreach (var resourceCost in _resourceCosts)
            {
                ItemAmount cost = new ItemAmount
                {
                    Item = resourceCost.Item,
                    Quantity = resourceCost.Quantity
                };
                clone.Add(cost);
            }

            return clone;
        }
        
        public List<string> InvalidPlacementTags
        {
            get
            {
                List<string> clone = new List<string>();
                foreach (var tag in _invalidPlacementTags)
                {
                    clone.Add(tag);
                }

                return clone;
            }
        }
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in _resourceCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }

        public WallPiece GetWall(Neighbours n)
        {
            if (n.Top && n.Right && n.Bottom && n.Left)
            {
                return HorizontalT;
            }
            
            if (!n.Top && !n.Right && !n.Bottom && !n.Left)
            {
                return Solo;
            }
            if (n.Top && !n.Right && !n.Bottom && !n.Left)
            {
                if (n.TopPiece.IsLeftAligned)
                {
                    return VerticalLeftEdge;
                }
                else
                {
                    return VerticalRightEdge;
                }
            }
            if (!n.Top && n.Right && !n.Bottom && !n.Left)
            {
                return HorizontalLeftEndRightEdge;
            }
            if (!n.Top && !n.Right && n.Bottom && !n.Left)
            {
                if (n.BottomPiece.IsLeftAligned)
                {
                    return VerticalLeftEdge;
                }
                else
                {
                    return VerticalRightEdge;
                }
            }
            if (!n.Top && !n.Right && !n.Bottom && n.Left)
            {
                return HorizontalRightEndLeftEdge;
            }

            
            if (n.Top && n.Right && !n.Bottom && !n.Left)
            {
                if (n.TopPiece.IsLeftAligned)
                {
                    return HorizontalLeftEndLeftEdge;
                }
                else
                {
                    return HorizontalLeftEndRightEdge;
                }
            }
            if (n.Top && !n.Right && n.Bottom && !n.Left)
            {
                if (n.BottomPiece.IsLeftAligned)
                {
                    return VerticalLeftEdge;
                }
                else
                {
                    return VerticalRightEdge;
                }
            }
            if (n.Top && !n.Right && !n.Bottom && n.Left)
            {
                if (n.TopPiece.IsLeftAligned)
                {
                    return HorizontalRightEndLeftEdge;
                }
                else
                {
                    return HorizontalRightEndRightEdge;
                }
            }
            if (!n.Top && n.Right && n.Bottom && !n.Left)
            {
                if (n.BottomPiece.IsLeftAligned)
                {
                    return HorizontalLeftEndLeftEdge;
                }
                else
                {
                    return HorizontalLeftEndRightEdge;
                }
            }
            if (!n.Top && n.Right && !n.Bottom && n.Left)
            {
                return Horizontal;
            }
            if (!n.Top && !n.Right && n.Bottom && n.Left)
            {
                return HorizontalRightEndLeftEdge;
            }

            
            if (n.Top && n.Right && n.Bottom && !n.Left)
            {
                return HorizontalLeftEndLeftEdge;
            }
            if (!n.Top && n.Right && n.Bottom && n.Left)
            {
                return HorizontalT;
            }
            if (n.Top && !n.Right && n.Bottom && n.Left)
            {
                if (n.BottomPiece.IsLeftAligned)
                {
                    return VerticalLeftEdge;
                }
                else
                {
                    return VerticalRightEdge;
                }
            }
            if (n.Top && n.Right && !n.Bottom && n.Left)
            {
                return Horizontal;
            }
            if (n.Top && n.Right && n.Bottom && !n.Left)
            {
                return HorizontalLeftEndLeftEdge;
            }

            Debug.LogError($"Missing possibility: Top {n.Top}, Right {n.Right}, Bottom {n.Bottom}, Left {n.Left}");
            return null;
        }

        [Serializable]
        public class WallPiece
        {
            public GameObject Prefab;
            public bool IsLeftAligned;

            public WallPiece(GameObject prefab, bool isLeftAligned)
            {
                Prefab = prefab;
                IsLeftAligned = isLeftAligned;
            }
        }

        public class Neighbours
        {
            public bool Top => TopPiece != null;
            public bool Bottom => BottomPiece != null;
            public bool Left => LeftPiece != null;
            public bool Right => RightPiece != null;

            public StructurePiece TopPiece;
            public StructurePiece BottomPiece;
            public StructurePiece LeftPiece;
            public StructurePiece RightPiece;

            public bool IsEqualTo(Neighbours other)
            {
                if (other == null) return false;
                
                if (TopPiece != other.TopPiece) return false;
                if (BottomPiece != other.BottomPiece) return false;
                if (LeftPiece != other.LeftPiece) return false;
                if (RightPiece != other.RightPiece) return false;

                return true;
            }
        }
    }
}
