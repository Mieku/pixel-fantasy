using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Social.Scripts
{
    [CreateAssetMenu(fileName = "SocialTopicSettings", menuName = "Settings/AI/Social Topic Settings")]
    public class SocialTopicSettings : ScriptableObject
    {
        [field: SerializeField] [PreviewField] public List<SocialTopic> AllOptions { get; protected set; }

        public SocialTopic GetRandomTopic()
        {
            int index = Random.Range(0, AllOptions.Count);
            return AllOptions[index];
        }
    }

    [Serializable]
    public class SocialTopic
    {
        [PreviewField] public Sprite TopicIcon;
    }
}
