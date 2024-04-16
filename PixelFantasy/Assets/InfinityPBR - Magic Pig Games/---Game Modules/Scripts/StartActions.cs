using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// This will start the StartActions() Coroutine on all objects in the list

namespace InfinityPBR.Modules
{
    [System.Serializable]
    public class StartActions : MonoBehaviour
     {
         public List<GameObject> objectsWithStartActions = new List<GameObject>();

         [SerializeField] public List<MonoBehaviour> cachedComponents = new List<MonoBehaviour>();
         [SerializeField] public List<string> cachedFieldNames = new List<string>();

         public Dictionary<MonoBehaviour, List<FieldInfo>> cachedComponentsAndFields;

         private void Awake() 
         {
             cachedComponentsAndFields = cachedComponents.Zip(cachedFieldNames, (comp, fieldName) => new { comp, name = fieldName })
                 .GroupBy(x => x.comp)
                 .ToDictionary(g => g.Key, g => g.Select(x => x.comp.GetType().GetField(x.name)).ToList());

             StartCoroutine(StartAllActions());
         }

         private IEnumerator StartAllActions()
         {
             var components = GetComponents<MonoBehaviour>();

             foreach (var component in components)
             {
                 if (!cachedComponentsAndFields.ContainsKey(component)) continue;
                    
                 foreach (var field in cachedComponentsAndFields[component])
                 {
                     if (field.GetValue(component) is not IHaveStartActions actionComponent) continue;
                        
                     yield return StartCoroutine(actionComponent.StartActions());
                 }
             }
         }
     }
}