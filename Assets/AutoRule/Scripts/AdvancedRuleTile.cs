using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable SuggestBaseTypeForParameter

// TODO: Change to a "group" based solution at sme point, since checking for thousands of arrays of tiles
// TODO: probably isn't ideal.

[CreateAssetMenu(fileName = "New Joining Rule Tile", menuName = "Tiles/Joining Rule Tile")]
public class AdvancedRuleTile : RuleTile<AdvancedRuleTile.Neighbor>
{
    [SerializeField] private TileBase[] friendTiles;

    [FormerlySerializedAs("joinAllTiles")]
    [SerializeField] private bool alwaysJoinAllTiles;
    [SerializeField] private bool alwaysJoinFriends;

    public class Neighbor : RuleTile.TilingRuleOutput.Neighbor
    {
        /// <summary>
        /// The Rule Tile will check if the contents of the cell in that direction is an instance of this or a friend Rule Tile.
        /// If not, the rule will fail.
        /// </summary>
        public const int ThisOrFriend = 3;
        /// <summary>
        /// The Rule Tile will check if the contents of the cell in that direction is an instance of a friend Rule Tile.
        /// If not, the rule will fail.
        /// </summary>
        public const int Friend = 4;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        if (tile != null && alwaysJoinAllTiles)
            if(tile != this)
                return true;

        switch (neighbor)
        {
            case Neighbor.ThisOrFriend:
            {
                return tile == this || HasFriendTile(tile);
            }
            case Neighbor.Friend:
            {
                return HasFriendTile(tile);
            }
            case TilingRuleOutput.Neighbor.NotThis:
            {
                return tile != this && !HasFriendTile(tile);
            }
            case TilingRuleOutput.Neighbor.This:
            {
                if (alwaysJoinFriends) return HasFriendTile(tile);
                
                return tile == this;
            }
            default:
                return true;
        }
    }

    /// <summary>
    /// Checks if supplied tile matches any friend tiles on this tile.
    /// </summary>
    /// <param name="tile">Tile to compare to this' friend tiles.</param>
    /// <returns></returns>
    private bool HasFriendTile(TileBase tile)
    {
        if (tile == null)
            return false;

        if (friendTiles.Length < 1)
            return false;

        return friendTiles.Any(t => t == tile);
    }
}
