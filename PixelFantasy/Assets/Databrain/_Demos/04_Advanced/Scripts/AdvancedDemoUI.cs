using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Examples
{
    public class AdvancedDemoUI : MonoBehaviour
    {
        public DataLibrary data;
        public AdvancedDemoManager manager;
        public UIDocument uiDocument;


        public void Start()
        {

            var _root = new VisualElement();
            _root.style.flexGrow = 1;

            var _toolbar = new VisualElement();
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.height = 40;
            _toolbar.style.backgroundColor = Color.white;

            var _space = new VisualElement();
            _space.style.flexGrow = 1;

            var _bottom = new VisualElement();
            _bottom.style.flexDirection = FlexDirection.Row;
            _bottom.style.height = 40;
            _bottom.style.backgroundColor = Color.white;

            var _label = new Label();
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.text = "Click on capsules to change health";


            _bottom.Add(_label);

            var _saveButton = new Button();
            _saveButton.text = "Save";
            _saveButton.RegisterCallback<ClickEvent>(click =>
            {
                Save();
            });

            var _loadButton = new Button();
            _loadButton.text = "Load";
            _loadButton.RegisterCallback<ClickEvent>(click =>
            {
                Load();
            });

            var _spawnEnemy = new Button();
            _spawnEnemy.text = "Spawn Enemy +1";
            _spawnEnemy.RegisterCallback<ClickEvent>(click =>
            {
                manager.SpawnEnemy(1);
            });


            var _spawnEnemy10 = new Button();
            _spawnEnemy10.text = "Spawn Enemy +10";
            _spawnEnemy10.RegisterCallback<ClickEvent>(click =>
            {
                manager.SpawnEnemy(10);
            });



            var _clearScene = new Button();
            _clearScene.text = "Clear Scene";
            _clearScene.RegisterCallback<ClickEvent>(click =>
            {
                manager.ClearScene();
            });


            _toolbar.Add(_saveButton);
            _toolbar.Add(_loadButton);
            _toolbar.Add(_spawnEnemy);
            _toolbar.Add(_spawnEnemy10);
            _toolbar.Add(_clearScene);


            _root.Add(_toolbar);
            _root.Add(_space);
            _root.Add(_bottom);

            uiDocument.rootVisualElement.Add(_root);

        }

        void Save()
        {
            data.Save(System.IO.Path.Combine(Application.persistentDataPath, "databrain_04_demo_save.json"));
        }

        void Load()
        {
            // first clear scene
            manager.ClearScene();


            data.Load(System.IO.Path.Combine(Application.persistentDataPath, "databrain_04_demo_save.json"));
        }

    }
}