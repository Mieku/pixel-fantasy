using System.Collections;
using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerInteractableDatabase : Singleton<PlayerInteractableDatabase>
{
    [ShowInInspector] private Dictionary<string, PlayerInteractable> _registeredPIs = new Dictionary<string, PlayerInteractable>();

    public void RegisterPlayerInteractable(PlayerInteractable pi)
    {
        _registeredPIs[pi.UniqueID] = pi;
    }

    public void DeregisterPlayerInteractable(PlayerInteractable pi)
    {
        _registeredPIs.Remove(pi.UniqueID);
    }

    public PlayerInteractable Query(string uniqueID)
    {
        return _registeredPIs[uniqueID];
    }

    public void ClearDatabase()
    {
        _registeredPIs.Clear();
    }
}
