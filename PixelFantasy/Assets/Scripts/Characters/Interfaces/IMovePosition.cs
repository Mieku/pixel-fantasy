using System;
using UnityEngine;

namespace Characters.Interfaces
{
    public interface IMovePosition
    {
        bool SetMovePosition(Vector2 movePosition, Action onReachedMovePosition = null);
    }
}
