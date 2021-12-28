using System;
using System.Security.Cryptography;
using CodeMonkey.Utils;
using Gods;
using UnityEngine;

namespace Gods
{
    public class TaskMaster : God<TaskMaster>
    {
        private TaskSystem taskSystem = new TaskSystem();
        
        // For Testing placeholder
        public Sprite rubble;

        public TaskSystem GetTaskSystem()
        {
            return taskSystem;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                AssignMoveLocation();
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                CreateGarbageOnMousePos();
            }
        }

        // TODO: Remove these when no longer needed
        #region Used For Testing

        private void AssignMoveLocation()
        {
            var newTask = new TaskSystem.Task.MoveToPosition
            {
                targetPosition = UtilsClass.GetMouseWorldPosition()
            };
            taskSystem.AddTask(newTask);
        }

        private void CreateGarbageOnMousePos()
        {
            var garbage = SpawnRubble(UtilsClass.GetMouseWorldPosition());
            var garbageSpriteRenderer = garbage.GetComponent<SpriteRenderer>();
            var newTask = new TaskSystem.Task.GarbageCleanup
            {
                targetPosition = garbage.transform.position,
                cleanUpAction = () =>
                {
                    float alpha = 1f;
                    FunctionUpdater.Create((() =>
                    {
                        alpha -= Time.deltaTime * 1.1f;
                        garbageSpriteRenderer.color = new Color(1, 1, 1, alpha);
                        if (alpha <= 0f)
                        {
                            Destroy(garbage);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }));
                }
            };
            taskSystem.AddTask(newTask);
        }

        private GameObject SpawnRubble(Vector3 position)
        {
            GameObject gameObject = new GameObject("Rubble", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = rubble;
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
            gameObject.transform.position = position;

            return gameObject;
        }

        #endregion
        
    }
}
