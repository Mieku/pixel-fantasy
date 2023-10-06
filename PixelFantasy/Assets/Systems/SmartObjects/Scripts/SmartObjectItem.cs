using Items;

namespace Systems.SmartObjects.Scripts
{
    public class SmartObjectItem : SmartObject
    {
        private Item _item;

        public void Init(Item item, bool populateInteractions = true)
        {
            _item = item;
            _displayName = item.GetItemData().ItemName;

            if (populateInteractions)
            {
                PopulateInteractions();
            }
        }
        
        private void PopulateInteractions()
        {
            var interConfigs = _item.GetItemData().InteractionConfigs;
            foreach (var interConfig in interConfigs)
            {
                ItemInteraction itemInteraction = gameObject.AddComponent<ItemInteraction>();
                itemInteraction.Init(_item, interConfig);
            }
            RefreshInteractions();
        }
    }
}
