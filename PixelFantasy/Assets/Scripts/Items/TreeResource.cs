using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class TreeResource : GrowingResource
    {
        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Axe;
        }
    }
}
