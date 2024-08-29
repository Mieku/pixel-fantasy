using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<PlayerInteractable> RegisteredPlayerInteractables => _registeredPIs.Values.ToList();

    public List<PlayerInteractable> GetAllSimilarVisiblePIs(PlayerInteractable piToMatch)
    {
        List<PlayerInteractable> visibleInteractables = new List<PlayerInteractable>();
        Camera mainCamera = Camera.main;

        foreach (var kvp in _registeredPIs)
        {
            var pi = kvp.Value;

            // Check if the pis are similar
            if (piToMatch.IsSimilar(pi))
            {
                Vector3 viewportPoint = mainCamera.WorldToViewportPoint(pi.transform.position);

                // Check if the object is within the camera's view
                if (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                    viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                    viewportPoint.z > 0) // Check that the object is in front of the camera
                {
                    visibleInteractables.Add(pi);
                }
            }
        }

        return visibleInteractables;
    }
    
    public void ClearDatabase()
    {
        _registeredPIs.Clear();
    }
}
