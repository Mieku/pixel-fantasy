using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class ObjectReferenceRepository : Repository<ItemObject>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static ObjectReferenceRepository objectReferenceRepository;

        private void Awake()
        {
            if (!objectReferenceRepository)
                objectReferenceRepository = this;
            else if (objectReferenceRepository != this)
                Destroy(gameObject);
            
            Debug.Log($"---- OBJECT REFERENCE REPOSITIORY AWAKE");
            #if UNITY_EDITOR
            return;
            #endif
            //Debug.Log($"---- Should populate dictionaries");
            //objectReference.PopulateDictionaries();
        }

        [Header("Auto populated")] 
        public ObjectReference objectReference;
        
        public override void PopulateList()
        {
#if UNITY_EDITOR
            objectReference = GetObjectReference();
            if (objectReference == null)
            {
                Debug.LogWarning("Object Reference was not found! This should be created automatically and " +
                                 "would be found at Assets/Game Modules Object Reference/ObjectReference! " +
                                 "The ObjectReference should be created now, and this warning should not show again.");
            }
#endif
        }
    }
}
