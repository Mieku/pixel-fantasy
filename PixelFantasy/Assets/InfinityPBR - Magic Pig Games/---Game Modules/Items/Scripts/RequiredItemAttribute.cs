using System;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class RequiredItemAttribute
    {
        public ItemAttribute itemAttribute;
        public bool onePerType = true;
    }
}
