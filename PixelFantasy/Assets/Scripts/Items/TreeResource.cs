using Systems.Appearance.Scripts;

namespace Items
{
    public class TreeResource : GrowingResource
    {
        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Swinging;
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is TreeResource)
            {
                return true;
            }

            return false;
        }
    }
}
