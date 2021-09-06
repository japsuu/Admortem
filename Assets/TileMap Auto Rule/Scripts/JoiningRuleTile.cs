using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable SuggestBaseTypeForParameter

// TODO: Change to a "group" based solution at sme point, since checking for thousands of arrays of tiles
// TODO: probably isn't ideal.

[CreateAssetMenu(fileName = "New Joining Rule Tile", menuName = "Tiles/Joining Rule Tile")]
public class JoiningRuleTile : RuleTile<JoiningRuleTile.Neighbor>
{
    [SerializeField] private TileBase[] friendTiles;

    [SerializeField] private bool joinAllTiles;

    public class Neighbor : RuleTile.TilingRuleOutput.Neighbor
    {
        public const int ThisOrFriend = 1;
        public const int Friend = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        if (tile != null && joinAllTiles)   //BUG: Fix this ASAP!
            if(tile != this)
                return true;
        
        return neighbor switch
        {
            Neighbor.ThisOrFriend => tile == this || HasFriendTile(tile),
            Neighbor.Friend => HasFriendTile(tile),
            TilingRuleOutput.Neighbor.NotThis => tile == null,
            _ => true
        };
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
