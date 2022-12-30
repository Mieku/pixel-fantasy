using System;
using Gods;
using ScriptableObjects;
using UnityEngine;

namespace Characters
{
    public class UnitAppearance : MonoBehaviour
    {
        public HairData HairData;
        public Gender Gender;
        
        [SerializeField] private SpriteRenderer _hairRenderer;
        [SerializeField] private SpriteRenderer _blushRenderer;

        private void Start()
        {
            SetGender(Gender);
        }

        private void SetGender(Gender gender)
        {
            Gender = gender;

            if (gender == Gender.Male)
            {
                _blushRenderer.gameObject.SetActive(false);
            }
            else
            {
                _blushRenderer.gameObject.SetActive(true);
            }
        }
        
        public void SetHairDirection(UnitActionDirection dir)
        {
            switch (dir)
            {
                case UnitActionDirection.Side:
                    _hairRenderer.sprite = HairData.Side;
                    break;
                case UnitActionDirection.Up:
                    _hairRenderer.sprite = HairData.Back;
                    break;
                case UnitActionDirection.Down:
                    _hairRenderer.sprite = HairData.Front;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }
    
        public void SetLoadData(AppearanceData data)
        {
            HairData = Librarian.Instance.GetHairData(data.Hair);
            SetGender(data.Gender);
        }

        public AppearanceData GetSaveData()
        {
            return new AppearanceData
            {
                Hair = HairData.Name,
                Gender = Gender,
            };
        }

        public struct AppearanceData
        {
            public string Hair;
            public Gender Gender;
        }
    }

    public enum Gender
    {
        Male,
        Female
    }
}
