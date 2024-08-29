using System.Collections;
using System.Collections.Generic;
using AI.Action_Tasks;
using Characters;
using Handlers;
using Managers;
using NodeCanvas.Framework;
using UnityEngine;

public class WithdrawItem : KinlingActionTask
{
    public BBParameter<string> ItemUID;
    public BBParameter<string> KinlingUID;
    
    protected override void OnExecute()
    {
        var kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
        var itemData = ItemsDatabase.Instance.Query(ItemUID.value);

        var assignedStorage = itemData.AssignedStorage;
        var item = assignedStorage.WithdrawItem(itemData);
        kinling.HoldItem(item, itemData.UniqueID);
        
        EndAction(true);
    }
}
