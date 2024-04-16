using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.SceneManagement;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class GameList
    {
        public List<GameFile> gameFiles = new List<GameFile>();

        public virtual void AddOrUpdate(string saveGameId, string gameName, string sceneName = "")
        {
            var foundFile = gameFiles.FirstOrDefault(x => x.saveGameId == saveGameId);
            if (foundFile == null)
            {
                gameFiles.Add(new GameFile());
                var newFile = gameFiles[^1];
                newFile.saveGameId = saveGameId;
                newFile.gametimeNow = Timeboard.timeboard.gametime.Now();
                newFile.systemTime = DateTimeString();
                newFile.systemTimecode = DateTime.Now.Ticks;
                newFile.gameName = gameName;
                newFile.sceneName = string.IsNullOrWhiteSpace(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            }
            else
            {
                foundFile.gametimeNow = Timeboard.timeboard.gametime.Now();
                foundFile.systemTime = DateTimeString();
                foundFile.systemTimecode = DateTime.Now.Ticks;
                foundFile.gameName = gameName;
                foundFile.sceneName = string.IsNullOrWhiteSpace(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            }
        }

        private string DateTimeString() => DateTime.Now.ToString("F", CultureInfo.CurrentCulture);
    }
}