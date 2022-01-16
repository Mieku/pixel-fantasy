using System;
using System.Collections.Generic;
using Controllers;
using Gods;
using Pathfinding;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Structure : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private DynamicGridObstacle _gridObstacle;

        private StructureData _structureData;
        private readonly List<Structure> _neighbours = new List<Structure>();
        private List<ResourceCost> _resourceCost;
        
        private TaskMaster taskMaster => TaskMaster.Instance;

        public void Init(StructureData structureData)
        {
            _structureData = structureData;
            _resourceCost = new List<ResourceCost> (_structureData.GetResourceCosts());
            UpdateSprite(true);
            _progressBar.ShowBar(false);
            ShowBlueprint(true);
            CreateConstructionHaulingTasks();
        }

        public StructureData GetStructureData()
        {
            return _structureData;
        }

        private void AddResourceToBlueprint(ItemData itemData)
        {
            foreach (var cost in _resourceCost)
            {
                if (cost.Item == itemData && cost.Quantity > 0)
                {
                    cost.Quantity--;
                    if (cost.Quantity <= 0)
                    {
                        _resourceCost.Remove(cost);
                    }

                    return;
                }
            }
        }

        private void CheckIfAllResourcesLoaded()
        {
            if (_resourceCost.Count == 0)
            {
                CreateConstructStructureTask();
            }
        }

        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _spriteRenderer.color = Librarian.Instance.GetColour("Blueprint");
                _gridObstacle.enabled = false;
            }
            else
            {
                _spriteRenderer.color = Color.white;
                _gridObstacle.enabled = true;
                gameObject.layer = 4;
            }
        }

        private WallNeighbourConnectionInfo RefreshNeighbourData()
        {
            _neighbours.Clear();
            var result = new WallNeighbourConnectionInfo();

            var pos = transform.position;
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
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Structure>());
                    result.Top = true;
                    break;
                }
            }
            // Bottom
            foreach (var hit in allHitBot)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Structure>());
                    result.Bottom = true;
                    break;
                }
            }
            // Left
            foreach (var hit in allHitLeft)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Structure>());
                    result.Left = true;
                    break;
                }
            }
            // Right
            foreach (var hit in allHitRight)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Structure>());
                    result.Right = true;
                    break;
                }
            }

            return result;
        }
        
        public void UpdateSprite(bool informNeighbours)
        {
            // collect data on connections
            var connectData = RefreshNeighbourData();

            // use connection data to update sprite
            _spriteRenderer.sprite = _structureData.GetSprite(connectData);

            // If inform neighbours, tell neighbours to UpdateSprite (but they shouldn't inform their neighbours
            if (informNeighbours)
            {
                RefreshNeighbours();
            }
        }

        private void RefreshNeighbours()
        {
            foreach (var neighbour in _neighbours)
            {
                neighbour.UpdateSprite(false);
            }
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _structureData.GetResourceCosts();
            foreach (var resourceCost in resourceCosts)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    CreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        private void CreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                Item resource = InventoryController.Instance.ClaimResource(resourceData);
                if (resource != null)
                {
                    var task = new HaulingTask.TakeResourceToBlueprint
                    {
                        resourcePosition = resource.transform.position,
                        blueprintPosition = transform.position,
                        grabResource = (UnitTaskAI unitTaskAI) =>
                        {
                            resource.transform.SetParent(unitTaskAI.transform);
                            InventoryController.Instance.DeductClaimedResource(resource);
                        },
                        useResource = () =>
                        {
                            resource.gameObject.SetActive(false);
                            AddResourceToBlueprint(resource.GetItemData());
                            Destroy(resource.gameObject);
                            CheckIfAllResourcesLoaded();
                        },
                    };

                    return task;
                }
                else
                {
                    return null;
                }
            });
        }
        
        private void CreateConstructStructureTask()
        {
            var task = new ConstructionTask.ConstructStructure
            {
                structurePosition = transform.position,
                workAmount = _structureData.GetWorkPerResource(),
                completeWork = CompleteConstruction
            };
            
            taskMaster.ConstructionTaskSystem.AddTask(task);
        }
        
        private void CompleteConstruction()
        {
            ShowBlueprint(false);
        }
    }
}
