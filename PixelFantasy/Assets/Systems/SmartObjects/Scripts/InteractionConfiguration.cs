using System;
using System.Collections.Generic;
using Systems.Mood.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.SmartObjects.Scripts
{
    [Serializable]
    public class InteractionConfiguration
    {
        public string InteractionName;
        public EInteractionType InteractionType;
        public List<InteractionStatChange> StatChanges;
        public float Duration;
        public bool DestroyItemAfterInteraction;
        [FormerlySerializedAs("InteractionEmotion")] public EmotionSettings InteractionEmotionSettings;
    }
}
