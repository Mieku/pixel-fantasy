using System;
using Character;
using Character.Interfaces;
using UnityEngine;
using CodeMonkey.Utils;

namespace Character
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
