/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Attributes;
using Databrain.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.UI
{

    public class DatabrainWelcome : EditorWindow
    {

        int selectedDropdownIndex;
        List<string> choices = new List<string>() { "On startup", "On new version", "Never" };

        VisualElement rightContainer;
        VisualElement changelogContainer;
        private string selectedChangelog = "Databrain";

        [InitializeOnLoadMethod]
        public static void Init()
        {
            var _keyValue = EditorPrefs.GetString("DatabrainShowWelcome");

            if (_keyValue == "startup")
            {
                var _hasShown = SessionState.GetString("DatabrainShowWelcome", "");
               
                if (string.IsNullOrEmpty(_hasShown))
                {
                    ShowWelcomeWindowDelayed();
                    SessionState.SetString("DatabrainShowWelcome", "startup");
                }
            }

            if (_keyValue == "newVersion")
            {
                var _newVersion = DatabrainHelpers.GetEditorVersionNumber();
                var _currentVersionString = EditorPrefs.GetString("DatabrainVersion");
                var _currentVersion = System.Version.Parse(_currentVersionString);

                if (_newVersion.CompareTo(_currentVersion) > 0)
                {
                    EditorPrefs.SetString("DatabrainVersion", _newVersion.ToString());
                    ShowWelcomeWindowDelayed();
                }
            }

            if (string.IsNullOrEmpty(_keyValue))
            {
                EditorPrefs.SetString("DatabrainShowWelcome", "startup");
                ShowWelcomeWindowDelayed();
            }

        }

        [MenuItem("Tools/Databrain/Welcome", false, 200)]
        static void ShowWelcomeWindow()
        {
            EditorWindow wnd = EditorWindow.CreateWindow<DatabrainWelcome>();
            wnd.titleContent = new GUIContent("Databrain - Welcome");
            wnd.maxSize = new Vector2(700f, 500f);
            wnd.minSize = wnd.maxSize;
            wnd.Show();
        }

        static async void ShowWelcomeWindowDelayed()
        {
            await Task.Delay(2000);

            ShowWelcomeWindow();
        }

        public void CreateGUI()
        {
            var _root = rootVisualElement;
            WelcomeGUI(_root, 0, true);
        }

        public VisualElement WelcomeGUI(VisualElement _root, int _selectedOption, bool _footer)
        {
            //var _root = rootVisualElement;
            _root.style.flexDirection = FlexDirection.Column;
            _root.style.flexGrow = 1;

            var _mainContainer = new VisualElement();
            _mainContainer.style.flexDirection = FlexDirection.Row;
            _mainContainer.style.flexGrow = 1;

            var _leftContainer = new VisualElement();
            _leftContainer.style.flexGrow = 1;
            _leftContainer.style.minWidth = 200;
            _leftContainer.style.maxWidth = 200;
            _leftContainer.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();

            rightContainer = new VisualElement();
            rightContainer.style.flexGrow = 1;
            DatabrainHelpers.SetPadding(rightContainer, 10, 10, 10, 10);

            var _bottomContainer = new VisualElement();
            if (_footer)
            {
                _bottomContainer.style.flexDirection = FlexDirection.Row;
                _bottomContainer.style.minHeight = 20;
            }

            _mainContainer.Add(_leftContainer);
            _mainContainer.Add(rightContainer);


            var _welcomeButton = BetterButton("Welcome");
            _welcomeButton.RegisterCallback<ClickEvent>(click =>
            {
                ShowWelcome();
            });

            var _addonsButton = BetterButton("Add-ons");
            _addonsButton.RegisterCallback<ClickEvent>(click =>
            {
                ShowAddons();
            });

            var _supportButton = BetterButton("Support");
            _supportButton.RegisterCallback<ClickEvent>(click =>
            {
                ShowSupport();
            });


            var _changelogButton = BetterButton("Changelog");
            _changelogButton.RegisterCallback<ClickEvent>(click =>
            {
                ShowChangelog();
            });

            var _moreButton = BetterButton("More");
            _moreButton.RegisterCallback<ClickEvent>(click =>
            {
                ShowMore();
            });

            _leftContainer.Add(_welcomeButton);
            _leftContainer.Add(_addonsButton);
            _leftContainer.Add(_supportButton);
            _leftContainer.Add(_changelogButton);
            _leftContainer.Add(_moreButton);

            var _dropdown = new DropdownField(choices, selectedDropdownIndex);
            _dropdown.label = "Show window:";
            _dropdown.RegisterValueChangedCallback(x =>
            {
                if (x.newValue != x.previousValue)
                {
                    switch (_dropdown.index)
                    {
                        case 0:
                            EditorPrefs.SetString("DatabrainShowWelcome", "startup");
                            break;
                        case 1:
                            EditorPrefs.SetString("DatabrainShowWelcome", "newVersion");
                            EditorPrefs.SetString("DatabrainVersion", DatabrainHelpers.GetEditorVersionNumber().ToString());
                            break;
                        case 2:
                            EditorPrefs.SetString("DatabrainShowWelcome", "never");
                            break;
                    }
                }
            });

            var _selectedDropdown = EditorPrefs.GetString("DatabrainShowWelcome");
            switch (_selectedDropdown)
            {
                case "startup":
                    _dropdown.index = 0;
                    break;
                case "newVersion":
                    _dropdown.index = 1;
                    break;
                case "never":
                    _dropdown.index = 2;
                    break;
            }

            var _space = new VisualElement();
            _space.style.flexGrow = 1;

            var _versionLabel = new Label();
            _versionLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            _versionLabel.text = "version: " + DatabrainHelpers.GetEditorVersionNumber().ToString();



            if (_footer)
            {
                _bottomContainer.Add(_versionLabel);
                _bottomContainer.Add(_space);
                _bottomContainer.Add(_dropdown);
            }

            _root.Add(_mainContainer);
            _root.Add(_bottomContainer);

            switch (_selectedOption)
            {
                case 0:
                    ShowWelcome();
                    break;
                case 1:
                    ShowAddons();
                    break;
                case 2:
                    ShowSupport();
                    break;
                case 3:
                    ShowChangelog();
                    break;
                case 4:
                    ShowMore();
                    break;
            }


            return _root;
        }

        Button BetterButton(string _text)
        {
            var button = new Button();
            DatabrainHelpers.SetBorderRadius(button, 0, 0, 0, 0);
            button.style.height = 40;
            button.text = _text;
            return button;
        }

        void ShowWelcome()
        {
            rightContainer.Clear();

            var _logo = new VisualElement();
            _logo.style.backgroundImage = DatabrainHelpers.LoadLogoLarge();
            _logo.style.width = 218;
            _logo.style.height = 144;
            _logo.style.alignSelf = Align.Center;
            _logo.style.marginBottom = 20;
            _logo.style.marginTop = 20;
            _logo.style.marginLeft = 20;
            _logo.style.marginRight = 20;

            var _textThankyou = new Label();
            _textThankyou.style.unityFontStyleAndWeight = FontStyle.Bold;
            _textThankyou.text = "Thank you for choosing Databrain!";
            _textThankyou.style.unityTextAlign = TextAnchor.MiddleCenter;
            _textThankyou.style.marginBottom = 10;
            _textThankyou.style.fontSize = 16;

            var _text = new Label();
            _text.text = "If you like Databrain, please consider leaving a review. This helps the further development of Databrain considerably - Thank you!";
            _text.style.whiteSpace = WhiteSpace.Normal;
            _text.style.unityTextAlign = TextAnchor.MiddleCenter;
            _text.style.fontSize = 14;

            var _ggLogo = new VisualElement();
            _ggLogo.style.marginTop = 50;
            _ggLogo.style.backgroundImage = DatabrainHelpers.LoadGGLogo();
            _ggLogo.style.width = 160;
            _ggLogo.style.height = 70;
            _ggLogo.style.alignSelf = Align.Center;

            var _byText = new Label();
            _byText.text = "Developed by Giant Grey | <a href=https://www.giantgrey.com>www.giantgrey.com</a>";
            _byText.style.whiteSpace = WhiteSpace.Normal;
            _byText.style.unityTextAlign = TextAnchor.MiddleCenter;
            _byText.enableRichText = true;

            var _twitterText = new Label();
            _twitterText.text = "Follow @Twitter/X: <a href=https://twitter.com/MarcCEgli>MarcCEgli</a>";
            _twitterText.style.whiteSpace = WhiteSpace.Normal;
            _twitterText.style.unityTextAlign = TextAnchor.MiddleCenter;
            _twitterText.enableRichText = true;

            rightContainer.Add(_logo);
            rightContainer.Add(_textThankyou);
            rightContainer.Add(_text);
            rightContainer.Add(_ggLogo);
            rightContainer.Add(_byText);
            rightContainer.Add(_twitterText);
        }

        void ShowAddons()
        {
            rightContainer.Clear();

            var _label = new Label();
            DatabrainHelpers.SetTitle(_label, "Add-ons");

            var _row1 = new VisualElement();
            _row1.style.flexDirection = FlexDirection.Row;
            //_row1.style.flexGrow = 1;

            var _row2 = new VisualElement();
            _row2.style.flexDirection = FlexDirection.Row;
            //_row2.style.flexGrow = 1;

            _row1.Add(AddonCard("LOGIC", "logic_icon.png", "https://u3d.as/32gj", "https://giantgrey.gitbook.io/databrain/add-ons/logic", "https://databrain.cc/addons/logic"));
            _row1.Add(AddonCard("INVENTORY", "inventory_icon.png", "", "https://giantgrey.gitbook.io/databrain/add-ons/inventory", "https://databrain.cc/addons/inventory"));
            _row1.Add(AddonCard("TECHTREE", "techtree_icon.png", "https://u3d.as/32go", "https://giantgrey.gitbook.io/databrain/add-ons/techtree", "https://databrain.cc/addons/techtree"));
            _row2.Add(AddonCard("STATS", "stats_icon.png", "https://u3d.as/32gk", "https://giantgrey.gitbook.io/databrain/add-ons/stats", "https://databrain.cc/addons/stats"));
            _row2.Add(AddonCard("LOCALIZATION", "localization_icon.png", "https://u3d.as/32gm", "https://giantgrey.gitbook.io/databrain/add-ons/localization", "https://databrain.cc/addons/localization"));

            rightContainer.Add(_label);
            rightContainer.Add(_row1);
            rightContainer.Add(_row2);

        }

        void ShowSupport()
        {
            rightContainer.Clear();

            var _support = new Label();
            DatabrainHelpers.SetTitle(_support, "Support");

            var _row1 = new VisualElement();
            _row1.style.flexDirection = FlexDirection.Column;
            _row1.style.flexGrow = 1;


            //var _documentationButton = BetterButton("Documentation");
            //_documentationButton.RegisterCallback<ClickEvent>(click =>
            //{
            //    Application.OpenURL("");
            //});

            //var _discordButton = BetterButton("Discord");
            //_discordButton.RegisterCallback<ClickEvent>(click =>
            //{
            //    Application.OpenURL("");
            //});

            //var _email = BetterButton("Email");
            //_email.RegisterCallback<ClickEvent>(click =>
            //{
            //    Application.OpenURL("mailto:hello@giantgrey.com");
            //});


            _row1.Add(Card("Getting started", "", "video_icon.png", "https://www.youtube.com/watch?v=vcIX5j4mFdY"));
            _row1.Add(Card("Documentation", "", "documentation_icon.png", "https://giantgrey.gitbook.io/databrain"));
            _row1.Add(Card("Discord", "", "discord_icon.png", "https://discord.gg/a5uf3nM"));
            _row1.Add(Card("Email", "", "email_icon.png", "mailto:hello@giantgrey.com"));

            rightContainer.Add(_support);
            rightContainer.Add(_row1);
        }

        void ShowMore()
        {
            rightContainer.Clear();

            var _label = new Label();
            DatabrainHelpers.SetTitle(_label, "More by Giant Grey");

            var _row1 = new VisualElement();
            _row1.style.flexDirection = FlexDirection.Column;
            _row1.style.flexGrow = 1;

            _row1.Add(Card("", "marz_title.png", "marz_logo.png", "https://store.steampowered.com/app/682530/MarZ_Tactical_Base_Defense/"));
            // _row1.Add(Card("", "flowreactor_title.png", "flowreactor_logo.png", "https://assetstore.unity.com/packages/tools/visual-scripting/flowreactor-high-level-visual-scripting-167519"));
            _row1.Add(Card("", "tileworldcreator_title.png", "tileworldcreator_logo.png", "https://assetstore.unity.com/packages/tools/level-design/tileworldcreator-3-199383"));
            // _row1.Add(Card("", "databox_title.png", "databox_logo.png", "https://assetstore.unity.com/packages/tools/utilities/databox-data-editor-save-solution-155189"));
            _row1.Add(Card("", "pathGrid_title.png", "pathGrid_logo.png", "https://assetstore.unity.com/packages/tools/level-design/pathgrid-277374"));

            rightContainer.Add(_label);
            rightContainer.Add(_row1);
        }

        void ShowChangelog()
        {
            rightContainer.Clear();

            var _dropdown = new DropdownField();
            _dropdown.choices = new List<string>()
            {
                
                "Databrain",

                #if DATABRAIN_LOGIC
                "Logic", 
                #endif
                #if DATABRAIN_INVENTORY
                "Inventory", 
                #endif
                #if DATABRAIN_TECHTREE
                "Techtree", 
                #endif
                #if DATABRAIN_STATS
                "Stats", 
                #endif
                #if DATABRAIN_LOCALIZATION
                "Localization", 
                #endif
            };

            _dropdown.value = "Databrain";
            _dropdown.style.marginBottom = 10;
            _dropdown.RegisterValueChangedCallback(evt => 
            {
                if (evt.newValue != evt.previousValue)
                {
                    selectedChangelog = evt.newValue;
                    BuildChangelog();
                }
            });

            if (changelogContainer == null)
            {
                changelogContainer = new VisualElement();
            }

            rightContainer.Add(_dropdown);
            rightContainer.Add(changelogContainer);

            BuildChangelog();
        }

        void BuildChangelog()
        {
            changelogContainer.Clear();

            var _scrollView = new ScrollView();

            var _changelogFileName = "DatabrainRoot.cs";
            switch (selectedChangelog)
            {
                case "Databrain":
                _changelogFileName = "DatabrainRoot.cs";
                break;
                case "Logic":
                _changelogFileName = "LogicRoot.cs";
                break;
                case "Inventory":
                _changelogFileName = "InventoryRoot.cs";
                break;
                case "Techtree":
                _changelogFileName = "TechtreeRoot.cs";
                break;
                case "Stats":
                _changelogFileName = "StatsRoot.cs";
                break;
                case "Localization":
                _changelogFileName = "LocalizationRoot.cs";
                break;
            }
           
            var _label = new Label();
            _label.style.backgroundColor = DatabrainColor.Grey.GetColor();
            _label.style.marginBottom = 2;
            var _changelog = DatabrainHelpers.LoadChangelog(_changelogFileName);
            _label.text = _changelog;
            _label.style.whiteSpace = WhiteSpace.Normal;
            _scrollView.Add(_label);

            changelogContainer.Add(_scrollView);
        }


        VisualElement Card(string _title, string _bgImage, string _logoImage, string _URL)
        {
            var _card = new VisualElement();
            _card.style.flexDirection = FlexDirection.Row;
            _card.style.alignItems = Align.Center;
            _card.style.justifyContent = Justify.FlexEnd;
            _card.style.flexGrow = 1;
            _card.style.height = 110;
            _card.style.maxHeight = 110;
            DatabrainHelpers.SetBorder(_card, 2, DatabrainColor.LightGrey.GetColor());

            _card.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();


            _card.style.backgroundImage = DatabrainHelpers.LoadResourceTexture(_bgImage);
            var _bs = new BackgroundSize();
            _bs.sizeType = BackgroundSizeType.Cover;
            _card.style.backgroundSize = _bs;

            var _logo = new VisualElement();
            var _iconTex = DatabrainHelpers.LoadResourceTexture(_logoImage);

            _logo.style.backgroundImage = _iconTex;
            _logo.style.width = _iconTex.width * 0.7f;
            _logo.style.height = _iconTex.height * 0.7f;
            _logo.style.alignContent = Align.Center;
            _logo.style.marginTop = 10;

            var _space = new VisualElement();
            _space.style.flexGrow = 1;


            _card.Add(_logo);
            _card.Add(_space);


            _card.RegisterCallback<MouseEnterEvent>(x =>
            {
                DatabrainHelpers.SetBorder(_card, 2, Color.white);
            });

            _card.RegisterCallback<MouseLeaveEvent>(x =>
            {
                DatabrainHelpers.SetBorder(_card, 2, DatabrainColor.LightGrey.GetColor());
            });

            _card.RegisterCallback<ClickEvent>(x =>
            {
                Application.OpenURL(_URL);
            });

            DatabrainHelpers.SetBorderRadius(_card, 10, 10, 10, 10);
            DatabrainHelpers.SetMargin(_card, 5, 5, 5, 5);
            DatabrainHelpers.SetPadding(_card, 10, 10, 10, 10);


            var _titleLabel = new Label();
            _titleLabel.text = _title;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.fontSize = 14;
            _titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            _card.Add(_titleLabel);

            return _card;
        }

        VisualElement AddonCard(string _title, string _image, string _downloadURL, string _documentationURL, string _websiteURL)
        {
            var _card = new VisualElement();
            _card.style.flexDirection = FlexDirection.Column;
            _card.style.alignItems = Align.Center;
            _card.style.justifyContent = Justify.FlexEnd;

            _card.style.height = 200;
            _card.style.maxHeight = 200;
            _card.style.width = 150;
            _card.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();

            DatabrainHelpers.SetBorder(_card, 2, DatabrainColor.DarkGrey.GetColor());
            DatabrainHelpers.SetBorderRadius(_card, 10, 10, 10, 10);
            DatabrainHelpers.SetMargin(_card, 5, 5, 5, 5);
            DatabrainHelpers.SetPadding(_card, 10, 10, 10, 10);


            var _icon = new VisualElement();
            var _iconTex = DatabrainHelpers.LoadResourceTexture(_image);

            _icon.style.backgroundImage = _iconTex;
            _icon.style.width = _iconTex.width * 0.7f;
            _icon.style.height = _iconTex.height * 0.7f;
            _icon.style.alignContent = Align.Center;

            var _space = new VisualElement();
            _space.style.flexGrow = 1;

            var _titleLabel = new Label();
            _titleLabel.text = _title;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.fontSize = 14;
            _titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;



            _card.Add(_icon);
            _card.Add(_space);
            _card.Add(_titleLabel);


            var _overlay = new VisualElement();
            _overlay.style.position = Position.Absolute;
            _overlay.style.flexGrow = 1;
            _overlay.style.flexDirection = FlexDirection.Column;
            _overlay.style.backgroundColor = new Color(120f / 255f, 120f / 255f, 120f / 255f, 120f / 255f);
            _overlay.visible = false;
            _overlay.name = "overlay";
            _overlay.style.justifyContent = new StyleEnum<Justify>(Justify.FlexEnd);
            _overlay.style.alignItems = Align.Center;
            _overlay.style.alignContent = Align.Center;
            DatabrainHelpers.SetPadding(_overlay, 20, 20, 20, 20);

            if (!string.IsNullOrEmpty(_downloadURL))
            {
                var _downloadButton = new Button();
                _downloadButton.style.height = 30;
                _downloadButton.style.width = 120;
                _downloadButton.text = "Download";
                _downloadButton.RegisterCallback<ClickEvent>(click =>
                {
                    Application.OpenURL(_downloadURL);
                });

                _overlay.Add(_downloadButton);
            }

            var _documentationButton = new Button();
            _documentationButton.style.height = 30;
            _documentationButton.style.width = 120;
            _documentationButton.text = "Documentation";
            _documentationButton.RegisterCallback<ClickEvent>(click =>
            {
                Application.OpenURL(_documentationURL);
            });

            var _websiteButton = new Button();
            _websiteButton.style.height = 30;
            _websiteButton.style.width = 120;
            _websiteButton.text = "Website";
            _websiteButton.RegisterCallback<ClickEvent>(click =>
            {
                Application.OpenURL(_websiteURL);
            });

            
            _overlay.Add(_documentationButton);
            _overlay.Add(_websiteButton);

            _card.Add(_overlay);
            _overlay.StretchToParentSize();

            _card.RegisterCallback<MouseEnterEvent>(x =>
            {
                _overlay.visible = true;
                _titleLabel.visible = false;

                DatabrainHelpers.SetBorder(_card, 2, Color.white);
            });

            _card.RegisterCallback<MouseLeaveEvent>(x =>
            {
                _overlay.visible = false;
                _titleLabel.visible = true;
                DatabrainHelpers.SetBorder(_card, 0, Color.white);
            });





            return _card;
        }
    }
}
#endif