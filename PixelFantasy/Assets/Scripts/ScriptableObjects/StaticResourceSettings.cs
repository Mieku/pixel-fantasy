using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StaticResourceSettings", menuName = "Settings/Resources/Static Resource Settings")]
    public class StaticResourceSettings : ResourceSettings
    {
        [SerializeField] private int _workToExtract;
        [SerializeField] private HarvestableItems _harvestableItems;
        [SerializeField] private List<Sprite> _potentialSprites = new List<Sprite>();

        public HarvestableItems HarvestableItems => _harvestableItems;

        public int GetRandomSpriteIndex()
        {
            int rand = Random.Range(0, _potentialSprites.Count - 1);
            return rand;
        }

        public Sprite GetSprite(int index)
        {
            return _potentialSprites[index];
        }

        public int WorkToExtract => _workToExtract;
    }
}
