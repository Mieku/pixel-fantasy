using System;
using System.Collections.Generic;
using Buildings;
using Items;
using UnityEngine;

namespace Systems.SmartObjects.Scripts
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class AmbientStatChange : MonoBehaviour
    {
        [SerializeField] protected List<InteractionStatChange> _statChanges;
        
        private CircleCollider2D _ambienceRadius;
        private List<BaseInteraction> _currentInteractions = new List<BaseInteraction>();
        
        //private Building _containingBuilding;

        public List<InteractionStatChange> StatChanges => _statChanges;

        private void Awake()
        {
            _ambienceRadius = GetComponent<CircleCollider2D>();
        }

        // private void OnDestroy()
        // {
        //     if (_containingBuilding != null)
        //     {
        //         _containingBuilding.OnBuildingFurnitureChanged -= OnBuildingFurnitureChanged;
        //     }
        // }

        // private void Start()
        // {
        //     var building = Helper.IsPositionInBuilding(transform.position);
        //     if (building != null)
        //     {
        //         _containingBuilding = building;
        //         _containingBuilding.OnBuildingFurnitureChanged += OnBuildingFurnitureChanged;
        //         UpdateRegisteredInteractionsFromFurniture(_containingBuilding.AllFurniture);
        //     }
        // }

        private void OnBuildingFurnitureChanged(List<Furniture> buildingFurniture)
        {
            UpdateRegisteredInteractionsFromFurniture(buildingFurniture);
        }

        private void UpdateRegisteredInteractionsFromFurniture(List<Furniture> allFurniture)
        {
            foreach (var currentInteraction in _currentInteractions)
            {
                currentInteraction.DeregisterAmbientStat(this);
            }
            _currentInteractions.Clear();
            
            foreach (var furniture in allFurniture)
            {
                var baseInteractions = furniture.GetComponentsInChildren<BaseInteraction>();
                foreach (var interaction in baseInteractions)
                {
                    _currentInteractions.Add(interaction);
                    interaction.RegisterAmbientStat(this);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // if(_containingBuilding != null) return;
            //
            // var building = Helper.IsPositionInBuilding(other.transform.position);
            // if (building != null) return;

            BaseInteraction interaction = other.GetComponent<BaseInteraction>();
            if (interaction != null)
            {
                _currentInteractions.Add(interaction);
                interaction.RegisterAmbientStat(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // if(_containingBuilding != null) return;
            //
            // var building = Helper.IsPositionInBuilding(other.transform.position);
            // if (building != null) return;

            BaseInteraction interaction = other.GetComponent<BaseInteraction>();
            if (interaction != null)
            {
                _currentInteractions.Remove(interaction);
                interaction.DeregisterAmbientStat(this);
            }
        }
        
        /*
         * If indoors, apply to all indoors
         * If outside, apply to all in radius
         * If outside, do NOT apply to indoor items even if in radius
         */
    }
}
