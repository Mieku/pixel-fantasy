using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "LayeredRuleTile", menuName = "2D/Tiles/LayeredRuleTile", order = 1)]
public class LayeredRuleTile : RuleTile<LayeredRuleTile.Neighbor> {
    public int Layer;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        LayeredRuleTile lTile = tile as LayeredRuleTile;
        switch (neighbor)
        {
            case TilingRuleOutput.Neighbor.This: return lTile == this && lTile != null && lTile.Layer == Layer;
            case TilingRuleOutput.Neighbor.NotThis: return lTile != this;
        }

        return true;
    }
}