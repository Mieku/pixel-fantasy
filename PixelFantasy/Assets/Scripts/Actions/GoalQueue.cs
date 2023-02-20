using System;
using System.Collections;
using System.Collections.Generic;
using SGoap;
using UnityEngine;

[Serializable]
public class GoalQueue
{
    public List<GoalRequest> Requests = new List<GoalRequest>();

    public GoalRequest NextRequest
    {
        get
        {
            if (Requests.Count > 0)
            {
                var result = Requests[0];
                Requests.RemoveAt(0);
                return result;
            }

            return null;
        }
    }

    public void AddRequest(GoalRequest request)
    {
        if (request != null)
        {
            Requests.Add(request);
        }
    }

    public void CancelRequest(GoalRequest requestToCancel)
    {
        GoalRequest target = null;
        foreach (var request in Requests)
        {
            if (request.IsEqual(requestToCancel))
            {
                target = request;
                break;
            }
        }

        if (target != null)
        {
            Requests.Remove(target);
        }
    }
}
