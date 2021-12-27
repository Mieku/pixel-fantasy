using System;
using System.Collections.Generic;
using Character;
using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace Controllers
{
    public class GameRTSController : MonoBehaviour
    {
        [SerializeField] private Transform selectionAreaTransform;
        
        private Vector3 startPosition;
        private List<UnitSelector> selectedCharacters;

        private void Awake()
        {
            selectedCharacters = new List<UnitSelector>();
            selectionAreaTransform.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                selectionAreaTransform.gameObject.SetActive(true);
                startPosition = UtilsClass.GetMouseWorldPosition();
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 currentMousePos = UtilsClass.GetMouseWorldPosition();
                var lowerLeft = new Vector2(
                    Mathf.Min(startPosition.x, currentMousePos.x),
                    Mathf.Min(startPosition.y, currentMousePos.y)
                );
                var upperRight = new Vector2(
                    Mathf.Max(startPosition.x, currentMousePos.x),
                    Mathf.Max(startPosition.y, currentMousePos.y)
                );
                selectionAreaTransform.position = lowerLeft;
                selectionAreaTransform.localScale = upperRight - lowerLeft;
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectionAreaTransform.gameObject.SetActive(false);
                
                Collider2D[] collider2DArray =
                    Physics2D.OverlapAreaAll(startPosition, UtilsClass.GetMouseWorldPosition());

                foreach (var selector in selectedCharacters)
                {
                    selector.SetSelectionVisible(false);
                }
                
                selectedCharacters.Clear();
                
                foreach (var collider2D in collider2DArray)
                {
                    UnitSelector selector = collider2D.GetComponent<UnitSelector>();
                    if (selector != null)
                    {
                        selectedCharacters.Add(selector);
                        selector.SetSelectionVisible(true);
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                var movePos = UtilsClass.GetMouseWorldPosition();

                var targetPosList = GetPositionListAround(movePos, 
                    new float[] {1f, 2f, 3f, 4f, 5f, 6f, 7f}, 
                    new int[] {5, 10, 20, 30, 40, 50, 60});
                int targetPosListIndex = 0;
                
                foreach (var selectedCharacter in selectedCharacters)
                {
                    selectedCharacter.MoveTo(targetPosList[targetPosListIndex]);
                    targetPosListIndex = (targetPosListIndex + 1) % targetPosList.Count;
                }
            }
        }

        private List<Vector3> GetPositionListAround(Vector3 startPos, float[] ringDistanceArray,
            int[] ringPosCountArray)
        {
            var posList = new List<Vector3>();
            for (int i = 0; i < ringDistanceArray.Length; i++)
            {
                posList.AddRange(GetPositionListAround(startPos, ringDistanceArray[i], ringPosCountArray[i]));
            }

            return posList;
        }

        private List<Vector3> GetPositionListAround(Vector3 startPos, float distance, int positionCount)
        {
            List<Vector3> posList = new List<Vector3>();
            for (int i = 0; i < positionCount; i++)
            {
                float angle = i * (360f / positionCount);
                var dir = ApplyRotationToVector(new Vector3(1, 0), angle);
                var pos = startPos + dir * distance;
                posList.Add(pos);
            }

            return posList;
        }

        private Vector3 ApplyRotationToVector(Vector3 vec, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vec;
        }
    }
}
