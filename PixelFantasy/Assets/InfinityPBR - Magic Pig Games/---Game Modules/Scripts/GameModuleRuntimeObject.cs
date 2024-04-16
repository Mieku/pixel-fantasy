using System;
using InfinityPBR.Modules;
using UnityEngine;

namespace InfinityPBR
{
    public abstract class GameModuleRuntimeObject<T> : IAmGameModuleObject, IHaveGameId, IHaveUid, IHaveDictionaries  where T : IAmGameModuleObject
    {
        // NOTE: These should be populated with the _parent data in the Constructor!
        public string ObjectName() => objectName;
        public string ObjectType() => objectType;
        public string objectName; // The name of this
        public string objectType; // The type (parent directory from hierarchy name)
        public Dictionaries dictionaries = new Dictionaries("Unnamed");
        
        // Private or Protected
        protected bool _isDirty = false; // Used to signify that this object is dirty (unique results per type)
        //protected ModulesScriptableObject _scriptableObject; // Holds the original scriptable object
        
        // Properties
        public bool IsDirty => _isDirty;
        
        // **********************************************************************
        // GameId
        // **********************************************************************
        
        [SerializeField] private string _gameId;

        // Will create a new _gameId if one does not exist
        // NOTE: call forceNew true in Constructor. Also, be careful when cloning, as
        // in some cases you may wish to make a new GameId, but in other cases, cloning
        // will not want to make a new GameId.
        public virtual string GameId(bool forceNew = false) =>
            String.IsNullOrWhiteSpace(_gameId) || forceNew
                ? _gameId = Guid.NewGuid().ToString() 
                : _gameId;

        // **********************************************************************
        // Uid
        // **********************************************************************
        
        [SerializeField] private string _uid;

        //public virtual string Uid() => Parent().Uid();
        
        // **********************************************************************
        // Virtual Methods
        // **********************************************************************
        
        //public virtual void Tick() => TickActions();
        //public virtual void LateTick() => TickActions();

        public virtual T Clone(bool forceNewGameId = false) => JsonUtility.FromJson<T>(JsonUtility.ToJson(this));

        // **********************************************************************
        // Abstract Classes
        // **********************************************************************

        // Used to get the repository for the object type
        //public abstract IAmRepository Repository();

        // Used to get the original Game Modules Scriptable Object this Game version is made from
        //public abstract ModulesScriptableObject ScriptableObject();
        
        // The "Update" analogy unique to each object type
        //public abstract void TickActions(); 
        
        // Used to set children "dirty" so that they recompute their value (such as in Stats).
        // Not all will utilize this.
        public abstract void SetDirty(bool dirtyValue = true);
        
        // **********************************************************************
        // IHaveDictionaries
        // **********************************************************************
        
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);

        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
        public void CheckForMissingObjectReferences()
        {
            // Not used in this context
        }

        public string Uid()
        {
            throw new NotImplementedException();
        }
    }
}
