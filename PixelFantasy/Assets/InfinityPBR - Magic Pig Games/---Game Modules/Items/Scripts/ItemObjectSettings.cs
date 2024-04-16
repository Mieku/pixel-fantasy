using System;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ItemObjectSettings
    {
        public ItemObject itemObject;
        public ItemAttribute[] itemAttributes;

        public GameItemObject CreateItemObject(IHaveStats owner)
        {
            if (itemObject == null)
                throw new NullReferenceException("The ItemObjectSettings is missing a itemObject! This is required.");
            
            var newItemObject = new GameItemObject(itemObject, owner);
            foreach(var itemAttribute in itemAttributes)
            {
                // Add each, but do not reset the modification level until the end
                newItemObject.AddAttribute(itemAttribute.Uid(), true, true, false, false);
            }

            newItemObject.SetAffectedStatsDirty();
            newItemObject.ResetItemModificationLevel(); // Reset the modification level
            return newItemObject;
        }
    }
}
