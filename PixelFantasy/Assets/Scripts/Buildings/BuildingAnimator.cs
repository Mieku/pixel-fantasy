using System;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(Animator))]
    public class BuildingAnimator : MonoBehaviour
    {
        private Animator _anim;
        
        private const string MATING = "isMating";
        [SerializeField] private GameObject _matingParticles;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public void SetAnimation(EBuildingAnimation buildingAnimation)
        {
            StopAnimations();
            
            switch (buildingAnimation)
            {
                case EBuildingAnimation.None:
                    break;
                case EBuildingAnimation.Mating:
                    SetBuildingAnimation(MATING);
                    _matingParticles.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildingAnimation), buildingAnimation, null);
            }
        }

        private void StopAnimations()
        {
            _matingParticles.SetActive(false);
            
            SetBuildingAnimation(MATING, false);
        }

        private void SetBuildingAnimation(string propertyID, bool isEnable = true)
        {
            _anim.SetBool(propertyID, isEnable);
        }
    }

    public enum EBuildingAnimation
    {
        None = 0,
        Mating = 1,
    }
}
