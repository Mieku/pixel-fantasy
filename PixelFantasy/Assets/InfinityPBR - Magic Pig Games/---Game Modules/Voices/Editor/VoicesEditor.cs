using System;
using InfinityPBR.Modules;
using UnityEditor;

namespace InfinityPBR
{
    [CustomEditor(typeof(Voices))]
    [CanEditMultipleObjects]
    [Serializable]
    public class VoicesEditor : Editor
    {
        private Voices _modulesObject;
        private VoicesDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (Voices)target;
            _modulesDrawer = CreateInstance<VoicesDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();

        /*
        private Voices _modulesObject;
        
        string[] menuBarOptions = 
        {
            "Voices",
            "Lines",
            "Emotions"
        };
        
        protected override void Header()
        {
            
        }

        protected override void Setup()
        {
            _modulesObject = (Voices) target;
            Cache();
        }
        
        protected override void Draw()
        {
            CheckNewClips();
            SetupVoices();
            
            // When this is first created, Unity will need to reimport scripts, which takes a few seconds. This keeps
            // errors from forming, and will show a message until that is complete.
            if (_modulesObject.voices == null)
            {
                Label("Please wait, scripts should re-import shortly...");
                return;
            }

            // Display the main buttons for the panels, and then display the panel that is selected, if any.
            ShowButtons();
            // Show the auto fill options for auto populating everything magically
            ShowAutoFill();
            
            if (_modulesObject.menubarIndex == 0)
                ShowVoices(_modulesObject);
            if (_modulesObject.menubarIndex == 1)
                ShowLines(_modulesObject);
            if (_modulesObject.menubarIndex == 2)
                ShowEmotions(_modulesObject);

            if (ShowFullInspector(name)) DrawDefaultInspector();
            
            SetDirty();
        }
        
        
        private void ShowButtons() => _modulesObject.menubarIndex =  ToolbarMenuMain(menuBarOptions, _modulesObject.menubarIndex);

        private void ShowAutoFill()
        {
            if (!_modulesObject.showAutoFill) return;

            SetBool("Show Auto Fill", 
                StartFoldoutHeaderGroup(GetBool("Show Auto Fill"),
                    GetBool("Show Auto Fill")
                    ? "Hide Auto Fill" 
                    : "Show Auto Fill"));

            if (GetBool("Show Auto Fill"))
            {
                MessageBox(
                    "You can auto populate the content of this system using a naming convention" +
                    " on your AudioClips:\n\n" +
                    "[Voice Name]_[Line Name]_[Emotion Name]_[#].wav\n\n" +
                    "The 3rd and 4th section are optional.\n\n" +
                    "Voice Name: This is the name of the \"Voice\", which is often a character name.\n" +
                    "Line Name: This is the \"Line\" that is being spoken, a common line, even if the " +
                    "contents of the line are different, that all Voices may be able to speak in your" +
                    " project.\n" +
                    "(Optional) Emotion Name: If you are using multiple emotion for a line, add the " +
                    "name of the emotion here, and it will be populated properly.\n" +
                    "(Optional) #: If you have multiple clips for the same Voice/Line/Emotion combination " +
                    "add a number or other unique string here, to keep them all named something unique.", MessageType.Info);
                DisplayAutoFillBox();
                Space();
            }
            
            EndFoldoutHeaderGroup();
        }

        private void CheckNewClips()
        {
            for (int i = 0; i < _modulesObject.voices.Count; i++)
            {
                Voice voice = _modulesObject.voices[i];
                foreach (Line line in voice.lines)
                {
                    if (line.newAudioClip)
                    {
                        //Undo.RecordObject(Script, "Add New Clip");
                        line.audioClip.Add(line.newAudioClip);
                        line.newAudioClip = null;
                    }

                    foreach (Emotion emotion in line.emotions)
                    {
                        if (emotion.newAudioClip)
                        {
                            //Undo.RecordObject(Script, "Add New Clip");
                            emotion.audioClip.Add(emotion.newAudioClip);
                            emotion.newAudioClip = null;
                        }
                    }
                }
            }
        }

        private void AddAudioClipToAllLines(string lineName, AudioClip newAudioClip, bool avoidDuplicates = true)
        {
            for (int i = 0; i < _modulesObject.voices.Count; i++)
            {
                Voice voice = _modulesObject.voices[i];
                foreach (Line line in voice.lines)
                {
                    if (line.line != lineName) continue;
                    if (avoidDuplicates && line.HasAudioClip(newAudioClip)) continue;
                    line.audioClip.Add(newAudioClip);
                }
            }
        }
        
        private void AddAudioClipToAllEmotions(string emotionName, AudioClip newAudioClip, bool avoidDuplicates = true)
        {
            for (int i = 0; i < _modulesObject.voices.Count; i++)
            {
                Voice voice = _modulesObject.voices[i];
                foreach (Line line in voice.lines)
                {
                    foreach (Emotion emotion in line.emotions)
                    {
                        if (emotion.emotion != emotionName) continue;
                        if (avoidDuplicates && emotion.HasAudioClip(newAudioClip)) continue;
                        emotion.audioClip.Add(newAudioClip);
                    }
                }
            }
        }

        private void SetupVoices()
        {
            if (!HasKey(name + " " + menuBarOptions[0])) 
                SetBool(name + " " + menuBarOptions[0], true);

            if (_modulesObject.types.Count == 0)
                _modulesObject.types.Add("Voice");
            
            if (!HasKey("Voice New Type"))
                SetString("Voice New Type", "New Type");
        }

        private void ShowPanelButtons()
        {
            StartRow();
            for (int i = 0; i < menuBarOptions.Length; i++)
            {
                BackgroundColor(GetBool(name + " " + menuBarOptions[i]) ? active : dark);
                if (Button(menuBarOptions[i]))
                {
                    for (int b = 0; b < menuBarOptions.Length; b++)
                    {
                        if (b == i)
                            SetBool(name + " " + menuBarOptions[b], !GetBool(name + " " + menuBarOptions[b]));
                        else
                            SetBool(name + " " + menuBarOptions[b], false);
                    }
                }
            }
            ResetColor();
            EndRow();
            Space();
        }

        public void ShowVoices(Voices voices)
        {
            MessageBox(
                "These are the individual character voices who may speak during your game. Try giving them " +
                "names, or use a naming convention that is logical based on your project. You can even use this system " +
                "to manage non-voice AudioClips.", MessageType.Info);
            StartRow();
            DisplayNewVoiceBox();
            DisplayNewTypeBox();
            EndRow();
            Space();
            DisplayVoices();
            Space();
            DisplayTypes();
        }

        public void DisplayTypes()
        {
            SetBool("Voices Show Types", EditorGUILayout.Foldout(GetBool("Voices Show Types"), "Manage Types"));
            if (!GetBool("Voices Show Types")) return;

            for (int i = 0; i < _modulesObject.types.Count; i++)
            {
                StartRow();
                string tempType = DelayedText(_modulesObject.types[i], 200);

                if (!String.IsNullOrWhiteSpace(tempType) && tempType != _modulesObject.types[i])
                {
                    foreach (Voice voice in _modulesObject.voices)
                    {
                        if (voice.type != _modulesObject.types[i]) continue;
                        voice.type = tempType;
                    }
                    _modulesObject.types[i] = tempType;
                }

                if (_modulesObject.types.Count > 1)
                {
                    if (XButton())
                    {
                        Undo.RecordObject(Script, "Delete Type");
                        foreach (Voice voice in _modulesObject.voices)
                        {
                            string newType = _modulesObject.types[0];
                            if (i == 0) newType = _modulesObject.types[1];

                            if (voice.type == _modulesObject.types[i])
                                voice.type = newType;
                        }

                        _modulesObject.types.RemoveAt(i);
                        ExitGUI();
                    }
                }
                
                EndRow();
            }
        }

        public void ShowLines(Voices voices)
        {
            MessageBox(
                "Each line is a potential thing your characters can say. Not all voices need to be able to say " +
                "all lines, but you'll have the option to populate AudioClips for each line you specify, for each voice " +
                "you've created.", MessageType.Info);
            DisplayNewLineBox();
            Space();
            DisplayLines();
        }
        
        public void ShowEmotions(Voices voices)
        {
            MessageBox(
                "For each line there is a default AudioClip. Here you can identify various emotions for each " +
                "line. In your code, you can call the default AudioClip for the line, or one of the emotions instead, " +
                "depending on the logic of your game.", MessageType.Info);
            DisplayNewEmotionBox();
            Space();
            DisplayEmotions();
        }

        private void DisplayLines()
        {
            for (int i = 0; i < _modulesObject.lineNames.Count; i++)
            {
                StartRow();
                string tempNameLine = _modulesObject.lineNames[i];
                _modulesObject.lineNames[i] = StrippedString(DelayedText(_modulesObject.lineNames[i]));
                if (TryToChangeFileNames(tempNameLine, _modulesObject.lineNames[i], 1))
                    UpdateLineNames(_modulesObject.lineNames[i], i);
                else
                    _modulesObject.lineNames[i] = tempNameLine;
                if (XButton())
                {
                    Undo.RecordObject(Script, "Delete Line");
                    _modulesObject.lineNames.RemoveAt(i);
                    foreach (Voice voice in _modulesObject.voices)
                    {
                        voice.lines.RemoveAt(i);
                    }
                    ExitGUI();
                }
                EndRow();
            }
        }
        
        private void DisplayEmotions()
        {
            for (int i = 0; i < _modulesObject.emotionNames.Count; i++)
            {
                StartRow();
                var tempNameEmotion = _modulesObject.emotionNames[i];
                _modulesObject.emotionNames[i] = StrippedString(DelayedText(_modulesObject.emotionNames[i]));
                if (TryToChangeFileNames(tempNameEmotion, _modulesObject.emotionNames[i], 2))
                    UpdateEmotionNames(_modulesObject.emotionNames[i], i);
                else
                    _modulesObject.emotionNames[i] = tempNameEmotion;
                if (XButton())
                {
                    Undo.RecordObject(Script, "Delete Emotion");
                    _modulesObject.emotionNames.RemoveAt(i);
                    foreach (Voice voice in _modulesObject.voices)
                    {
                        foreach (Line line in voice.lines)
                        {
                            line.emotions.RemoveAt(i);
                        }
                    }
                    ExitGUI();
                }
                EndRow();
            }
        }

        private bool IsInt(string test)
        {
            int testInt;
            if (int.TryParse(test, out testInt))
                return true;

            return false;
        }

        private string StrippedString(string stringToStrip)
        {
            stringToStrip = stringToStrip.Replace(" ", "");
            return Regex.Replace(stringToStrip, @"[^A-Za-z0-9]+", "");
        }
        
        private bool TryToChangeFileNames(string oldName, string newName, int segment)
        {
            if (String.IsNullOrWhiteSpace(newName)) return false; // Make sure the newName isn't empty
            if (oldName == newName) return false; // If the names don't match, return

            // Check to make sure the newName isn't just a number. It needs to be a string.
            if (IsInt(newName))
            {
                Debug.LogError("Error: You can't change a name segment into a number.");
                return false;
            }

            
            // Make a List<AudioCLip> with all the clips assigned through the Voices module.
            List<AudioClip> audioClips = new List<AudioClip>();
            foreach (Voice voice in _modulesObject.voices)
            {
                foreach (Line line in voice.lines)
                {
                    foreach (AudioClip clip in line.audioClip)
                        audioClips.Add(clip); // Add a clip

                    foreach (Emotion emotion in line.emotions)
                    {
                        foreach (AudioClip clip in emotion.audioClip)
                            audioClips.Add(clip); // Add a clip
                    }
                }
            }

            // Work on each clip, if the clip can be worked on...
            foreach (AudioClip clip in audioClips)
            {
                // Split the name into the array nameSegments
                string[] nameSegments = clip.name.Split(char.Parse("_"));
                
                // Make sure we have enough segments -- or don't do this clip at all, skip it
                if (nameSegments.Length <= segment) 
                    continue;
                
                // Make sure the string we are replacing is not a number
                if (IsInt(nameSegments[segment]))
                    continue;
                
                // Continue if the segment name does not match oldName
                if (nameSegments[segment] != oldName)
                    continue;

                // Rebuild the fullName with the newName replacing the selected segment
                nameSegments[segment] = newName;
                string fullName = "";
                for (int i = 0; i < nameSegments.Length; i++)
                {
                    if (i > 0)
                        fullName = fullName + "_";

                    fullName = fullName + nameSegments[i];
                }
                
                // Rename the file
                var path = AssetDatabase.GetAssetPath(clip);
                AssetDatabase.RenameAsset(path, fullName);
            }

            return true;
        }

        private void UpdateLineNames(string newName, int i)
        {
            foreach (Voice voice in _modulesObject.voices)
            {
                voice.lines[i].line = newName;
            }
        }
        
        private void UpdateEmotionNames(string newName, int i)
        {
            foreach (var line in _modulesObject.voices
                         .SelectMany(voice => voice.lines
                             .Where(line => line.emotions.Count < i)))
            {
                line.emotions[i].emotion = newName;
            }
        }

        private void DisplayVoices()
        {
            for (int i = 0; i < _modulesObject.voices.Count; i++)
            {
                var voice = _modulesObject.voices[i];
                StartRow();
                
                // Expand button
                ButtonOpenClose($"Show Voice {i}");

                // Name of voice
                string tempNameVoice = voice.voice;
                voice.voice = StrippedString(DelayedText(voice.voice, 300));
                if (TryToChangeFileNames(tempNameVoice, voice.voice, 0))
                    UpdateEmotionNames(voice.voice, i);
                else
                    voice.voice = tempNameVoice;
                
                // Types
                Undo.RecordObject(Script, "Type type");
                voice.type = _modulesObject.types[Popup(_modulesObject.GetTypeIndex(voice.type), _modulesObject.types.ToArray(), 200)];
                
                EndRow();
                if (!GetBool("Show Voice " + i))
                    continue;

                foreach (Line line in voice.lines)
                {
                    StartRow();
                    StartVertical();
                    ContentColor(line.audioClip.Count == 0 ? Color.grey : Color.white);
                    Label(line.line + " - Default", 200);
                    ResetColor();
                    EndVertical();
                    StartVertical();
                    for (var l = 0; l < line.audioClip.Count; l++)
                    {
                        StartRow();
                        line.audioClip[l] = Object(line.audioClip[l], typeof(AudioClip), 150) as AudioClip;
                        if (Button("Play", 70))
                        {
                            PlayClip(line.audioClip[l], 0, false);
                        }

                        if (Button("Copy > Voices", "Copy this AudioClip to all " + line.line + " - Default lists on all other voices.", 100))
                        {
                            Undo.RecordObject(Script, "Copy To All");
                            AddAudioClipToAllLines(line.line, line.audioClip[l]);
                        }
                        if (XButton())
                        {
                            Undo.RecordObject(Script, "Delete Clip");
                            line.audioClip.RemoveAt(l);
                            ExitGUI();
                        }
                        EndRow();
                    }
                    StartRow();
                    BackgroundColor(Color.grey);
                    ContentColor(Color.grey);
                    line.newAudioClip = Object(line.newAudioClip, typeof(AudioClip), 150) as AudioClip;
                    Label("Add new AudioClip here");
                    ResetColor();
                    EndRow();
                    EndVertical();
                    EndRow();
                    
                    foreach (Emotion emotion in line.emotions)
                    {
                        StartRow();
                        StartVertical();
                        ContentColor(emotion.audioClip.Count == 0 ? Color.grey : Color.white);
                        Label(line.line + " - " + emotion.emotion, 200);
                        EndVertical();
                        StartVertical();
                        for (int e = 0; e < emotion.audioClip.Count; e++)
                        {
                            StartRow();
                            emotion.audioClip[e] = Object(emotion.audioClip[e], typeof(AudioClip), 150) as AudioClip;
                            if (Button("Play", 70))
                            {
                                PlayClip(emotion.audioClip[e], 0, false);
                            }
                            if (Button("Copy > Voices", "Copy this AudioClip to all " + line.line + " - " + emotion.emotion + " lists on all other voices.", 100))
                            {
                                Undo.RecordObject(Script, "Copy To All");
                                AddAudioClipToAllEmotions(emotion.emotion, emotion.audioClip[e]);
                            }
                            if (XButton())
                            {
                                Undo.RecordObject(Script, "Delete Clip");
                                emotion.audioClip.RemoveAt(e);
                                ExitGUI();
                            }
                            EndRow();
                        }
                        StartRow();
                        BackgroundColor(Color.grey);
                        ContentColor(Color.grey);
                        emotion.newAudioClip = Object(emotion.newAudioClip, typeof(AudioClip), 150) as AudioClip;
                        Label("Add new AudioClip here");
                        ResetColor();
                        EndRow();
                        EndVertical();
                        EndRow();
                    }
                    if (line.emotions.Count > 0) Space();
                }
                
                BackgroundColor(Color.red);
                if (Button("Delete " + voice.voice, 300))
                {
                    if (Dialog("Delete voice", "Do you really want to delete " + voice.voice + "?"))
                    {
                        Undo.RecordObject(Script, "Delete Voice");
                        _modulesObject.voices.RemoveAt(i);
                        ExitGUI();
                    }
                }
                ResetColor();

                Space();
            }
        }

        private void DisplayAutoFillBox()
        {
            Space();
            StartVerticalBox();
            Label(String.IsNullOrWhiteSpace(_modulesObject.autoFillDirectory) ? 
                "Select the directory holding your audio files" : 
                _modulesObject.autoFillDirectory, true);
            StartRow();

            
            
            Object tempObject = AssetDatabase.LoadAssetAtPath(_modulesObject.autoFillDirectory, typeof(Object));

            Object userObject = Object(tempObject, typeof(Object), 300);
            if (userObject && tempObject != userObject)
                tempObject = userObject;
            
            _modulesObject.autoFillDirectory = AssetDatabase.GetAssetPath(tempObject);

            EndRow();

            if (tempObject)
            {
                BackgroundColor(Color.green);
                if (Button("Auto Fill Contents"))
                {
                    Undo.RecordObject(Script, "Auto Fill Contents");
                    AutoFillContents();
                }
            }
            
            EndVertical();
            ResetColor();
        }

        /// <summary>
        /// This will auto fill all the contents into the structure from the selected folder
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void AutoFillContents()
        {
            var guidsToFiles = AssetDatabase.FindAssets("t:AudioClip",new[] {_modulesObject.autoFillDirectory});
            
            foreach (var guid in guidsToFiles)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
                string clipName = clip.name;
                string[] nameSegments = clipName.Split(char.Parse("_"));

                if (nameSegments.Length < 2)
                {
                    Debug.LogError("Error: There are only " + nameSegments.Length + " segements " +
                                   "in the name " + clipName + ". Please read the instructions on " +
                                   "how to properly name your AudioClip files for this module. " +
                                   "We will skip this file.");
                    continue;
                }
                
                // Add Voice, Line, Emotion
                if (!_modulesObject.HasVoice(nameSegments[0]))
                    _modulesObject.AddVoice(nameSegments[0]);
                
                if (!_modulesObject.HasLine(nameSegments[1]))
                    _modulesObject.AddLine(nameSegments[1]);

                var hasEmotion = false;
                if (nameSegments.Length >= 3)
                {
                    if (IsInt(nameSegments[2]))
                    {
                        Debug.LogWarning("Warning: The third segment was a number, so we did not " +
                                         "try to make it into an \"Emotion\". We will now skip the " +
                                         "file " + clipName);
                    }
                    else
                    {
                        hasEmotion = true;
                        if (!_modulesObject.HasEmotion(nameSegments[2]))
                            _modulesObject.AddEmotion(nameSegments[2]);
                    }
                }
                
                // Fill the AudioClip field
                var voice = _modulesObject.GetVoice(nameSegments[0]);
                var line = voice.GetLine(nameSegments[1]);
                if (!hasEmotion)
                {
                    if (!line.HasAudioClip(clip))
                        line.audioClip.Add(clip);
                }
                else
                {
                    var emotion = line.GetEmotion(nameSegments[2]);
                    if (!emotion.HasAudioClip(clip))
                        emotion.audioClip.Add(clip);
                }
            }
        }

        private void DisplayNewVoiceBox()
        {
            Space();
            BackgroundColor(mixed);
            StartVerticalBox();
            Label("Create new Voice", true);
            StartRow();
            newVoiceName = TextField(newVoiceName, 200);
            if (Button("Add", 100))
            {
                newVoiceName = newVoiceName.Trim();
                if (_modulesObject.HasVoice(newVoiceName))
                    Debug.Log("There is already a Voice named " + newVoiceName);
                else
                {
                    Undo.RecordObject(Script, "Add Voice");
                    _modulesObject.AddVoice(newVoiceName);
                }
            }

            EndRow();
            EndVertical();
            ResetColor();
        }
        
        private void DisplayNewTypeBox()
        {
            Space();
            BackgroundColor(mixed);
            StartVerticalBox();
            Label("Add new type", true);
            StartRow();
            SetString("Voice New Type", TextField(GetString("Voice New Type"), 200));
            if (Button("Add", 100))
            {
                if (_modulesObject.HasType(GetString("Voice New Type")))
                    Debug.Log("There is already a type named " + GetString("Voice New Type"));
                else
                {
                    Undo.RecordObject(Script, "Add Type");
                    _modulesObject.AddType(GetString("Voice New Type"));
                }
            }

            EndRow();
            EndVertical();
            ResetColor();
        }
        
        private void DisplayNewLineBox()
        {
            Space();
            BackgroundColor(mixed);
            StartVerticalBox();
            Label("Create new Line", true);
            StartRow();
            newLineName = TextField(newLineName, 200);
            if (Button("Add", 100))
            {
                newLineName = newLineName.Trim();
                if (_modulesObject.HasLine(newLineName))
                    Debug.Log("There is already a Line named " + newLineName);
                else
                {
                    Undo.RecordObject(Script, "Add Line");
                    _modulesObject.AddLine(newLineName);
                }
            }

            EndRow();
            EndVertical();
            ResetColor();
        }
        
        private void DisplayNewEmotionBox()
        {
            Space();
            BackgroundColor(mixed);
            StartVerticalBox();
            Label("Create new Emotion", true);
            StartRow();
            newEmotionName = TextField(newEmotionName, 200);
            if (Button("Add", 100))
            {
                newEmotionName = newEmotionName.Trim();
                if (_modulesObject.HasEmotion(newEmotionName))
                    Debug.Log("There is already an Emotion named " + newEmotionName);
                else
                {
                    Undo.RecordObject(Script, "Add Emotion");
                    _modulesObject.AddEmotion(newEmotionName);
                }
            }

            EndRow();
            EndVertical();
            ResetColor();
        }
        
        /*
         * https://forum.unity.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
         */
/*
        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
     
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );
 
            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { clip, startSample, loop }
            );
        }

        public static void StopAllClips()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
 
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { },
                null
            );
 
            Debug.Log(method);
            method.Invoke(
                null,
                new object[] { }
            );
        }








        private string newVoiceName = "New Voice Name";
        private string newLineName = "New Line";
        private string newEmotionName = "New Emotion";
        
        
        */
    }
}