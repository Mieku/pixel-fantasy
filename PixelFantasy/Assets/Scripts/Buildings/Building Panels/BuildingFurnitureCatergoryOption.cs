using System;
using System.ComponentModel;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class BuildingFurnitureCatergoryOption : MonoBehaviour
    {
        [SerializeField] private Image _bgImage;
        [SerializeField] private TextMeshProUGUI _catergoryText;
        [SerializeField] private Sprite _selectedSpr;
        [SerializeField] private Sprite _unselectedSpr;

        // private FurnitureCatergory _catergory;
        // private Action<FurnitureCatergory, BuildingFurnitureCatergoryOption> _onPressedCallback;
        //
        // public FurnitureCatergory Catergory => _catergory;
        
        // public void Init(FurnitureCatergory catergory,
        //     Action<FurnitureCatergory, BuildingFurnitureCatergoryOption> onPressedCallback)
        // {
        //     _catergory = catergory;
        //     _onPressedCallback = onPressedCallback;
        //
        //     _catergoryText.text = _catergory.GetDescription();
        //     DisplaySelected(false);
        // }
        
        public void OnCatergoryPressed()
        {
            //_onPressedCallback.Invoke(_catergory, this);
        }

        public void DisplaySelected(bool isSelected)
        {
            if (isSelected)
            {
                _bgImage.sprite = _selectedSpr;
            }
            else
            {
                _bgImage.sprite = _unselectedSpr;
            }
        }
    }
}