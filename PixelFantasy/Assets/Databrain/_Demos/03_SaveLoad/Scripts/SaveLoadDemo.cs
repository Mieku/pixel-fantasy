using System.Collections.Generic;

using UnityEngine;

namespace Databrain.Examples
{
    public class SaveLoadDemo : MonoBehaviour
    {
        public DataLibrary saveLoadDemoDataCore;

        // Cache the runtime data temporarily
        private SaveLoadDemoData runtimeData;

        private GameObject tmpObject;

        private Vector2 _cellSize = new Vector2(1, 1);
        private Plane _plane = new Plane(Vector3.up, 0);
        private Vector3 _worldPosition;

        private List<GameObject> objects = new List<GameObject>();

        private void Start()
        {
            // Register to events
            saveLoadDemoDataCore.OnSaved += Saved;
            saveLoadDemoDataCore.OnLoaded += Loaded;
        }

        void OnDestroy()
        {
            saveLoadDemoDataCore.OnLoaded -= Loaded;
            saveLoadDemoDataCore.OnSaved -= Saved;
        }

        // Event OnSaved has been called
        void Saved()
        {
            Debug.Log("DATA SAVED");
        }

        // Event OnLoaded has been called
        void Loaded()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                Destroy(objects[i]);
            }

            objects = new List<GameObject>();

            // Rebuild the prefabs from the runtime datacore object
            var _objects = saveLoadDemoDataCore.GetAllRuntimeDataObjectsByType(typeof(SaveLoadDemoData));


            for (int j = 0; j < _objects.Count; j++)
            {
                var _obj = (_objects[j] as SaveLoadDemoData);

                var _d = Instantiate((_obj.GetInitialDataObject() as SaveLoadDemoData).prefab);
                _d.transform.position = _obj.position;
                objects.Add(_d);
            }
        }

        // UI METHODS
        #region UIMethods

        /// <summary>
        /// Select and instantiate the object
        /// </summary>
        /// <param name="_object"></param>
        public void SelectObject(DataObject _object)
        {
            var _data = _object as SaveLoadDemoData;
            var _obj = Instantiate(_data.prefab);
            tmpObject = _obj;
            runtimeData = saveLoadDemoDataCore.CloneDataObjectToRuntime(_data, _obj.gameObject) as SaveLoadDemoData;
        }

        public void Save()
        {
            saveLoadDemoDataCore.Save(System.IO.Path.Combine(Application.persistentDataPath, "saveloaddemo.json"));
        }

        public void Load()
        {
            saveLoadDemoDataCore.Load(System.IO.Path.Combine(Application.persistentDataPath, "saveloaddemo.json"));
        }
        public void Clear()
        {
            var _dataObjects = saveLoadDemoDataCore.GetAllRuntimeDataObjectsByType(typeof(SaveLoadDemoData));

            for (int i = 0; i < objects.Count; i++)
            {
                Destroy(objects[i]);
            }

            // Cleanup runtime datacore object
            for (int i = 0; i < _dataObjects.Count; i++)
            {
                saveLoadDemoDataCore.RemoveDataObjectFromRuntime(_dataObjects[i]);
            }

            var tt = saveLoadDemoDataCore.GetAllRuntimeDataObjectsByType(typeof(SaveLoadDemoData));

            objects = new List<GameObject>();
        }
        #endregion

        // Update is called once per frame
        void Update()
        {
            if (tmpObject != null)
            {
                // Move object on a grid
                float _distance = 0f;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (_plane.Raycast(ray, out _distance))
                {
                    _worldPosition = ray.GetPoint(_distance);
                }

                Vector3 pos = _worldPosition;
                pos.x = Mathf.RoundToInt(pos.x / _cellSize.x) * _cellSize.x;
                pos.z = Mathf.RoundToInt(pos.z / _cellSize.y) * _cellSize.y;
                tmpObject.transform.position = pos;


                // Input handles
                // Place selected object
                if (Input.GetMouseButton(0))
                {
                    // Build and set position information to the runtime data
                    runtimeData.position = tmpObject.transform.position;

                    objects.Add(tmpObject);

                    tmpObject = null;
                }

                // Abort
                if (Input.GetMouseButton(1))
                {
                    Destroy(tmpObject);
                }

            }
        }
    }
}