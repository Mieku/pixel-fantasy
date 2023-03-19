using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingExterior : MonoBehaviour
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
    private Building _building;

    public OffMeshLink EntranceLink => _entranceLink;

    private void Awake()
    {
        _allExteriorArt = _exteriorArtRoot.GetComponentsInChildren<SpriteRenderer>();
        _footings = _footingsRoot.GetComponentsInChildren<SpriteRenderer>();
        _obstacleRoot.SetActive(false);
    }

    public void Init(Building building)
    {
        _building = building;
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
            if (Helper.IsGridPosValidToBuild(footing.transform.position, _building.InvalidPlacementTags))
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

    public enum ColourStates
    {
        Built,
        Blueprint,
        CanPlace,
        CantPlace,
    }
}
