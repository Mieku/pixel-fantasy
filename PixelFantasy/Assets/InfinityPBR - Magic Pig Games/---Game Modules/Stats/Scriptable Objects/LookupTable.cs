using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Lookup Table", menuName = "Game Modules/Create/Lookup Table", order = 1)]
    [Serializable]
    public class LookupTable : ModulesScriptableObject, IHaveUid
    {
        public enum LookupTableOrder
        {
            Manual,
            InputAsc,
            InputDesc,
            OutputAsc,
            OutputDesc,
            Shuffle
        }
        
        public LookupTableOrder orderBy = LookupTableOrder.Manual;
        
        public List<LookupTableValues> table = new List<LookupTableValues>();
        public float ResultFrom(float inputValue) => GetOutputValue(inputValue);

        [HideInInspector] public int menubarIndex;
        [HideInInspector] public bool showCurves = true;
        [HideInInspector] public bool useInputCurve;
        [HideInInspector] public bool useOutputCurve;
        [HideInInspector] public int inputDecimals;
        [HideInInspector] public int outputDecimals;
        [HideInInspector] public float inputCurveMin;
        [HideInInspector] public float inputCurveMax = 1f;
        [HideInInspector] public float outputCurveMin;
        [HideInInspector] public float outputCurveMax = 1f;
        [HideInInspector] public bool showAddValue = true;
        [HideInInspector] public AnimationCurve inputCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [HideInInspector] public AnimationCurve outputCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        private float GetOutputValue(float inputValue)
        {
            if (table.Count == 0) return 0f;

            // Check if input is less than lowest input value
            if (inputValue <= table[0].input)
                return table[0].output;
            
            float outputValue = 0f;
            foreach (LookupTableValues values in table)
            {
                if (values.input <= inputValue)
                {
                    outputValue = values.output;
                    continue;
                }

                if (values.input > inputValue)
                    return outputValue;
            }

            return outputValue;
        }
        
        public bool Add(float newInput, float newOutput)
        {
            if (HasInputValue(newInput)) return false;
            table.Add(new LookupTableValues(newInput, newOutput));
            return true;
        }

        public void OrderTableInputAsc() => table = table.OrderBy(x => x.input).ToList();
        public void OrderTableInputDesc() => table = table.OrderBy(x => x.input).Reverse().ToList();
        public void OrderTableOutputAsc() => table = table.OrderBy(x => x.output).ToList();
        public void OrderTableOutputDesc() => table = table.OrderBy(x => x.output).Reverse().ToList();
        public void ShuffleTableOrder() => table.Shuffle();
        public void OrderTableManually() => table = table.OrderBy(x => x.manualOrder).ToList();

        private bool HasInputValue(float inputValue) 
            => table.FirstOrDefault(x => Math.Abs(x.input - inputValue) < 0.01) != null;

        public void RemoveLookupTable(float inputValue) 
            => table.RemoveAll(x => Math.Abs(x.input - inputValue) < 0.01);

        public void Reorder()
        {
            if (orderBy == LookupTableOrder.InputAsc) OrderTableInputAsc();
            if (orderBy == LookupTableOrder.InputDesc) OrderTableInputDesc();
            if (orderBy == LookupTableOrder.OutputAsc) OrderTableOutputAsc();
            if (orderBy == LookupTableOrder.OutputDesc) OrderTableOutputDesc();
            if (orderBy == LookupTableOrder.Shuffle) ShuffleTableOrder();
            if (orderBy == LookupTableOrder.Manual) OrderTableManually();
        }

        public void SaveManualOrder()
        {
            for (var i = 0; i < table.Count; i++)
                table[i].manualOrder = i;
        }
    }

    [Serializable]
    public class LookupTableValues
    {
        public float input;
        public float output;
        public float manualOrder;

        public LookupTableValues(float newInput, float newOutput)
        {
            input = newInput;
            output = newOutput;
        }
    }
}
