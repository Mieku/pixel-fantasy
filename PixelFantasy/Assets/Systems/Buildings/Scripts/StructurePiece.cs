using System.Collections.Generic;
using System.Linq;
using AI;
using Controllers;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Buildings.Scripts
{
    public abstract class StructurePiece : Construction
    {
        [SerializeField] private StructureCell _cell;

        public StructureCell Cell => _cell;

        protected void OnPlaced()
        {
            RuntimeData.Position = transform.position;
            StructureDatabase.Instance.RegisterStructure(this);
        }

        protected void OnDeconstructed()
        {
            StructureDatabase.Instance.DeregisterStructure(this);
        }

        public abstract void RefreshTile();

        public void ShowCell(bool shouldShow)
        {
            Cell.CellRenderer.enabled = shouldShow;
        }
        
        public override void CancelConstruction()
        {
            OnDeconstructed();
            base.CancelConstruction();
        }

        public abstract void DeletePiece();
    }
}
