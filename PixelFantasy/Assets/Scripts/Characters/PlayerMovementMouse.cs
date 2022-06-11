using System;
using Characters;
using Characters.Interfaces;
using UnityEngine;
using CodeMonkey.Utils;

namespace Characters
{
    public class PlayerMovementMouse : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                GetComponent<IMovePosition>().SetMovePosition(UtilsClass.GetMouseWorldPosition());
            }
        }
    }
}
