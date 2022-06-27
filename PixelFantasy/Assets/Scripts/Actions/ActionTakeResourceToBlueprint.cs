using Characters;
using Gods;
using Items;
using ScriptableObjects;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionTakeResourceToBlueprint", menuName ="Actions/TakeResourceToBlueprint", order = 50)]
    public class ActionTakeResourceToBlueprint : ActionBase
    {
        public void EnqueueTask(Interactable requestor, ItemData resourceData)
        {
            taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                StorageSlot slot = ControllerManager.Instance.InventoryController.ClaimResource(resourceData);
                if (slot != null)
                {
                    var structure = requestor.GetComponent<Structure>();
                    if (structure != null)
                    {
                        structure.AddToPendingResourceCosts(resourceData);
                    }
                    var floor = requestor.GetComponent<Floor>();
                    if (floor != null)
                    {
                        floor.AddToPendingResourceCosts(resourceData);
                    }
                    var furniture = requestor.GetComponent<Furniture>();
                    if (furniture != null)
                    {
                        furniture.AddToPendingResourceCosts(resourceData);
                    }
                    
                    return CreateTaskWithSlot(requestor, slot);
                }
                else
                {
                    return null;
                }
            });
        }

        public HaulingTask.TakeResourceToBlueprint CreateTaskWithSlot(Interactable requestor, StorageSlot slot)
        {
            Item resource;
            var task = new HaulingTask.TakeResourceToBlueprint
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                resourcePosition = slot.transform.position,
                blueprintPosition = requestor.transform.position,
                grabResource = (UnitTaskAI unitTaskAI) =>
                {
                    // Get item from the slot
                    resource = slot.GetItem();
                    
                    var structure = requestor.GetComponent<Structure>();
                    if (structure != null)
                    {
                        structure.AddToIncomingItems(resource);
                    }
                    var floor = requestor.GetComponent<Floor>();
                    if (floor != null)
                    {
                        floor.AddToIncomingItems(resource);
                    }
                    var furniture = requestor.GetComponent<Furniture>();
                    if (furniture != null)
                    {
                        furniture.AddToIncomingItems(resource);
                    }

                    resource.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(resource);
                },
                useResource = ( heldItem) =>
                {
                    var structure = requestor.GetComponent<Structure>();
                    if (structure != null)
                    {
                        structure.AddResourceToBlueprint(heldItem.GetItemData());
                        structure.RemoveFromIncomingItems(heldItem);
                        structure.CheckIfAllResourcesLoaded();
                    }
                    var floor = requestor.GetComponent<Floor>();
                    if (floor != null)
                    {
                        floor.AddResourceToBlueprint(heldItem.GetItemData());
                        floor.RemoveFromIncomingItems(heldItem);
                        floor.CheckIfAllResourcesLoaded();
                    }
                    var furniture = requestor.GetComponent<Furniture>();
                    if (furniture != null)
                    {
                        furniture.AddResourceToBlueprint(heldItem.GetItemData());
                        furniture.RemoveFromIncomingItems(heldItem);
                        furniture.CheckIfAllResourcesLoaded();
                    }
                    
                    heldItem.gameObject.SetActive(false);
                    Destroy(heldItem.gameObject);
                    OnTaskComplete(requestor);
                },
            };

            return task;
        }
        
        public HaulingTask.TakeResourceToBlueprint CreateTaskWithItem(Interactable requestor, Item item)
        {
            var task = new HaulingTask.TakeResourceToBlueprint
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                resourcePosition = item.transform.position,
                blueprintPosition = requestor.transform.position,
                grabResource = (UnitTaskAI unitTaskAI) =>
                {
                    item.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(item);
                },
                useResource = ( heldItem) =>
                {
                    var recievingStructure = requestor.GetComponent<Structure>();
                    if (recievingStructure != null)
                    {
                        recievingStructure.AddResourceToBlueprint(heldItem.GetItemData());
                        recievingStructure.RemoveFromIncomingItems(heldItem);
                        recievingStructure.CheckIfAllResourcesLoaded();
                    }
                    var recievingFloor = requestor.GetComponent<Floor>();
                    if (recievingFloor != null)
                    {
                        recievingFloor.AddResourceToBlueprint(heldItem.GetItemData());
                        recievingFloor.RemoveFromIncomingItems(heldItem);
                        recievingFloor.CheckIfAllResourcesLoaded();
                    }
                    var recievingFurniture = requestor.GetComponent<Furniture>();
                    if (recievingFurniture != null)
                    {
                        recievingFurniture.AddResourceToBlueprint(heldItem.GetItemData());
                        recievingFurniture.RemoveFromIncomingItems(heldItem);
                        recievingFurniture.CheckIfAllResourcesLoaded();
                    }
                    
                    heldItem.gameObject.SetActive(false);
                    Destroy(heldItem.gameObject);
                    OnTaskComplete(requestor);
                },
            };

            return task;
        }

        public TaskBase RestoreTask(Interactable requestor, StorageSlot slot, Item heldItem)
        {
            if (slot != null)
            {
                return CreateTaskWithSlot(requestor, slot);
            } else if (heldItem != null)
            {
                return CreateTaskWithItem(requestor, heldItem);
            }
            else
            {
                return null;
            }
        }

        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
        }
    }
}
