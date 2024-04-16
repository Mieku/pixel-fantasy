namespace InfinityPBR.Modules
{
    public interface ItemAttributeDrawer
    {
        void Draw(ItemAttributeDrawer itemAttributeDrawer, ItemObject itemObject, bool structure = false);
        void DrawStructure(ItemAttributeDrawer itemAttributeDrawer, string itemObjectType);
        void ResetCache(bool force = false);
    }
}
