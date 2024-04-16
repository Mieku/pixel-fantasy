using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ItemObjectVariable
    {
        public string name;
        public GameItemObject parent;
        public float value;
        public bool useCurve;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        // This is the 0-1 value for value with min / max. 
        // If min = 100 and max = 500, and value = 250...
        // rangeValue = value / (max - min) = 250 / (500 - 100) = 250 / 400 = 0.625
        public float rangeValue = 1;
        public float min;
        public float max = 1;
        
        public List<ItemObjectVariableAttribute> variableAttributes = new List<ItemObjectVariableAttribute>();

        public ItemAttribute ActiveAttribute => variableAttributes[GetAttributeIndex()].ItemAttribute;
        public int GetAttributeIndex()
        {
            if (variableAttributes.Count == 0)
                return -1;
            if (RangeValue < variableAttributes[0].value)
                return -1;
            for (var i = 0; i < variableAttributes.Count; i++)
            {
                if (RangeValue < variableAttributes[i].value)
                    return i - 1;
            }
            return variableAttributes.Count - 1;
        }
        
        // Inspector
        [HideInInspector] public bool show = true;

        // If there are variable attributes, this will return the ObjectType of the ItemAttributes.
        public string ObjectType => variableAttributes.Count > 0 ? variableAttributes[0].ItemAttribute.objectType : "";

        public ItemObjectVariable Clone => JsonUtility.FromJson<ItemObjectVariable>(JsonUtility.ToJson(this));

        public void CopyValuesFrom(ItemObjectVariable other)
        {
            name = other.name;
            value = other.value;
            useCurve = other.useCurve;
            curve = new AnimationCurve(other.curve.keys);
            rangeValue = other.rangeValue;
            min = other.min;
            max = other.max;
            variableAttributes = new List<ItemObjectVariableAttribute>(other.variableAttributes.Select(x => new ItemObjectVariableAttribute
            {
                _itemAttribute = x._itemAttribute,
                _itemAttributeUid = x._itemAttributeUid,
                value = x.value
            }));
        }
        
        /// <summary>
        /// Adds to the value. Add negative to reduce value. Will return the new RangeValue.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public float AddValue(float newValue) => SetValue(value + newValue);

        /// <summary>
        /// Sets the value, and returns the 0-1 rangeValue, or the result of the curve if one is being used
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public float SetValue(float newValue, bool skipAttributeSet = false)
        {
            // Set the value (clamped between min/max) and compute the rangeValue
            value = Mathf.Clamp(newValue, min, max);
            rangeValue = Mathf.InverseLerp(min, max, value);

            // Set the ItemAttribute, if needed
            if (!skipAttributeSet)
                SetAttribute();
            
            // Return the evaluation from the curve, or the rangeValue if we aren't using the curve
            return RangeValue;
        }

        /// <summary>
        /// Computes a new value between min and max based on a provide RangeValue
        /// </summary>
        /// <param name="newRangeValue"></param>
        /// <returns></returns>
        public float SetToRangeValue(float newRangeValue)
        {
            SetValue(min + (max - min) * newRangeValue);
            return value;
        }
        
        public float SetToMax() => SetValue(max);
        public float SetToMin() => SetValue(min);

        /// <summary>
        /// Returns the 0-1 range value, or the Animation Curve value if that is being used
        /// </summary>
        /// <returns></returns>
        public float RangeValue => useCurve ? curve.Evaluate(rangeValue) : rangeValue;

        public void AddAttribute(ItemAttribute newAttribute, float newValue)
        {
            if (variableAttributes.Any(x => x.ItemAttribute == newAttribute))
            {
                Debug.Log($"Attribute {newAttribute.objectName} is already in the list.");
                return;
            }
            
            variableAttributes.Add(new ItemObjectVariableAttribute
            {
                _itemAttribute = newAttribute,
                _itemAttributeUid = newAttribute.Uid(),
                value = Utilities.RoundToDecimal(newValue, 3)
            });

            // Order them to be 0 --> 1
            variableAttributes.OrderBy(x => x.value);
        }

        /// <summary>
        /// Removes all GameItemAttributes from the list with the objectType of the newAttribute
        /// </summary>
        /// <param name="newAttribute"></param>
        public void RemoveAttribute(ItemAttribute newAttribute) => variableAttributes.RemoveAll(x 
            => x.ItemAttribute == newAttribute);

        /// <summary>
        /// Will set the GameItemAttribute on the GameItemObject based on the current RangeValue. Does nothing if we
        /// are not using variableAttributes, or if the GameItemObject already has the attribute.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="resetModificationLevel"></param>
        public void SetAttribute(GameItemObject gameItemObject = null, bool resetModificationLevel = true)
        {
            // Return out of this if we are in the editor and we aren't in play mode.
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            
            // If we don't have any attributes, then return
            if (variableAttributes.Count == 0)
                return;

            gameItemObject ??= parent; // if gameItemObject is not provided, use parent
            
            if (gameItemObject == null)
            {
                Debug.LogWarning("SetAttribute called with no GameItemObject and no parent.");
                return;
            }

            // Find the last attribute which is greater than RangeValue
            var attribute = variableAttributes.Last(x => x.value <= RangeValue);
            
            // If we already have the attribute in the list, do nothing
            if (gameItemObject.attributeList.Contains(attribute.ItemAttribute))
                return;
            
            // Remove others of this same type
            gameItemObject.attributeList.RemoveType(attribute.ItemAttribute.objectType);
            
            // Add the attribute
            gameItemObject.AddAttribute(attribute.ItemAttribute.Uid());

            // If we aren't resetting the modificaiton level, we're done. Generally this will be false when we have
            // perhaps more than one operation to accomplish before we finally reset it.
            if (!resetModificationLevel) return;

            gameItemObject.ResetItemModificationLevel();
        }
    }
}