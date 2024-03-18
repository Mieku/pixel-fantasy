using Databrain;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Item
{
    [DataObjectAddToRuntimeLibrary]
    public class CropData : DataObject
    {
        public string CropName;
        public ItemData HarvestedItem;
        public Sprite Stage1, Stage2, Stage3, Stage4;
        public float TimeToHarvestSec;
        public float WaterFrequencySec;
        public int AmountToHarvest;

        public Sprite GetCropImage(float growthTime)
        {
            float timePerStage = TimeToHarvestSec / 5f;
            if (growthTime < timePerStage)
            {
                return null;
            }
            if (growthTime < timePerStage * 2f)
            {
                return Stage1;
            }
            if (growthTime < timePerStage * 3f)
            {
                return Stage2;
            }
            if (growthTime < timePerStage * 4f)
            {
                return Stage3;
            }
            else
            {
                return Stage4;
            }
        }
    }
}
