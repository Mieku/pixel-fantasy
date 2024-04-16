/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0162
namespace Databrain
{
    [CreateAssetMenu(menuName = "Databrain / Hierarchy Template")]
    public class DatabrainHierarchyTemplate : ScriptableObject, ISerializationCallbackReceiver
    {

        [System.Serializable]
        public class SerializedGroup
        {
            public bool foldout;
            public string typeDisplayName;
            public string typeName;
            public string typeAssemblyQualifiedName;    
            public bool isFirstClassType;
           
            public int childCount;
            public int indexOfFirstChild;

            public SerializedGroup() { }
        }

        public List<SerializedGroup> serializedGroup;

        public DatabrainTypes rootDatabrainTypes = new DatabrainTypes();

        public class DatabrainTypes
        {   

            public bool foldout;
            public string name;  
            public string type;
            [Hide]
            public string assemblyQualifiedTypeName;
            public bool isFirstClassType;

            // public Texture2D namespaceIcon;

            public List<DatabrainTypes> subTypes;


            public List<DatabrainTypes> CollectTypes()
            {
                var list = new List<DatabrainTypes>();

                for (int i = 0; i < subTypes.Count; i++)
                {
                    var ls = subTypes[i].CollectTypes();
                    list.AddRange(ls);
                }

                list.AddRange(subTypes);

                return list;
            }

            public bool HasType(DataObject _object)
            {
                if (string.Equals(_object.GetType().Name, type))
                {
                    return true;
                }
                else
                {
                    for (int i = 0; i < subTypes.Count; i++)
                    {
                        return subTypes[i].HasType(_object);
                    }
                }

                return false;
            }

            public DatabrainTypes()
            {
                subTypes = new List<DatabrainTypes>();
                name = "";
                type = "";
                assemblyQualifiedTypeName = "";
            }
        }



        public void OnBeforeSerialize()
        {
            if (serializedGroup == null) serializedGroup = new List<SerializedGroup>();
            if (rootDatabrainTypes == null)
            {
                rootDatabrainTypes = new DatabrainTypes();
            }

            serializedGroup.Clear();
            AddToSerializedGroup(rootDatabrainTypes);
        }

        public void OnAfterDeserialize()
        {
            if (serializedGroup.Count > 0)
            {
                ReadNodeFromSerializedNodes(0, out rootDatabrainTypes);
            }
            else
            {
                rootDatabrainTypes = new DatabrainTypes();
            }
        }

        int ReadNodeFromSerializedNodes(int index, out DatabrainTypes node)
        {
            var serializedNode = serializedGroup[index];
         
            DatabrainTypes newNode = new DatabrainTypes()
            {
                foldout = serializedNode.foldout,
                type = serializedNode.typeName,
                name = serializedNode.typeDisplayName,
                assemblyQualifiedTypeName = serializedNode.typeAssemblyQualifiedName,
                isFirstClassType = serializedNode.isFirstClassType,

                subTypes = new List<DatabrainTypes>()
            };

          
            for (int i = 0; i != serializedNode.childCount; i++)
            {
                DatabrainTypes childNode;
                index = ReadNodeFromSerializedNodes(++index, out childNode);
                newNode.subTypes.Add(childNode);
            }
            node = newNode;
            return index;
        }

        void AddToSerializedGroup(DatabrainTypes _rootDatabrainTypes)
        {
            var serializedNode = new SerializedGroup()
            {
                foldout = _rootDatabrainTypes.foldout,
                typeDisplayName = _rootDatabrainTypes.name,
                typeName = _rootDatabrainTypes.type,
                typeAssemblyQualifiedName = _rootDatabrainTypes.assemblyQualifiedTypeName,
                isFirstClassType = _rootDatabrainTypes.isFirstClassType,

                childCount = _rootDatabrainTypes.subTypes.Count,
                indexOfFirstChild = serializedGroup.Count + 1
            };

            serializedGroup.Add(serializedNode);
            foreach (var child in _rootDatabrainTypes.subTypes)
            {
                AddToSerializedGroup(child);
            }
        }
    }
}
#pragma warning restore