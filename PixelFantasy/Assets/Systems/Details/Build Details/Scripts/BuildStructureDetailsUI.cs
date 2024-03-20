using System.Collections.Generic;
using Data.Structure;
using Databrain;
using Databrain.Attributes;
using Systems.Build_Controls.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Details.Build_Details.Scripts
{
    public class BuildStructureDetailsUI : MonoBehaviour
    {
        public DataLibrary DataLibrary;
        [DataObjectDropdown("DataLibrary")] [SerializeField] private List<WallSettings> _wallOptions;

        [SerializeField] private StructureCategoryBtn _structureCategoryBtn;
        
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private TextMeshProUGUI _panelTitle;

        public void Show()
        {
            _panelHandle.SetActive(true);
            _structureCategoryBtn.HighlightBtn(true);
        }

        public void Hide()
        {
            _panelHandle.SetActive(false);
            _structureCategoryBtn.HighlightBtn(false);
        }
    }
}
