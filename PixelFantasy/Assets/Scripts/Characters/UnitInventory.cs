using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Items;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace Characters
{
    public class UnitInventory : MonoBehaviour
    {
        [SerializeField] private StorageSlot _held;
        [SerializeField] private StorageSlot _pocket;
    }
}
