namespace DataPersistence
{
    public interface IPersistent
    {
        public object CaptureState();
        public void RestoreState(object data);
    }
}
