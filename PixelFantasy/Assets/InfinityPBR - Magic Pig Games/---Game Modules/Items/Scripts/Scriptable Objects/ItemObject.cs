using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static InfinityPBR.Modules.Utilities;
using static InfinityPBR.Modules.GameModuleUtilities;

/*
 * ITEM OBJECT
 *
 * The "Item Object" can be a sword, gun, coffee cup -- or it can be any other "thing" that can be obtained. These can
 * affect Stat through that module. For example, you may create a list of "Conditions" (Poisoned, Tired, Heroic,
 * Happy etc), which affect various Stat. There's a LOT of ways you can use ItemObjects that have little to
 * do with physical objects.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items")]
    [CreateAssetMenu(fileName = "Item Object", menuName = "Game Modules/Items/Item Object", order = 1)]
    [Serializable]
    public class ItemObject : ModulesScriptableObject, IAffectStats, IFitInInventory
    {
        public string FullName => GetFullName();
        private string GetFullName() => objectName; // In this context, the objectName IS the full name.

        public bool questItem;

        public List<ItemAttribute> itemAttributes = new List<ItemAttribute>(); // These are ItemAttributes active on this object
        public List<ItemAttribute> startingItemAttributes = new List<ItemAttribute>(); // Will be added whenever a GameItemObject is made from this
        [FormerlySerializedAs("availableItemAttributes")] public List<ItemAttribute> allowedItemAttributes = new List<ItemAttribute>(); // Item attributes which can be on this object
        [FormerlySerializedAs("availableItemAttributeObjectTypes")] public List<string> allowedItemAttributeObjectTypes = new List<string>(); // Types of Item Attributes which can be on this object
        public ModificationLevel modificationLevel; // The modification level for this object
        public List<ItemObjectVariable> variables = new List<ItemObjectVariable>();

        // INVENTORY MODULE / IFitInInventory
        public int GetSpotInRow() => inventorySpotY;
        public int GetSpotInColumn() => inventorySpotX;
        public int GetInventoryWidth() => inventoryWidth;
        public int GetInventoryHeight() => inventoryHeight;
        public GameObject GetWorldPrefab() => prefabWorld;
        public GameObject GetInventoryPrefab() => prefabInventory;
        
        public int inventorySpotY;
        public int inventorySpotX;
        public int inventoryHeight;
        public int inventoryWidth;
        public GameObject prefabWorld;
        public GameObject prefabInventory;
        
        // Editor/Inspector
        [HideInInspector] public bool showItemAttributes;
        [HideInInspector] public bool showInventory;
        [HideInInspector] public bool showItemAttributeTypes;
        [HideInInspector] public bool showItemAttributesAttributes;
        [HideInInspector] public bool showLootItems;
        [HideInInspector] public bool showDictionaries;
        [HideInInspector] public bool showStats;
        [HideInInspector] public bool hasBeenSetup;
        [HideInInspector] public int menubarIndex;
        [HideInInspector] public bool showAllowedAttributes = true;
        [HideInInspector] public bool showStartingAttributes = true;
        [HideInInspector] public bool showCopyPaste = true;

        // -------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // -------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the Modification Level on this as a List<StatsModificationLevel>
        /// </summary>
        /// <returns></returns>
        /// NOTE: Currently ItemObject only has one modification level, so both of these return essentially the same
        /// thing, but perhaps in the future I'll add the concept of an ItemObject itself being "leveled up", in which
        /// case it would have multiple modification levels.
        public List<ModificationLevel> GetModificationLevels() => new List<ModificationLevel> { modificationLevel };

        // Returns the Modification Level object attached to this Item Object
        public ModificationLevel GetModificationLevel() => modificationLevel;
        
        // Unused in this context, required by IAffectStats
        public void SetOwner(IHaveStats newOwner) { }
        public IHaveStats GetOwner() => default;
        public void SetAffectedStatsDirty(object obj)
        {
            throw new NotImplementedException();
        }

        // These will return true if this Item Object can use the specific Item Attribute provided
        public bool CanUseAttribute(ItemAttribute itemAttribute) => allowedItemAttributes.Contains(itemAttribute);
        public bool CanUseAttribute(string itemAttributeUid) => allowedItemAttributes.FirstOrDefault(x => x.Uid() == itemAttributeUid);
        
        // Will return true if ALL of the provided Item Attribute uids can be used.
        public bool CanUseAttributes(string[] itemAttributeUids)
        {
            foreach (string uid in itemAttributeUids)
                if (!CanUseAttribute(uid)) return false;

            return true;
        }
        
        // Returns true if the Item Object can use attributes of this type. It does not check if the individual attribute
        // provided can be used, just the type that the Item Attribute is part of.
        public bool CanUseAttributeType(string type) => allowedItemAttributeObjectTypes.Contains(type);
        public bool CanUseAttributeType(ItemAttribute itemAttribute) => CanUseAttributeType(itemAttribute.objectType);
        
        // Returns true if the provided key exists in the Dictionaries on this Item Object.
        //public bool HasKeyValue(string key) => dictionaries.keyValues.Any(x => x.key == key);

        // -------------------------------------------------------------------------------------
        // EDITOR / INSPECTOR
        // -------------------------------------------------------------------------------------

        // Turns on/off whether we can use this Item Attribute type
        public void ToggleCanUseAttributeType(string type) => SetCanUseAttributeType(type, !CanUseAttributeType(type));
        
        // Sets the specific value for whether we can or can not use this Attribute Type
        public void SetCanUseAttributeType(string type, bool setCanUse)
        {
            if (setCanUse)
            {
                allowedItemAttributeObjectTypes.Add(type);
                return;
            }
            
            allowedItemAttributeObjectTypes.RemoveAll(x => x == type);
            allowedItemAttributes.RemoveAll(x => x.objectType == type);
        }
        
        public void ToggleAllItemObjectsHasAttribute(ItemAttribute itemAttribute, bool setCanUse)
        {
            Debug.Log("Toggle: " + objectName + " can use " + setCanUse + " for " + itemAttribute.objectName);
            if (!CanUseAttributeType(itemAttribute.objectType))
                return;
            
            if (setCanUse && !CanUseAttribute(itemAttribute))
            {
                allowedItemAttributes.Add(itemAttribute);
                return;
            }
            
            allowedItemAttributes.RemoveAll(x => x.objectName == itemAttribute.objectName);
        }
        
        public void ToggleAllItemObjectsHasAttribute(ItemAttribute itemAttribute)
        {
            if (!CanUseAttributeType(itemAttribute.objectType))
                return;

            if (CanUseAttributeType(itemAttribute))
            {
                ToggleAllItemObjectsHasAttribute(itemAttribute, false);
                return;
            }

            ToggleAllItemObjectsHasAttribute(itemAttribute, true);
        }

        public void ToggleCanUseItemAttribute(ItemAttribute itemAttribute)
        {
            if (CanUseAttribute(itemAttribute))
                allowedItemAttributes.RemoveAll(x => x == itemAttribute);
            else
                allowedItemAttributes.Add(itemAttribute);
        }
        
        public void ToggleCanUseItemAttribute(ItemAttribute itemAttribute, bool canUse)
        {
            if (canUse)
            {
                if (!CanUseAttribute(itemAttribute))
                    allowedItemAttributes.Add(itemAttribute);
                return;
            }
            
            allowedItemAttributes.RemoveAll(x => x == itemAttribute);
        }

        // Copy/Paste for "Allowed Item Attributes"
        public void CopyAllowedItemAttributes(ItemObject copyItemObject, string itemAttributeType = null, bool append = false)
        {
            // Handle clearing when itemAttributeType is not provided (clear all)
            if (!append && itemAttributeType == null)
            {
                allowedItemAttributes.Clear();
                allowedItemAttributeObjectTypes.Clear();
            }

            // Handle clearing when itemAttributeType is provided (only clear that type)
            if (!append && itemAttributeType != null)
            {
                for (var index = allowedItemAttributes.Count - 1; index >= 0; index--)
                {
                    var attribute = allowedItemAttributes[index];
                    if (attribute.objectType != itemAttributeType)
                        continue;

                    allowedItemAttributes.RemoveAt(index);
                }

                allowedItemAttributeObjectTypes.Remove(itemAttributeType);
            }

            
            foreach (var attribute in copyItemObject.allowedItemAttributes)
            {
                // If we have a provided itemAttributeType and this isn't of that type, then continue
                if (itemAttributeType != null && attribute.objectType != itemAttributeType)
                    continue;
                
                // If we already have it, and we are appending, then continue
                if (append && allowedItemAttributes.Contains(attribute))
                    continue;
                
                allowedItemAttributes.Add(attribute);

                // Handle the types list
                if (allowedItemAttributeObjectTypes.Contains(attribute.objectType))
                    continue;

                allowedItemAttributeObjectTypes.Add(attribute.objectType);
            }
        }
        
        // Copy/Paste for "Starting Item Attributes"
        public void CopyStartingAttributes(ItemObject copyItemObject, bool append = false, bool onePerType = false)
        {
            if (!append)
                startingItemAttributes.Clear();

            foreach (var attribute in copyItemObject.startingItemAttributes)
            {
                // If we are appending and the list already contains this, continue
                if (append && HasStartingItemAttribute(attribute))
                    continue;

                // If we are keeping to one per type and the list already has an object of this type, continue
                if (onePerType && HasStartingItemAttributeType(attribute.objectType))
                    continue;

                // Ensure the new itemAttribute is not distinct, or the list doesn't contain that type already
                if (attribute.distinct && HasStartingItemAttributeType(attribute.objectType))
                {
                    Debug.Log($"{attribute.objectType} must be distinct, and there is already an attribute of " +
                              "that type in the list.");
                    continue;
                }
                
                // Check to see if the objectType is in variables
                if (HasVariableOfAttributeType(attribute))
                {
                    Debug.Log($"Type {attribute.objectType} is already represented in Variables, so we will not " +
                              $"add {attribute.objectName} to the Starting Item Attributes list.");
                    continue;
                }
                
                startingItemAttributes.Add(attribute);

                ForceCanUseAttribute(attribute);
            }
        }

        public void ForceCanUseAttribute(ItemAttribute itemAttribute)
        {
            if (CanUseAttribute(itemAttribute)) return;
            
            ToggleCanUseItemAttribute(itemAttribute);
            if(!CanUseAttributeType(itemAttribute.objectType))
                ToggleCanUseAttributeType(itemAttribute.objectType);
            Debug.LogWarning($"Important: {itemAttribute.objectName} was not set as an attribute that can be used by {objectName}. " +
                             $"This has now been set true. If you did not intend to select {itemAttribute.objectName}, please undo this action.");
        }

        /*
         * ITEM OBJECT VARIABLE ETC CHECKS
         */
        
        // Check to make sure it's not in variables
        public bool HasVariableOfAttributeType(ItemAttribute attributeToAdd) => HasVariableOfAttributeType(attributeToAdd.objectType);
        
        public bool HasVariableOfAttributeType(string typeToCheck) 
            => variables
                .Any(x => x.variableAttributes
                    .Any(y => y.ItemAttribute.objectType == typeToCheck));

        // Returns true if any Variables is named nameToCheck
        public bool HasVariableName(string nameToCheck) => variables.Any(x => x.name == nameToCheck);

        public bool HasStartingItemAttribute(ItemAttribute itemAttribute) => startingItemAttributes.Contains(itemAttribute);
        public bool HasStartingItemAttributeType(string typeToCheck) 
            => startingItemAttributes.Any(x => x.objectType == typeToCheck);
        

        /// <summary>
        /// Returns a List<Stat> of objects that are directly affected by the Stat on this object
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public List<Stat> DirectlyAffectsList(Stat stat)
        {
            var tempList = new List<Stat>();
            foreach (var statModification in modificationLevel.modifications)
            {
                if (statModification.HasNoEffect()) continue;
                if (statModification.isBase || statModification.isPerSkillPoint) continue;
                if (statModification.source != stat && (statModification.multiplierUid != stat.Uid() ||
                                                        statModification.sourceCalculationStyle != 4)) continue;
                if (tempList.Contains(statModification.target)) continue;
                
                tempList.Add(statModification.target);
            }

            return tempList;
        }

        /// <summary>
        /// Returns a List<Stat> of objects that are directly affected by the provided stat on this object
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public List<Stat> DirectlyAffectedByList(Stat stat)
        {
            var tempList = new List<Stat>();
            if (modificationLevel.targets.Contains(stat))
            {
                foreach (StatModification mod in modificationLevel.modifications)
                {
                    if (mod.HasNoEffect()) continue;
                    if (mod.source == null) continue;
                    if (mod.target != stat) continue;
                    
                    if (!tempList.Contains(mod.source))
                        tempList.Add(mod.source);
                    
                    if (mod.sourceCalculationStyle != 4) continue;
                    
                    if (!tempList.Contains(GetStatByUid(mod.multiplierUid)))
                        tempList.Add(GetStatByUid(mod.multiplierUid));
                }
            }

            return tempList;
        }
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public ItemObject Clone() => JsonUtility.FromJson<ItemObject>(JsonUtility.ToJson(this));

        public void SetSpots(int spotY, int spotX)
        {
            inventorySpotY = spotY;
            inventorySpotX = spotX;
        }
        
        public void ClearMissingAttributes() => allowedItemAttributes.RemoveAll(x => x == null); // Removes any empty Item Attribute entries

        public void CopyVariables(ItemObject copyObject, bool append = false)
        {
            // Handle clearing when itemAttributeType is not provided (clear all)
            if (!append)
                variables.Clear();

            foreach (var variable in copyObject.variables)
            {
                var variableType = variable.ObjectType;
                
                // Continue if we already have a variable that uses the same ItemAttribute objectType
                if (HasVariableOfAttributeType(variableType))
                    continue;

                // Add a clone to the list
                variables.Add(variable.Clone);
                
                // Remove from Starting Item Attributes any of this objectType
                if (HasStartingItemAttributeType(variable.ObjectType))
                {
                    startingItemAttributes.RemoveAll(x => x.objectType == variable.ObjectType);
                    Debug.LogWarning($"Important: There was at least one Starting Item Attribute of type {variable.ObjectType}, " +
                                     "which has been removed. If you did not intend this, then please \"Undo\". Keep in mind " +
                                     "that Starting Item Attributes and Variables can not each have ItemAttributes of the " +
                                     "same objectType.");
                }

                if (string.IsNullOrWhiteSpace(variableType))
                    continue;
                
                // Make sure the object can use the attributes
                foreach (var attribute in GameModuleObjectsOfType<ItemAttribute>(variableType))
                    ForceCanUseAttribute(attribute);
            }
        }

        public ItemObjectVariable VariableOfType(string variableObjectType) 
            => variables.FirstOrDefault(x => x.ObjectType == variableObjectType);
    }
}