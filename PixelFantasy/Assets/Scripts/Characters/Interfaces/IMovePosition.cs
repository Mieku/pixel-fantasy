using System;
using UnityEngine;

namespace Characters.Interfaces
{
    public interface IMovePosition
    {
        bool SetMovePosition(Vector3 movePosition, Action onReachedMovePosition = null);
    }
}
