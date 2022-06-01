using DataPersistence.States;

namespace DataPersistence
{
    public class FileDataHandler
    {

        public GameState Load()
        {
            var state = ES3.Load("GameState", new GameState());
            return state;
        }

        public void Save(GameState state)
        {
            ES3.Save("GameState", state);
        }
    }
}
