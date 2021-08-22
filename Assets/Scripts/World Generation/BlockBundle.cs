using UnityEngine.Tilemaps;

/// <summary>
/// Represents a cell in the tilemap's grid. Contains a foreground as well as a background block.
/// </summary>
public class BlockBundle
{
    private Block foreground;
    private Block background;
    
    public BlockBundle(Block foreground, Block background)
    {
        if(foreground != null)
            this.foreground = (Block)foreground.CreateDuplicate(false, 1);
        if(background != null)
            this.background = (Block)background.CreateDuplicate(false, 1);
    }

    public Block GetForegroundBlock()
    {
        return foreground;
    }

    public Block GetBackgroundBlock()
    {
        return background;
    }

    public TileBase GetForegroundTile()
    {
        if (GetForegroundBlock() == null)
            return null;
        
        return GetForegroundBlock().Tile != null ? GetForegroundBlock().Tile : null;
    }

    public TileBase GetBackgroundTile()
    {
        if (GetBackgroundBlock() == null)
            return null;

        return GetBackgroundBlock().Tile != null ? GetBackgroundBlock().Tile : null;
    }

    public void SetForegroundBlock(Block newBlock)
    {
        if(newBlock != null)
            foreground = (Block)newBlock.CreateDuplicate(true, 1);
    }

    public void SetBackgroundBlock(Block newBlock)
    {
        if(newBlock != null)
            background = (Block)newBlock.CreateDuplicate(true, 1);
    }
}