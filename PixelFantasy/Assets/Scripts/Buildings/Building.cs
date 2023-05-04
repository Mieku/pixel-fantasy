using System;
using System.Collections.Generic;
using Buildings.Building_Panels;
using Characters;
using Gods;
using HUD;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.AI;
using Zones;

namespace Buildings
{
    public abstract class Building : MonoBehaviour
    {
        [SerializeField] private Transform _exteriorArtRoot;
        [SerializeField] private GameObject _obstacleRoot;
        [SerializeField] private OffMeshLink _entranceLink;
        [SerializeField] private Color _blueprintColour;
        [SerializeField] private Color _canPlaceColour;
        [SerializeField] private Color _cantPlaceColour;
        [SerializeField] private GameObject _footingsRoot;

        private SpriteRenderer[] _allExteriorArt;
        private SpriteRenderer[] _footings;
        private BuildingNode _buildingNode;
        private List<Material> _materials = new List<Material>();
        private int _fadePropertyID;

        [SerializeField] protected Vector2 _interiorCamOffset;

        public TaskQueue BuildingTasks = new TaskQueue();
        public OffMeshLink EntranceLink => _entranceLink;
        public BuildingData BuildingData => _buildingNode.BuildingData;
        public int MaxOccupants => BuildingData.MaxOccupants;
        public List<UnitState> Occupants = new List<UnitState>();

        public bool IsBuilt;
        public bool IsPlaced;
        public Interior Interior;
        public Zone AssignedZone;
        public BuildingPanel BuildingPanel => _buildingNode.BuildingData.BuildingPanel;

        private void Awake()
        {
            _allExteriorArt = _exteriorArtRoot.GetComponentsInChildren<SpriteRenderer>();
            _footings = _footingsRoot.GetComponentsInChildren<SpriteRenderer>();
            _obstacleRoot.SetActive(false);
        
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            foreach (var extArt in _allExteriorArt)
            {
                _materials.Add(extArt.material);
            }
        }

        public void Init(BuildingNode buildingNode)
        {
            _buildingNode = buildingNode;
        }

        public void SetBlueprint()
        {
            HideFootings();
            _obstacleRoot.SetActive(true);
            NavMeshManager.Instance.UpdateNavMesh();
            ColourArt(ColourStates.Blueprint);
        }

        public void ColourArt(ColourStates colourState)
        {
            Color colour;
            switch (colourState)
            {
                case ColourStates.Built:
                    colour = Color.white;
                    break;
                case ColourStates.Blueprint:
                    colour = _blueprintColour;
                    break;
                case ColourStates.CanPlace:
                    colour = _canPlaceColour;
                    break;
                case ColourStates.CantPlace:
                    colour = _cantPlaceColour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colourState), colourState, null);
            }
        
            foreach (var extArt in _allExteriorArt)
            {
                extArt.color = colour;
            }
        }

        public bool CheckPlacement()
        {
            bool result = true;

            foreach (var footing in _footings)
            {
                if (Helper.IsGridPosValidToBuild(footing.transform.position, _buildingNode.InvalidPlacementTags))
                {
                    footing.color = _canPlaceColour;
                }
                else
                {
                    footing.color = _cantPlaceColour;
                    result = false;
                }
            }

            return result;
        }

        private void HideFootings()
        {
            _footingsRoot.gameObject.SetActive(false);
        }
    
        private void TriggerOutline(bool showOuline)
        {
            foreach (var material in _materials)
            {
                if (showOuline)
                {
                    material.SetFloat(_fadePropertyID, 1);
                    ColourArt(ColourStates.Built);
                }
                else
                {
                    material.SetFloat(_fadePropertyID, 0);
                    if (!IsBuilt)
                    {
                        ColourArt(ColourStates.Blueprint);
                    }
                }
            }
        }

        private void OnMouseEnter()
        {
            if(!IsPlaced) return;
            
            TriggerOutline(true);
        }

        private void OnMouseExit()
        {
            if(!IsPlaced) return;
            
            TriggerOutline(false);
        }

        private void OnMouseDown()
        {
            if(!IsPlaced) return;
            
            OnBuildingClicked();
        }

        /// <summary>
        /// Moves the camera to the interior of the building
        /// </summary>
        public void ViewInterior()
        {
            if (Interior == null)
            {
                Debug.LogError("Attempted to enter not existing Interior");
                return;
            }
            
            Vector2 cameraPos = new Vector2(Interior.EntrancePos.position.x + _interiorCamOffset.x, Interior.EntrancePos.position.y + _interiorCamOffset.y);
            Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, Camera.main.transform.position.z);
        }

        /// <summary>
        /// Moves the camera to the exterior of the building
        /// </summary>
        public void ViewExterior()
        {
            Vector2 cameraPos = new Vector2(transform.position.x, transform.position.y);
            Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, Camera.main.transform.position.z);
        }

        protected abstract void OnBuildingClicked();

        public enum ColourStates
        {
            Built,
            Blueprint,
            CanPlace,
            CantPlace,
        }

        public void AssignWorker(UnitState unit)
        {
            Occupants.Add(unit);
            unit.Occupation = this;
            unit.Profession = BuildingData.WorkersProfession;
        }

        public void UnassignWorker(UnitState unit)
        {
            Occupants.Remove(unit);
            unit.RemoveOccupation();
            unit.Profession = Profession.Labourer;
        }

        public void AssignHome(UnitState unit)
        {
            Occupants.Add(unit);
            unit.Home = this;
        }
    }
}
