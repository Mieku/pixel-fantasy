using System;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR.Modules;
using UnityEngine;

/*
 * GAME ITEM OBJECT
 *
 * This is the in-game, runtime object to use for Item Objects. It will automatically cache the scriptable object from
 * the Items repository.
 */

namespace InfinityPBR
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items")]
    [Serializable]
    public class GameItemObject : IAmGameModuleObject, IFitInInventory
    {
        public GameItemObjectList ParentList { get; private set; }
        public void SetParentList(GameItemObjectList value) => ParentList = value;
        public IHaveStats Owner => ParentList == null ? _owner : ParentList.Owner;
        public void SetOwner(IHaveStats value) => _owner = value;
        private IHaveStats _owner;

        public string ObjectName() => objectName;
        public string ObjectType() => objectType;
        public string objectName; // The name of this
        public string objectType; // The type (parent directory from hierarchy name)
        public bool QuestItem => Parent().questItem;
        public Dictionaries dictionaries = new Dictionaries("Unnamed");
        
        public List<ItemObjectVariable> variables = new List<ItemObjectVariable>();

        private Dictionary<string, ItemObjectVariable> _cachedVariables = new Dictionary<string, ItemObjectVariable>();
        public ItemObjectVariable Variable(string variableName) => GetVariable(variableName);

        private ItemObjectVariable GetVariable(string variableName)
        {
            if (_cachedVariables.TryGetValue(variableName, out ItemObjectVariable variable))
                return variable;
            
            var newVariable = variables.FirstOrDefault(v => v.name == variableName);

            if (newVariable != null)
            {
                _cachedVariables.Add(variableName, newVariable);
                return newVariable;
            }
            
            Debug.LogError($"Variable {variableName} not found in {ObjectName()}");
            return default;
        }

         

        // ************************************************************************************************
        // Connection to the parent object
        // ************************************************************************************************
        
        private ItemObject _parent;
        public ItemObject Parent() 
            => _parent 
                ? _parent 
                : _parent = GameModuleRepository.Instance.Get<ItemObject>(Uid());
        
        [SerializeField] private string _uid;
        public void SetUid(string value) => _uid = value;
        public string Uid() => _uid;

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
        
        // ************************************************************************************************
        // ItemAttribute Enums
        // ************************************************************************************************

        /// <summary>
        /// Returns true if the provided attributeUid exists on this Item Object. If itemAttributeType is also
        /// provided, then it must also have that type. The itemAttributeType does not have to be the same type as
        /// the provided itemAttributeName
        /// </summary>
        /// <param name="itemAttributeName"></param>
        /// <param name="itemAttributeType"></param>
        /// <returns></returns>
        public bool Is(string itemAttributeName, string itemAttributeType = "")
        {
            if (string.IsNullOrWhiteSpace(itemAttributeType))
                attributeList.ContainsName(itemAttributeName);
            
            return !string.IsNullOrWhiteSpace(itemAttributeType) && Has(itemAttributeType) &&
                   attributeList.ContainsName(itemAttributeName);
        }
        
        /// <summary>
        /// Returns true if the attributes list does not contain the itemAttributeName provided
        /// </summary>
        /// <param name="itemAttributeName"></param>
        /// <returns></returns>
        public bool IsNot(string itemAttributeName) => !attributeList.ContainsName(itemAttributeName);
        
        /// <summary>
        /// Returns true if the provided attributeUid exists on this Item Object
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool HasAttribute(string uid) => attributeList.Contains(uid);
        
        /// <summary>
        /// Returns true if the attributeList contains at least one attribute of this type
        /// </summary>
        /// <param name="itemAttributeType"></param>
        /// <returns></returns>
        public bool Has(string itemAttributeType) => attributeList.ContainsObjectType(itemAttributeType);

        /// <summary>
        /// If the attributeList contains at least one attribute of this type, returns the objectName of the
        /// first one found. Otherwise, returns "None".
        /// </summary>
        /// <param name="itemAttributeType"></param>
        /// <param name="noValueResult"></param>
        /// <returns></returns>
        public string ValueNameOf(string itemAttributeType, string noValueResult = "None") 
            => !attributeList.TryGetByObjectType(itemAttributeType, out var result) 
                ? noValueResult : result.objectName;

        // ************************************************************************************************
        // Members unique for this type
        // ************************************************************************************************
        
        public string FullName(bool recompute = false) 
            => GetHumanName(recompute); // The full name of this, including all attributes
        [SerializeField] [HideInInspector] private string _fullName;
        
        public GameItemAttributeList attributeList = new GameItemAttributeList();
        public List<GameItemAttribute> Attributes => attributeList.list;

        /*
         * ModificationLevels gets the modification level of the GameItemObject PLUS the attached GameItemAttributes which
         * affect the Actor. 
         */
        public List<ModificationLevel> ModificationLevels => GetModificationLevels(); 
        
        /*
         * ModificationLevel gets the ModifcationLevel of JUST THIS ITEM OBJECT, and will cache _modificationLevel if null,
         * at which point it handles all the GameItemAttributes which do not affect the Actor (and so, affect the gameItemObject).
         */
        public ModificationLevel ModificationLevel => GetModificationLevel(); // returns the current Modification Level from the ItemObject
        private ModificationLevel _modificationLevel; // Runtime version, created / cached with ItemAttribute Affects

        public void ResetItemModificationLevel(bool compute = true)
        {
            _modificationLevel = null;
            if (!compute) return;

            GetModificationLevel(); // This recomputes the null _modificationLevel
        }
        
        [SerializeField] [HideInInspector] private IHaveStats owner;
        
        // INVENTORY MODULE / IFitInInventory
        public int GetSpotInRow() => inventorySpotY;
        public int GetSpotInColumn() => inventorySpotX;
        public int GetInventoryWidth() => inventoryWidth;
        public int GetInventoryHeight() => inventoryHeight;
        public GameObject GetWorldPrefab() => Parent().prefabWorld;
        public GameObject GetInventoryPrefab() => Parent().prefabInventory;
        public int inventorySpotY;
        public int inventorySpotX;
        public int inventoryHeight;
        public int inventoryWidth;
        public GameObject prefabWorld;
        public GameObject prefabInventory;

        // ------------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // ------------------------------------------------------------------------------------------

        public void SetDirty(bool dirtyValue = true)
        {
            if (!dirtyValue) return;
            SetAffectedStatsDirty();
        }

        public void SetAffectedStatsDirty(List<Stat> statList = null) 
            => Owner?.SetStatsDirty(statList ?? DirectlyAffectsList());

        public void StartActions()
        {
            CheckDictionaries();
            attributeList.SetParentItemObject(this);

            foreach (var gameItemAttribute in Attributes)
                gameItemAttribute.StartActions();
        }
        
        // This is to ensure that if anything changes in the setup on Dictionaries for
        // ItemObject objects of this type, it is reflected here in the GameItemObject
        private void CheckDictionaries() 
            => Parent().dictionaries.keyValues
                .ForEach(keyValue => dictionaries.AddNewKeyValue(keyValue));

        /// <summary>
        /// This will add an attribute to the Item Object. Note, it will take the attribute from the Scriptable Object, creating a vanilla version.
        /// </summary>
        /// <param name="attributeUid"></param>
        /// <param name="distinct"></param>
        /// <param name="recomputeHumanName"></param>
        public GameItemAttribute AddAttribute(string attributeUid, bool distinct = true, bool recomputeHumanName = true, bool resetModificationLevel = true, bool setAffectedStatsDirty = true)
        {
            //Debug.Log($"Adding { itemAttributeRepository.GetByUid(uid).objectName} -- has it? { HasAttribute(uid)}");
            if (distinct && HasAttribute(attributeUid)) return attributeList.Get(attributeUid);
          
            ItemAttribute attribute;

#if UNITY_EDITOR
            attribute = GameModuleUtilities.GameModuleObject<ItemAttribute>(attributeUid);
#else
            // attribute = ItemAttributeRepository.itemAttributeRepository.GetByUid(attributeUid);
            attribute = GameModuleRepository.Instance.Get<ItemAttribute>(attributeUid); // Fixed version provided by Mineant on the Discord!
#endif

            // If the parent can't use this attribute, then abort
            if (!Parent().CanUseAttributeType(attribute)) return default;
            
            attributeList.Add(attributeUid, distinct);
            if (setAffectedStatsDirty)
                SetAffectedStatsDirty();

            var newAttribute = attributeList.Last();
            
            ResetItemModificationLevel(resetModificationLevel);
            
            // If we aren't recomputing the name, or if the attribute doesn't change the name, we are done
            if (!recomputeHumanName || string.IsNullOrWhiteSpace(attribute.humanName)) return newAttribute;
            
            RecomputeHumanName();
            return newAttribute;
        }

        /// <summary>
        /// Removes an Item Attribute from the list
        /// </summary>
        /// <param name="attributeUid"></param>
        /// <param name="recomputeHumanName"></param>
        public void RemoveAttribute(string attributeUid, bool recomputeHumanName = true, bool resetModificationLevel = true)
        {
            SetAffectedStatsDirty(attributeList.Get(attributeUid).DirectlyAffectsList());
            attributeList.Remove(attributeUid);
            if (!recomputeHumanName) return;
            
            ResetItemModificationLevel(resetModificationLevel);
            RecomputeHumanName();
        }

        /// <summary>
        /// Returns true if the Item Object associated with this Game Item Object can use the provided Item Attribute
        /// </summary>
        /// <param name="attributeUid"></param>
        /// <returns></returns>
        public bool CanUseAttribute(string attributeUid) => Parent().CanUseAttribute(attributeUid);

        /// <summary>
        /// Returns the owner of this Game Item Object
        /// </summary>
        /// <returns></returns>
        public IHaveStats GetOwner() => owner;

        // ------------------------------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------------------------------

        public GameItemObject(ItemObject parent, IHaveStats newOwner = null)
        {
            SetOwner(newOwner);
            _parent = parent; // v3.6
            GameId(true);
            
            if (!parent) return; // Return if we do not have an ItemObject attached yet

            objectName = _parent.objectName;
            objectType = _parent.objectType;
            dictionaries = _parent.dictionaries?.Clone();

            SetUid(parent.Uid()); // v3.6 -- set the uid with this method now
            
            // Set the Inventory module values
            inventoryHeight = parent.inventoryHeight;
            inventoryWidth = parent.inventoryWidth;
            prefabInventory = parent.prefabInventory;
            prefabWorld = parent.prefabWorld;
            
            // Variables
            foreach (var variable in Parent().variables)
            {
                variables.Add(variable.Clone);
                variables[^1].parent = this; // Set this GameItemObject as the parent
                variables[^1].SetAttribute(null, false);
            }

            // Starting Attributes -- Do not reset modification levels until after all are added
            //Debug.Log($"Should add {_parent.startingItemAttributes.Count} starting attributes");
            foreach (var startingAttribute in _parent.startingItemAttributes)
            {
                //Debug.Log($"Starting with: {startingAttribute.objectName}");
                AddAttribute(startingAttribute.Uid(), true, true, false);
            }
            //Debug.Log($"The object now has {attributeList.Count()} attributes");
            //foreach (var att in attributeList.list)
            //{
             //   Debug.Log($"Attribute: {att.objectName}");
            //}

            ResetItemModificationLevel(); // Now we can reset the Modification levels (cache the value)
        }


        // ------------------------------------------------------------------------------------------
        // PRIVATE METHODS
        // ------------------------------------------------------------------------------------------

        private string GetHumanName(bool recompute = false) =>
            recompute || string.IsNullOrWhiteSpace(_fullName)
                ? RecomputeHumanName() : _fullName;

        private string RecomputeHumanName()
        {
            // If there are no attributes, then the full name is just the object name of the ItemObject
            if (attributeList.Count() == 0)
                return _fullName = objectName.Trim();
    
            // Order the attributes by the name order selected in the Inspector
            var attributesOrdered = attributeList.list.OrderBy(x => x.NameOrder);

            // Start with the prefix attributes, which come before the ItemObject name
            // Note the space comes AFTER the HumanName.
            var prefixNames = attributesOrdered
                .Where(attribute => attribute.NameOrder < 0)
                .Aggregate("", (current, attribute) => $"{current}{attribute.HumanName} ")
                .TrimEnd(); // remove trailing spaces

            // Finish with the suffix attributes, which come after the ItemObject name
            // Note the space comes BEFORE the HumanName
            var suffixNames = attributesOrdered
                .Where(attribute => attribute.NameOrder > 0)
                .Aggregate("", (current, attribute) => $" {current}{attribute.HumanName}")
                .TrimStart(); // remove leading spaces

            // Assign to _fullName, and also remove any multiple spaces within the string
            _fullName = $"{prefixNames} {objectName} {suffixNames}".Trim().Replace("  ", " ");

            return _fullName; // return the value
        }



        /// <summary>
        /// Will return the current Modification Level of this ItemObject. Note there is only one modification level for
        /// Item Objects.
        /// </summary>
        /// <returns></returns>
        public ModificationLevel GetModificationLevel()
        {
            if (_modificationLevel != null)
                return _modificationLevel;
            
            var level = Parent().modificationLevel.Clone();

            var profDictionary = new Dictionary<StatModification, float>();

            // Go through all of the attributes attached to this object which affect the object (i.e. !affectsActor)
            foreach (var statModification in attributeList.list
                         .Where(gameItemAttribute => !gameItemAttribute.Parent().affectsActor)
                         .SelectMany(gameItemAttribute => gameItemAttribute.ModificationLevel.modifications))
            {
                StatModification foundMod;
                
                // if level has the statModification (i.e. Parent() had it), then return that, otherwise, add the 
                // statModification to level. We will add the effects to this object, either appending or setting as 
                // new values.
                if (level.TryGetStatModification(statModification.targetUid, out var thisMod))
                {
                    //Debug.Log($"Had {thisMod.target.objectName} V/P = {thisMod.value}/{thisMod.proficiency} already");
                    foundMod = thisMod;
                }
                else
                {
                    level.modifications.Add(statModification.Clone());
                    foundMod = level.modifications[^1];
                    //Debug.Log($"Added {foundMod.target.objectName} to the object");
                }
                
                // Compute the effect on the target stat from the perspective of the statModification, providing the Owner
                var foundModEffect = statModification.GetEffectOn(0f, Owner);

                // Set the effect
                foundMod.value += foundModEffect.Item1;

                // If we don't already have the key in the dictionary, add it with a starting value of 0 (proficiency = 0)
                if (!profDictionary.ContainsKey(foundMod))
                    profDictionary[foundMod] = 0;
                    
                // Add the proficiency mod to the dictionary, for later computation
                profDictionary[foundMod] += foundModEffect.Item2;
            }

            // Compute the final value, by modification of the total combined proficiency value from itemAttributes
            foreach (var item in profDictionary)
            {
                //Debug.Log($"Doing dictionary key {item.Key.target.objectName} current value of key.value is {item.Key.value}, prof =  {item.Value}");
                item.Key.value *= 1 + item.Value;
                //Debug.Log($"New values of key.value is {item.Key.value}");
            }

            _modificationLevel = level;
            return _modificationLevel;
        }
        
        /// <summary>
        /// Will return the current Modification Level of this ItemObject and all of it's ItemAttributes
        /// </summary>
        /// <returns></returns>
        public List<ModificationLevel> GetModificationLevels()
        {
            // Get the ItemObject ModificationLevel and modify it by the ItemAttributes that do not affect the Actor
            var levels = new List<ModificationLevel> { ModificationLevel }; // Start w/ the ItemObject

            // Add only attributes that affect the actor
            levels.AddRange(attributeList.ModificationLevels); // Add all the itemAttributes
            return levels;
        }
        
        public (float, float) GetEffectOn(string targetStatUid, bool includeAttributes = true)
        {
            // If we are not including attributes, just return the itemObject effects
            if (!includeAttributes)
                return GetModificationLevel().GetEffectOn(targetStatUid, 0f, 0f, owner);

            var impactOnValue = 0f;
            var impactOnProficiency = 0f;

            // Loop through all modification levels
            foreach (var level in GetModificationLevels())
            {
                var impact = level.GetEffectOn(targetStatUid, 0f, 0f, owner);
                impactOnValue += impact.Item1;
                impactOnProficiency += impact.Item2;
            }

            return (impactOnValue, impactOnProficiency);
        }

        // Not used in this context.
        public List<Stat> DirectlyAffectedByList(Stat stat = null)
        {
            throw new NotImplementedException();
        }

        public List<Stat> DirectlyAffectsList(Stat stat = null)
        {
            // Start with the ItemObject
            var affectsList = ModificationLevels
                .SelectMany(x => x.targets)
                .Distinct()
                .ToList();

            // Add the ItemAttributes
            foreach (var itemAttribute in attributeList.list)
                affectsList.AddRange(itemAttribute.DirectlyAffectsList());

            return affectsList.Distinct().ToList(); // Return a list with distinct attributes
        }
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public GameItemObject Clone(bool forceNewGameId = false)
        {
            // Note: The clone will clone the Dictionaries object after, or it will continue to be linked directly to
            // the original object.
            var clonedObject = JsonUtility.FromJson<GameItemObject>(JsonUtility.ToJson(this));
            clonedObject.dictionaries = clonedObject.dictionaries.Clone();
            clonedObject.GameId(forceNewGameId);
            return clonedObject;
        }
        
        public void SetSpots(int spotY, int spotX)
        {
            inventorySpotY = spotY;
            inventorySpotX = spotX;
        }

        /// <summary>
        /// Relinks the prefabInventory and prefabWorld from the ItemObject scriptable object. Required after loading data
        /// if using the Inventory module.
        /// </summary>
        public void RelinkPrefabs()
        {
            prefabInventory = Parent().prefabInventory;
            prefabWorld = Parent().prefabWorld;
        }
        
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);

        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
    }
}