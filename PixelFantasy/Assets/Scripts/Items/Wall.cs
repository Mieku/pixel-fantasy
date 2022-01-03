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
    public class Wall : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _blueprintColour;
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private DynamicGridObstacle _gridObstacle;

        private StructureData _structureData;
        private List<Wall> _neighbours = new List<Wall>();
        private float _workComplete;
        
        private TaskMaster taskMaster => TaskMaster.Instance;

        public void Init(StructureData structureData)
        {
            _structureData = structureData;
            UpdateSprite(true);
            _progressBar.ShowBar(false);
            ShowBlueprint(true);
            CreateConstuctTask();
        }

        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _spriteRenderer.color = _blueprintColour;
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
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Top = true;
                    break;
                }
            }
            // Bottom
            foreach (var hit in allHitBot)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Bottom = true;
                    break;
                }
            }
            // Left
            foreach (var hit in allHitLeft)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Left = true;
                    break;
                }
            }
            // Right
            foreach (var hit in allHitRight)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
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

        private void CreateConstuctTask()
        {
            // Each resource required gets a task
            var resourceCosts = _structureData.ResourceCosts;
            foreach (var resourceCost in resourceCosts)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    CreateConstructResourceIntoStructureTask(resourceCost.Item);
                }
            }
        }

        private void CreateConstructResourceIntoStructureTask(ItemData resourceData)
        {
            taskMaster.ConstructionTaskSystem.EnqueueTask(() =>
            {
                Item resource = InventoryController.Instance.ClaimResource(resourceData);
                if (resource != null)
                {
                    var task = new ConstructionTask.ConstructResourceIntoStructure
                    {
                        resourcePosition = resource.transform.position,
                        structurePosition = transform.position,
                        workAmount = _structureData.GetWorkPerResource(),
                        grabResource = (UnitTaskAI unitTaskAI) =>
                        {
                            resource.transform.SetParent(unitTaskAI.transform);
                            InventoryController.Instance.DeductClaimedResource(resource);
                        },
                        useResource = () =>
                        {
                            resource.gameObject.SetActive(false);
                        },
                        completeWork = () =>
                        {
                            AddProgress(resource);
                        }
                    };
                    return task;
                }
                else
                {
                    return null;
                }
            });
        }

        private void AddProgress(Item resource)
        {
            _workComplete += _structureData.GetWorkPerResource();
            var percentDone = _workComplete / _structureData.WorkCost;

            if (Math.Abs(_workComplete - _structureData.WorkCost) < 0.01f)
            {
                ShowBlueprint(false);
                _progressBar.ShowBar(false);
            }
            else
            {
                _progressBar.ShowBar(true);
                _progressBar.SetProgress(percentDone);
            }
            
            Destroy(resource.gameObject);
        }
    }
}
