using System;
using System.Linq;
using Characters;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(Collider2D))]
    public class BuildingInteriorDetector : MonoBehaviour
    {
        [SerializeField] private Building _building;

        private BoxCollider2D[] _interiorColliders;

        private void Awake()
        {
            _interiorColliders = GetComponentsInChildren<BoxCollider2D>();
        }

        public bool IsColliderInInterior(Collider2D otherCollider)
        {
            Vector2[] otherColliderCorners = GetColliderCorners(otherCollider);
            bool isInside = false;

            // Check if each corner of the otherCollider is within any of the _interiorColliders
            foreach (var corner in otherColliderCorners)
            {
                isInside = _interiorColliders.Any(interiorCollider => interiorCollider.bounds.Contains(corner));
                // If any corner is not inside any of the _interiorColliders, then otherCollider is not fully inside
                if (!isInside) return false;
            }

            // If all corners are inside at least one of the _interiorColliders, then otherCollider is considered inside
            return true;
        }

        private Vector2[] GetColliderCorners(Collider2D collider)
        {
            // Assuming the collider is a BoxCollider2D for simplicity, but you can extend this to handle other types
            if (collider is BoxCollider2D box)
            {
                Vector2 topLeft = box.offset + new Vector2(-box.size.x, box.size.y) * 0.5f;
                Vector2 topRight = box.offset + new Vector2(box.size.x, box.size.y) * 0.5f;
                Vector2 bottomLeft = box.offset + new Vector2(-box.size.x, -box.size.y) * 0.5f;
                Vector2 bottomRight = box.offset + new Vector2(box.size.x, -box.size.y) * 0.5f;

                return new Vector2[]
                {
                    collider.transform.TransformPoint(topLeft),
                    collider.transform.TransformPoint(topRight),
                    collider.transform.TransformPoint(bottomLeft),
                    collider.transform.TransformPoint(bottomRight)
                };
            }
            // Add checks for other Collider2D types if necessary
            return new Vector2[0];
        }
        
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
