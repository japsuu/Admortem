using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Joining Rule Tile", menuName = "Tiles/Joining Rule Tile")]
public class JoiningRuleTile : RuleTile<JoiningRuleTile.Neighbor>
{
    public TileBase[] friendTiles;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int ThisOrFriend = 1;
        public const int Friend = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case Neighbor.ThisOrFriend:
                return tile == this || HasFriendTile(tile);
            case Neighbor.Friend:
                return HasFriendTile(tile);
            case TilingRuleOutput.Neighbor.NotThis:
                return tile == null;
        }
        return true;
    }

    private bool HasFriendTile(TileBase tile)
    {
        if (tile == null)
            return false;

        if (friendTiles.Length < 1)
            return false;

        foreach (var t in friendTiles)
        {
            if (t == tile)
                return true;
        }
        return false;
    }
}
