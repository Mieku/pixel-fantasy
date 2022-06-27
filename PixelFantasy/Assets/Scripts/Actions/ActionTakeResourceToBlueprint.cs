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
                    var construction = requestor.GetComponent<Construction>();
                    if (construction != null)
                    {
                        construction.AddToPendingResourceCosts(resourceData);
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
                    
                    var construction = requestor.GetComponent<Construction>();
                    if (construction != null)
                    {
                        construction.AddToIncomingItems(resource);
                    }

                    resource.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(resource);
                },
                useResource = ( heldItem) =>
                {
                    var construction = requestor.GetComponent<Construction>();
                    if (construction != null)
                    {
                        construction.AddResourceToBlueprint(heldItem.GetItemData());
                        construction.RemoveFromIncomingItems(heldItem);
                        construction.CheckIfAllResourcesLoaded();
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
                    var recievingConstruction = requestor.GetComponent<Construction>();
                    if (recievingConstruction != null)
                    {
                        recievingConstruction.AddResourceToBlueprint(heldItem.GetItemData());
                        recievingConstruction.RemoveFromIncomingItems(heldItem);
                        recievingConstruction.CheckIfAllResourcesLoaded();
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
