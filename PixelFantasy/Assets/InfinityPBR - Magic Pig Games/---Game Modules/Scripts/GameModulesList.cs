using System;
using System.Collections.Generic;

namespace InfinityPBR.Modules
{
    [Serializable]
    public abstract class GameModulesList<T> where T : IAmGameModuleObject
    {
        
        public List<T> list = new List<T>();

        public virtual T Add(string uid, bool distinct = true)
        {
            return default;
        }

        public virtual void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject)
        {
            
        }

        public virtual string GameId()
        {
            return default;
        }
        
        public bool postListToBlackboard;
        public bool notifyOnPost;
        
        public virtual bool PostListToBlackboard() => postListToBlackboard;
        public virtual bool NotifyOnPost() => notifyOnPost;
        public virtual string NoteSubject() => "";
        
        public virtual float Now => Timeboard.timeboard.gametime.Now();
    }
}
