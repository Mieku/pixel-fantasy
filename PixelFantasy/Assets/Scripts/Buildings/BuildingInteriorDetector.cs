using Characters;
using UnityEngine;

namespace Buildings
{
    public class BuildingInteriorDetector : MonoBehaviour
    {
        [SerializeField] private Building _building;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null)
            {
                unit.SetInsideBuilding(_building);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null)
            {
                unit.SetInsideBuilding(null);
            }
        }
    }
}
