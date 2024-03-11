using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Examples
{
    public class SaveLoadDemoUI : MonoBehaviour
    {

        public DataLibrary dataLibrary;
        public SaveLoadDemo demoScript;

        [Header("UI")]
        public VisualTreeAsset uiButtonAsset;
        public UIDocument uiDocument;

        private List<DataObject> dataObjects = new List<DataObject>(); 

        void Start()
        {
            // Build UI based on the available objects
            dataObjects = dataLibrary.GetAllInitialDataObjectsByType(typeof(SaveLoadDemoData));

            if (dataObjects != null)
            {
                var _root = uiDocument.rootVisualElement;
                var _buttonBar = _root.Q<VisualElement>("bar");

                // create buttons for each available object in the data library
                for (int i = 0; i < dataObjects.Count; i++)
                {
                    uiButtonAsset.CloneTree(_buttonBar);
                    var _button = _buttonBar.Q<Button>("button");

                    _button.name = i.ToString();
                    _button.text = dataObjects[i].title;

                    int _index = i;
                    _button.RegisterCallback<ClickEvent>(click =>
                    {
                        demoScript.SelectObject(dataObjects[_index]);
                    });
                }


                // Register callbacks for save load and clear buttons
                var _saveButton = _root.Q<Button>("saveButton");
                var _loadButton = _root.Q<Button>("loadButton");
                var _clearButton = _root.Q<Button>("clearButton");

                _saveButton.RegisterCallback<ClickEvent>(click =>
                {
                    demoScript.Save();
                });

                _loadButton.RegisterCallback<ClickEvent>(click =>
                {
                    demoScript.Load();
                });

                _clearButton.RegisterCallback<ClickEvent>(click =>
                {
                    demoScript.Clear();
                });
            }
        }

    }
}