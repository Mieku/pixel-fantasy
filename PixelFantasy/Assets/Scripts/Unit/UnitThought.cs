using System;
using UnityEngine;

namespace Character
{
    public class UnitThought : MonoBehaviour
    {
        public enum ThoughtState
        {
            None,
            Idle,
            Moving,
            Building,
        }
        
        
        public Sprite Idle, Moving, Building;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(thoughtState), thoughtState, null);
            }
        }
    }
}
