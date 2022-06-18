using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UID))]
public class UniqueObject : MonoBehaviour
{
    protected UID _uid;
    public string UniqueId 
    {
        get
        {
            if (_uid == null)
            {
                _uid = GetComponent<UID>();
            }

            return _uid.uniqueID;
        }
        set
        {
            if (_uid == null)
            {
                _uid = GetComponent<UID>();
            }
                
            _uid.AssignGUID(value);
        }
    }

    protected void InitUID()
    {
        if (_uid == null)
        {
            _uid = GetComponent<UID>();
        }
                
        _uid.AssignGUID();
    }
}
