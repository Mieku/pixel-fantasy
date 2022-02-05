namespace Interfaces
{
    public interface IClickableObject
    {
        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }

        public void ToggleAllowed(bool isAllowed);
    }
}
