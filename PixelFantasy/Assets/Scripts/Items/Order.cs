namespace Items
{
    public enum Order
    {
        Deconstruct,  // Is also cancel when not built yet
        CutTree,
        CutPlant,
        Harvest,
        Cancel,
        Disallow,
    }
}
