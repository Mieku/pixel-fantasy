using System;
using UnityEngine;

namespace Characters
{
    public class UnitThought : MonoBehaviour
    {
        public enum ThoughtState
        {
            None,
            Idle,
            Moving,
            Building,
            Cleaning
        }
        
        
        public Sprite Idle, Moving, Building, Cleaning;
        public GameObject ThoughtBubble;

        private SpriteRenderer thoughtRenderer;
        private ThoughtState thoughtState;

        private void Awake()
        {
            thoughtRenderer = ThoughtBubble.GetComponent<SpriteRenderer>();
        }

        public void SetThought(ThoughtState thoughtState)
        {
            switch (thoughtState)
            {
                case ThoughtState.None:
                    ThoughtBubble.SetActive(false);
                    break;
                case ThoughtState.Idle:
                    ThoughtBubble.SetActive(true);
                    thoughtRenderer.sprite = Idle;
                    break;
                case ThoughtState.Moving:
                    ThoughtBubble.SetActive(true);
                    thoughtRenderer.sprite = Moving;
                    break;
                case ThoughtState.Building:
                    ThoughtBubble.SetActive(true);
                    thoughtRenderer.sprite = Building;
                    break;
                case ThoughtState.Cleaning:
                    ThoughtBubble.SetActive(true);
                    thoughtRenderer.sprite = Cleaning;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(thoughtState), thoughtState, null);
            }
        }

        public UnitThoughtData GetSaveData()
        {
            return new UnitThoughtData
            {
                ThoughtState = this.thoughtState,
            };
        }

        public void SetLoadData(UnitThoughtData unitThoughtData)
        {
            thoughtState = unitThoughtData.ThoughtState;
            SetThought(thoughtState);
        }

        public struct UnitThoughtData
        {
            public ThoughtState ThoughtState;
        }
    }
}
