using System;
using UnityEngine;

namespace Characters.Interfaces
{
    public interface IMovePosition
    {
        void SetMovePosition(Vector3 movePosition, Action onReachedMovePosition = null);
    }
}
