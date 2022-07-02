using System;
using System.Collections.Generic;
using Actions;
using Controllers;
using Gods;
using Interfaces;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "CraftedData/StructureData", order = 1)]
    public class StructureData : ScriptableObject
    {
        public string StructureName;
        public float WorkCost;
        public Sprite Icon;

        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags;
        [SerializeField] private List<Order> _options;
        [SerializeField] private PlanningMode _planningMode;
        [SerializeField] private RuleOverrideTile _wallRuleTile;
        [SerializeField] private List<ActionBase> _availableActions;
        
        public List<ActionBase> AvailableActions => _availableActions;
        
        public PlanningMode PlanningMode => _planningMode;
        
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

        public List<Order> Options
        {
            get
            {
                List<Order> clone = new List<Order>();
                foreach (var option in _options)
                {
                    clone.Add(option);
                }

                return clone;
            }
        }

        public RuleOverrideTile RuleTile => _wallRuleTile;

        public Sprite GetSprite()
        {
            return Icon;
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
        
        public NeighbourData GetNeighbourData(Vector2 pos)
        {
            NeighbourData neighbourData = new NeighbourData();
            
            neighbourData.Neighbours.Clear();
            var connectionInfo = new WallNeighbourConnectionInfo();
            
            Vector2 topPos = new Vector2(pos.x, pos.y + 1);
            Vector2 botPos = new Vector2(pos.x, pos.y - 1);
            Vector2 leftPos = new Vector2(pos.x - 1, pos.y);
            Vector2 rightPos = new Vector2(pos.x + 1, pos.y);
            
            var allHitTop = Physics2D.RaycastAll(topPos, Vector2.down, 0.4f);
            var allHitBot = Physics2D.RaycastAll(botPos, Vector2.up, 0.4f);
            var allHitLeft = Physics2D.RaycastAll(leftPos, Vector2.right, 0.4f);
            var allHitRight = Physics2D.RaycastAll(rightPos, Vector2.left, 0.4f);

            // Top
            foreach (var hit in allHitTop)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    neighbourData.Neighbours.Add(hit.transform.gameObject);
                    connectionInfo.Top = true;
                    break;
                }
            }
            // Bottom
            foreach (var hit in allHitBot)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    neighbourData.Neighbours.Add(hit.transform.gameObject);
                    connectionInfo.Bottom = true;
                    break;
                }
            }
            // Left
            foreach (var hit in allHitLeft)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    neighbourData.Neighbours.Add(hit.transform.gameObject);
                    connectionInfo.Left = true;
                    break;
                }
            }
            // Right
            foreach (var hit in allHitRight)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    neighbourData.Neighbours.Add(hit.transform.gameObject);
                    connectionInfo.Right = true;
                    break;
                }
            }

            neighbourData.WallNeighbourConnectionInfo = connectionInfo;

            return neighbourData;
        }
    }

    [Serializable]
    public class ItemAmount
    {
        public ItemData Item;
        public int Quantity;
    }

    public class NeighbourData
    {
        public List<GameObject> Neighbours = new List<GameObject>();
        public WallNeighbourConnectionInfo WallNeighbourConnectionInfo = new WallNeighbourConnectionInfo();
    }
}
