using Databrain;
using Databrain.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Databrain.Examples
{
    public class EnemyUI : MonoBehaviour
    {
        public Camera followCamera;

        public DataLibrary library;
        [DataObjectDropdown("library")]
        public EnemyDataDemo enemyData;
        public Image healthBar;

        private void Start()
        {
            followCamera = Camera.main;
        }

        public void UpdateHealthbar(float _currentHealth)
        {
            float _maxHealth = enemyData.health;

            healthBar.rectTransform.sizeDelta = new Vector2((_currentHealth * 2f) / _maxHealth, 0.1f);
        }

        void Update()
        {
            transform.rotation = Quaternion.LookRotation(transform.position - followCamera.transform.position);
        }
    }
}