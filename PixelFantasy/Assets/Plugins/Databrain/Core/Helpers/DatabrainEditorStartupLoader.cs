/*  
 *  DATABRAIN
 *  (c) 2023 by Giant Grey / www.giantgrey.com
 *  Author: Marc Egli
 *  
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

namespace Databrain.Helpers
{
    /// <summary>
    /// Restores all open Databrain editor instances after Unity loads
    /// </summary>
    [InitializeOnLoad]
    public class DatabrainEditorStartupLoader
    {
        [InitializeOnLoadMethod]
        public static async void OnLoad()
        {
            await Task.Delay(2000);

            DatabrainEditorWindow[] _w = Resources.FindObjectsOfTypeAll<DatabrainEditorWindow>();
            //Debug.Log("Databrain editor reloading.");
            for (int i = 0; i < _w.Length; i++)
            {
                DataLibrary _container = EditorUtility.InstanceIDToObject(_w[i].container.GetInstanceID()) as DataLibrary;
                if (_container != null)
                {
                    DatabrainHelpers.OpenEditor(_w[i].container.GetInstanceID(), false);
                }
            }
        }
    }
}
#endif