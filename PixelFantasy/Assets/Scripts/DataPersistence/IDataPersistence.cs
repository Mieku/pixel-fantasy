using DataPersistence.States;

namespace DataPersistence
{
    public interface IDataPersistence
    {
        void LoadData(GameState state);
        void SaveState(ref GameState state);
    }
}
