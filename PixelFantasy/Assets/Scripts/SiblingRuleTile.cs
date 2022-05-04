using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "SiblingRuleTile", menuName = "2D/Tiles/SiblingRuleTile", order = 1)]
public class SiblingRuleTile : RuleTile
{
    public bool IsSiblingOnly;
    public enum SibingGroup
    {
        Walls,
        Floor,
    }
    public SibingGroup sibingGroup;

    public override bool RuleMatch(int neighbor, TileBase other)
    {
        if (other is RuleOverrideTile)
            other = (other as RuleOverrideTile).m_InstanceTile;
        
        switch (neighbor)
        {
            case TilingRule.Neighbor.This:
            {
                return other is SiblingRuleTile
                       && (other as SiblingRuleTile).sibingGroup == this.sibingGroup;
            }
            case TilingRule.Neighbor.NotThis:
            {
                return !(other is SiblingRuleTile
                         && (other as SiblingRuleTile).sibingGroup == this.sibingGroup);
            }
        }

            
        return base.RuleMatch(neighbor, other);
    }
}