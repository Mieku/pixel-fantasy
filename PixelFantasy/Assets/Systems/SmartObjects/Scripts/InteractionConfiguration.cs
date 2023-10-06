using System;
using System.Collections.Generic;
using Systems.Mood.Scripts;
using UnityEngine;

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
        public Emotion InteractionEmotion;
    }
}
