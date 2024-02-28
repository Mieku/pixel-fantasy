using System.Collections.Generic;
using System.Linq;
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
            StructureManager.Instance.RegisterStructurePiece(this);
        }

        protected void OnDeconstructed()
        {
            StructureManager.Instance.DeregisterStructurePiece(this);
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
    }
}
