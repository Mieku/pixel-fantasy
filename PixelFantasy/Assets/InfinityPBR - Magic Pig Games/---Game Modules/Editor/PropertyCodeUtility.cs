using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public static class PropertyCodeUtility
    {
        public static void ExportEnumCode()
        {
            
        }
        
        public static void CreatePropertyCode()
        {
            const string nameOfScript = "Properties";
            
            var foundScript = AssetDatabase
                .FindAssets("GameItemObjectList").First(x => AssetDatabase.GUIDToAssetPath(x).Split('/').Last() == "GameItemObjectList.cs");
            if (foundScript == null) return;

            var foundPropertyScript = "";
            var databaseResults = AssetDatabase.FindAssets(nameOfScript);
            
            AssetDatabase.Refresh();
            if (databaseResults.Length > 0)
            {
                foundPropertyScript = databaseResults.FirstOrDefault(x => AssetDatabase.GUIDToAssetPath(x).Contains($"/{nameOfScript}.cs")); 
            }
            
            // Path is to the standard location defined in GameModulesUtilities or to where the script is now.
            //var newScriptPath = string.IsNullOrWhiteSpace(foundPropertyScript)
                //? GameModulesGeneratedScriptPath
                //: AssetDatabase.GUIDToAssetPath(foundPropertyScript);
            var newScriptPath = string.IsNullOrWhiteSpace(foundPropertyScript) 
                ? Path.Combine(GameModulesGeneratedScriptPath, $"{nameOfScript}.cs") 
                : AssetDatabase.GUIDToAssetPath(foundPropertyScript);
            
            var folderPath = Path.GetDirectoryName(newScriptPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            var scriptContent = BuildScriptContent();

            scriptContent = scriptContent.Replace("[[", "{");
            scriptContent = scriptContent.Replace("]]", "}");

            DebugConsoleMessage($"Property Script Path {newScriptPath}");

            File.WriteAllText(newScriptPath, scriptContent);
            
            AssetDatabase.Refresh();
            DebugConsoleMessage("Created Properties.cs");
        }
        
        private static string PropertyCodeAllTypes()
        {
            string content = "public static TypesQuest quests = new TypesQuest();"; 
            content = $"{content}\npublic static TypesItemObjects itemObjects = new TypesItemObjects();";
            content = $"{content}\npublic static TypesItemAttributes itemAttributes = new TypesItemAttributes();";
            content = $"{content}\npublic static TypesStats stats = new TypesStats();";
            content = $"{content}\npublic static TypesConditions conditions = new TypesConditions();";
            return content;
        }

        private static string BuildScriptContent()
        {

            string content = "";
            content = $"{content}\nnamespace InfinityPBR.Modules\n";
            content = $"{content}[[\n\n";

            content = $"{content}    // This code was automatically generated with Game Modules Bundle by Infinity PBR.\n";
            content = $"{content}    // Visit the documentation site linked from www.InfinityPBR.com for more details\n";
            content = $"{content}    // instructions, and videos.\n\n";
                
            content = $"{content}    [System.Serializable]\n";
            content = $"{content}    public class Properties\n";
            content = $"{content}    [[\n";
        
            content = $"{content}    public class Types\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeAllTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class TypesQuest\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeQuestTypes()}\n";
            content = $"{content}    ]]\n";
        
            
            content = $"{content}    public class TypesItemObjects\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeItemObjectTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class TypesItemAttributes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeItemAttributeTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class TypesStats\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeStatTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class TypesConditions\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeConditionTypes()}\n";
            content = $"{content}    ]]\n";

            content = $"{content}    public class ItemObjectTypes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeItemObjectTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class ItemObjects\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeItemObjects()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class ItemAttributeTypes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeItemAttributeTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class ItemAttributes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeItemAttributes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class ConditionTypes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeConditionTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class Conditions\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeConditions()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class StatTypes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeStatTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class Stats\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeStats()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class QuestTypes\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeQuestTypes()}\n";
            content = $"{content}    ]]\n";
            
            content = $"{content}    public class Quests\n";
            content = $"{content}    [[\n";
            content = $"{content}        {PropertyCodeQuests()}\n";
            content = $"{content}    ]]\n";

            content = $"{content}    ]]\n";
            content = $"{content}]]\n";

            return content;
        }
        
        private static string PropertyCodeQuestTypes() => AllTypeCodeString(Utilities.QuestArray().Select(x => x.objectType).Distinct().ToList());
        
        private static string PropertyCodeQuests()
        {
            string codeString = "";

            foreach (var type in Utilities.QuestArray().Select(x => x.objectType).Distinct())
            {
                var category = "Quests";
                var key = $"{(type == category ? $"{type}Type" : type)}";
                key = key.Replace(" ", "");
                key = key.Replace("-", "");
                
                codeString = $"{codeString}public static {key}{category} {key} = new {key}{category}();\n\n";
                
                codeString = $"{codeString}public class {key}{category}\n[[\n";

                foreach (var quest in Utilities.QuestArray().Where(x => x.objectType == type))
                {
                    var obj = quest.objectName;
                    obj = obj.Replace(" ", "");
                    obj = obj.Replace("-", "");
                    
                    codeString = $"{codeString}public string {obj}Uid => \"{quest.Uid()}\";\n";
                    codeString = $"{codeString}public Quest {obj} => GameModuleRepository.Instance.Get<Quest>({obj}Uid);\n";
                }
                
                codeString = $"{codeString}]]\n\n";
            }

            return codeString;
        }

        private static string PropertyCodeStatTypes() => AllTypeCodeString(Utilities.StatArray().Select(x => x.objectType).Distinct().ToList());
        
        private static string PropertyCodeStats()
        {
            string codeString = "";

            foreach (var type in Utilities.StatArray().Select(x => x.objectType).Distinct())
            {
                var category = "Stats";
                var key = $"{(type == category ? $"{type}Type" : type)}";
                key = key.Replace(" ", "");
                key = key.Replace("-", "");
                
                codeString = $"{codeString}public static {key}{category} {key} = new {key}{category}();\n\n";
                
                codeString = $"{codeString}public class {key}{category}\n[[\n";

                foreach (var stat in Utilities.StatArray().Where(x => x.objectType == type))
                {
                    var obj = stat.objectName;
                    obj = obj.Replace(" ", "");
                    obj = obj.Replace("-", "");
                    
                    codeString = $"{codeString}public string {obj}Uid => \"{stat.Uid()}\";\n";
                    codeString = $"{codeString}public Stat {obj} => GameModuleRepository.Instance.Get<Stat>({obj}Uid);\n";
                }
                
                codeString = $"{codeString}]]\n\n";
            }

            return codeString;
        }
        
        private static string PropertyCodeConditionTypes() => AllTypeCodeString(Utilities.ConditionArray().Select(x => x.objectType).Distinct().ToList());
        
        private static string PropertyCodeConditions()
        {
            string codeString = "";

            foreach (var type in Utilities.ConditionArray().Select(x => x.objectType).Distinct())
            {
                var category = "Conditions";
                var key = $"{(type == category ? $"{type}Type" : type)}";
                key = key.Replace(" ", "");
                key = key.Replace("-", "");
                
                codeString = $"{codeString}public static {key}{category} {key} = new {key}{category}();\n\n";
                
                codeString = $"{codeString}public class {key}{category}\n[[\n";

                foreach (var condition in Utilities.ConditionArray().Where(x => x.objectType == type))
                {
                    var obj = condition.objectName;
                    obj = obj.Replace(" ", "");
                    obj = obj.Replace("-", "");
                    
                    codeString = $"{codeString}public string {obj}Uid => \"{condition.Uid()}\";\n";
                    codeString = $"{codeString}public Condition {obj} => GameModuleRepository.Instance.Get<Condition>({obj}Uid);\n";
                }
                
                codeString = $"{codeString}]]\n\n";
            }

            return codeString;
        }
        
        private static string PropertyCodeItemAttributeTypes() => AllTypeCodeString(Utilities.ItemAttributeArray().Select(x => x.objectType).Distinct().ToList());
        
        private static string PropertyCodeItemAttributes()
        {
            string codeString = "";

            foreach (var type in Utilities.ItemAttributeArray().Select(x => x.objectType).Distinct())
            {
                var category = "Attributes";
                var key = $"{(type == category ? $"{type}Type" : type)}";
                key = key.Replace(" ", "");
                key = key.Replace("-", "");
                
                codeString = $"{codeString}public static {key}{category} {key} = new {key}{category}();\n\n";
                
                codeString = $"{codeString}public class {key}{category}\n[[\n";

                foreach (var itemAttribute in Utilities.ItemAttributeArray().Where(x => x.objectType == type))
                {
                    var obj = itemAttribute.objectName;
                    obj = obj.Replace(" ", "");
                    obj = obj.Replace("-", "");
                    
                    codeString = $"{codeString}public string {obj}Uid => \"{itemAttribute.Uid()}\";\n";
                    codeString = $"{codeString}public ItemAttribute {obj} => GameModuleRepository.Instance.Get<ItemAttribute>({obj}Uid);\n";
                }
                
                codeString = $"{codeString}]]\n\n";
            }

            return codeString;
        }
        
        private static string PropertyCodeItemObjectTypes() => AllTypeCodeString(Utilities.ItemObjectArray().Select(x => x.objectType).Distinct().ToList());

        private static string AllTypeCodeString(List<string> strings)
        {
            string codeString = "";
            foreach (var type in strings)
                codeString = $"{codeString}{TypeCodeString(type)}";
            return codeString;
        }

        private static string TypeCodeString(string type)
        {
            var typeFixed = type;
            typeFixed = typeFixed.Replace(" ", "");
            typeFixed = typeFixed.Replace("-", "");
                
            return $"public string {typeFixed} => \"{type}\";\n";
        }
        
        private static string PropertyCodeItemObjects()
        {
            string codeString = "";

            foreach (var type in Utilities.ItemObjectArray().Select(x => x.objectType).Distinct())
            {
                var category = "Objects";
                var key = $"{(type == category ? $"{type}Type" : type)}";
                key = key.Replace(" ", "");
                key = key.Replace("-", "");
                
                codeString = $"{codeString}public static {key}{category} {key} = new {key}{category}();\n\n";
                
                codeString = $"{codeString}public class {key}{category}\n[[\n";

                foreach (var itemObject in Utilities.GetItemObjectArray().Where(x => x.objectType == type))
                {
                    var obj = itemObject.objectName;
                    obj = obj.Replace(" ", "");
                    obj = obj.Replace("-", "");
                    
                    codeString = $"{codeString}public string {obj}Uid => \"{itemObject.Uid()}\";\n";
                    codeString = $"{codeString}public ItemObject {obj} => GameModuleRepository.Instance.Get<ItemObject>({obj}Uid);\n";
                }
                
                codeString = $"{codeString}]]\n\n";
            }

            return codeString;
        }
    }
}

