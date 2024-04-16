using System.Reflection;
using UnityEditor;
using InfinityPBR.Modules;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(StartActions))]
    [CanEditMultipleObjects]
    public class StartActionsEditor : InfinityEditor
    {
        public override void OnInspectorGUI()
        {
            var startActions = (StartActions)target;
            
            Header1("Start Actions");
            Label(
                $"<color=#999999>This component will trigger the <color=#99ffff><b>IEnumerator StartActions()</b></color> on all <b>IHaveStartActions</b> classes on this object. " +
                $"There are <color=#99ffff><b>{startActions.cachedComponents.Count} " +
                $"<b>IHaveStartActions</b></b></color> components on this game object</color> \n<color=#555555><i>Components update automatically when this Inspector is viewed.</i></color>",false, true, true);
            for (var index = 0; index < startActions.cachedComponents.Count; index++)
            {
                var component = startActions.cachedComponents[index];
                var field = startActions.cachedFieldNames[index];
                Label($"<color=#999999> - {component} {field}</color>", false, true, true);
                
            }
        }
        
        private void OnEnable() => CacheStatsComponents();

        /// <summary>
        /// Loops through all Monobehaviours on this object, and all fields, and will cache any fields that are IHaveStartActions.
        /// </summary>
        public void CacheStatsComponents()
        {
            var startActions = (StartActions)target;
            
            startActions.cachedComponents.Clear();
            startActions.cachedFieldNames.Clear();
            
            foreach (var component in startActions.GetComponents<MonoBehaviour>())
            {
                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.GetValue(component) is not IHaveStartActions) continue;
                    
                    startActions.cachedComponents.Add(component);
                    startActions.cachedFieldNames.Add(field.Name);
                }
            }
            
            EditorUtility.SetDirty(startActions);
        }
    }
}