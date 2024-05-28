using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Items
{
    public interface IAssignedFurniture
    {
        public KinlingData GetPrimaryOwner();
        public KinlingData GetSecondaryOwner();
        public void ReplacePrimaryOwner(KinlingData newOwner);
        public void ReplaceSecondaryOwner(KinlingData newOwner);
        public bool CanHaveSecondaryOwner();
    }
}
