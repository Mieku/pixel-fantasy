using System;
using System.Linq;
using Characters;
using UnityEngine;
using UnityEngine.AI;

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

        public Vector2? GetRandomInteriorPosition(Unit kinling)
        {
            const int maxAttempts = 100;

            for (int i = 0; i < maxAttempts; i++)
            {
                BoxCollider2D chosenCollider = _interiorColliders[UnityEngine.Random.Range(0, _interiorColliders.Length)];
                Vector2 randomPosition = GetRandomPositionInCollider(chosenCollider);

                if (kinling.UnitAgent.IsDestinationPossible(randomPosition))
                {
                    return randomPosition;
                }
            }

            return null;
        }
        
        private Vector2 GetRandomPositionInCollider(BoxCollider2D collider)
        {
            Vector2 randomPoint = new Vector2(
                UnityEngine.Random.Range(-collider.size.x / 2, collider.size.x / 2),
                UnityEngine.Random.Range(-collider.size.y / 2, collider.size.y / 2)
            );
            
            // Debugging
            Vector2 worldPosition = collider.transform.TransformPoint(randomPoint + collider.offset);
            Debug.DrawLine(worldPosition, worldPosition + Vector2.up * 0.5f, Color.red, 2f);
            
            return worldPosition;
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
