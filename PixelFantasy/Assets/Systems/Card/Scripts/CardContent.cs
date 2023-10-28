using System;
using UnityEngine;

namespace Systems.Card.Scripts
{
    [Serializable]
    public class CardContent
    {
        public enum EContentType
        {
            Normal,
            Positive,
            Negative,
        }
        
        public string ContentOnCard;
        public string ExpandedContent;
        public EContentType ContentType;

        public bool HasExpandedContent => !string.IsNullOrEmpty(ExpandedContent);
    }
}
