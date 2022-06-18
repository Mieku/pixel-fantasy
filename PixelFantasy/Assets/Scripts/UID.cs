using System;
using System.Collections;
using System.Collections.Generic;
using Gods;
using Sirenix.OdinInspector;
using UnityEngine;

public class UID : MonoBehaviour
{
    public string uniqueID;

    [Button("Assign UID")]
    public void AssignGUID(string uidOverride = "")
    {
        if (uidOverride != "")
        {
            UIDManager.Instance.RemoveUID(this);
            uniqueID = uidOverride;
            UIDManager.Instance.AddUID(this);
        }
        else if (uniqueID == "")
        {
            uniqueID = $"{gameObject.name}_{Guid.NewGuid()}";
        }
    }

    private void Start()
    {
        AssignGUID();
        UIDManager.Instance.AddUID(this);
    }

    private void OnDestroy()
    {
        UIDManager.Instance.RemoveUID(this);
    }
}
