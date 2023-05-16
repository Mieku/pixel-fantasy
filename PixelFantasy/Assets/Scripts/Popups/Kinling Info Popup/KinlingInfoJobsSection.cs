using Characters;
using Managers;
using TMPro;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoJobsSection : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _professionDropdown;
        
        private Unit _unit;

        public void Show(Unit unit)
        {
            _unit = unit;
            
            RefreshProfessionsDropdown();
        }

        private void RefreshProfessionsDropdown()
        {
            _professionDropdown.ClearOptions();
            var allJobs = Librarian.Instance.GetAllProfessions();
            int unitsIndex = 0;
            foreach (var job in allJobs)
            {
                if (_unit.GetUnitState().Profession == job || job.IsRequiredToolAvailable)
                {
                    if (_unit.GetUnitState().Profession == job)
                    {
                        // Preselect this
                        unitsIndex = allJobs.IndexOf(job);
                    }

                    _professionDropdown.options.Add(new TMP_Dropdown.OptionData(job.ProfessionName));
                }
            }
            
            _professionDropdown.SetValueWithoutNotify(unitsIndex);
        }

        public void OnProfessionDDChanged(TMP_Dropdown dropdown)
        {
            var professionName = dropdown.options[dropdown.value].text;
            if (professionName != _unit.GetUnitState().Profession.ProfessionName)
            {
                var prof = Librarian.Instance.GetProfession(professionName);
                _unit.AssignProfession(prof);
            }
        }
    }
}
