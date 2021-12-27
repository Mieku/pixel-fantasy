using System;
using UnityEngine;

namespace Character.Interfaces
{
    public interface IMovePosition
    {
        void SetMovePosition(Vector3 movePosition, Action onReachedMovePosition = null);
    }
}
