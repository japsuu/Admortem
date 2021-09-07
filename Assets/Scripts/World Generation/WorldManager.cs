using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public Tilemap foregroundTilemap;
    public Tilemap backgroundTilemap;

    private BlockBundle[,] world;
    private int halfWorldWidth;
    private int halfWorldHeight;

    private bool spawnPointSet;
    
    private ChunkIt chunkIt;

    public enum DamageResult
    {
        NotDamaged,
        Damaged,
        Destroyed
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        chunkIt = foregroundTilemap.GetComponent<ChunkIt>();
    }

    public void CreateWorld(BlockBundle[,] worldBundle)
    {
        StartCoroutine(GenerateWorld(worldBundle));
    }

    private IEnumerator GenerateWorld(BlockBundle[,] worldBundle)
    {
        world = worldBundle;
        
        //World.Initialize(GetColliderChunks(world), foregroundTilemap, backgroundTilemap);
        halfWorldWidth = TerrainGenerator.Instance.worldWidth / 2;
        halfWorldHeight = TerrainGenerator.Instance.worldHeight / 2;

        for (int x = 0; x < worldBundle.GetLength(0); x++)
        {
            for (int y = worldBundle.GetLength(1) - 1; y >= 0; y--)
            {
                if (worldBundle[x, y] == null) continue;

                //Debug.Log("Cell pos: " + new Vector2(x, y));
                // Set the player SpawnPoint
                //if (!spawnPointSet && x == halfWorldWidth)
                //{
                //    Debug.Log("Cell pos: " + new Vector3Int(x, y, 0));
                //    GameController.PlayerSpawnPos = foregroundTilemap.LocalToWorld(new Vector2(0, y / 2));
                //    spawnPointSet = true;
                //    
                //    Debug.Log("Spawn pos: " + GameController.PlayerSpawnPos);
                //}
                
                foregroundTilemap.SetTile(new Vector3Int(x - halfWorldWidth, y - halfWorldHeight, 0), worldBundle[x, y].GetForegroundTile());
                backgroundTilemap.SetTile(new Vector3Int(x - halfWorldWidth, y - halfWorldHeight, 0), worldBundle[x, y].GetBackgroundTile());
            }
            
            if(x % 4 == 0)
                yield return null;
        }
        
        chunkIt.GenerateColliders();
        
        GameController.Instance.SpawnPlayer();

        yield return null;
    }

    public void PlaceBlockAt(Vector3Int tilePos, Tilemap selectedTilemap, Block newBlock)
    {
        if(selectedTilemap == foregroundTilemap)
            PlaceForegroundBlockAt(tilePos, newBlock);
        else
            PlaceBackgroundBlockAt(tilePos, newBlock);

        InventoryUIManager.Instance.Inventory.RemoveItem(newBlock);
    }

    /// <summary>
    /// Damages a block at position.
    /// </summary>
    /// <param name="tilePos">Position to damage the tile at</param>
    /// <param name="selectedTilemap"></param>
    /// <param name="damageAmount">Amount to damage</param>
    /// <returns>True if the block was damaged, false otherwise.</returns>
    public DamageResult DamageBlockAt(Vector3Int tilePos, Tilemap selectedTilemap, int damageAmount)
    {
        TileBase clickedTile = selectedTilemap.GetTile(tilePos);

        if (clickedTile == null)
        {
            return DamageResult.NotDamaged;
        }
        
        if (selectedTilemap == foregroundTilemap)
        {
            Block block = GetBlockBundleAt(tilePos).GetForegroundBlock();

            block.Durability -= damageAmount;
        
            RuntimeManager.PlayOneShot(block.GetMinedEvent(), transform.position);

            if (block.Durability > 0) return DamageResult.Damaged;
            
            DestroyForegroundBlockAt(tilePos);
            return DamageResult.Destroyed;
        }
        else
        {
            Block block = GetBlockBundleAt(tilePos).GetBackgroundBlock();

            block.Durability -= damageAmount;
        
            RuntimeManager.PlayOneShot(block.GetMinedEvent(), transform.position);

            if (block.Durability > 0) return DamageResult.Damaged;
            
            DestroyBackgroundBlockAt(tilePos);
            return DamageResult.Destroyed;
        }
    }

    /// <summary>
    /// Destroys the block at FOREGROUND tilePos, and spawns a pickup there.
    /// </summary>
    /// <param name="tilePos"></param>
    private void DestroyForegroundBlockAt(Vector3Int tilePos)
    {
        Vector3 tileWorldPos = foregroundTilemap.GetCellCenterWorld(tilePos);

        Block block = world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight].GetForegroundBlock();
        
        ItemPickup.SpawnItemPickup(block, new Vector2(tileWorldPos.x, tileWorldPos.y + 0.1f), Quaternion.identity);
        
        RuntimeManager.PlayOneShot(block.GetBrokenEvent(), transform.position);
        
        world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight].SetForegroundBlock(null);
        chunkIt.SetTile(tilePos, null);
    }

    /// <summary>
    /// Destroys the block at BACKGROUND tilePos, and spawns a pickup there.
    /// </summary>
    private void DestroyBackgroundBlockAt(Vector3Int tilePos)
    {
        Vector3 tileWorldPos = backgroundTilemap.GetCellCenterWorld(tilePos);
        
        Block block = world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight].GetBackgroundBlock();
        
        ItemPickup.SpawnItemPickup(block, new Vector2(tileWorldPos.x, tileWorldPos.y + 0.1f), Quaternion.identity);
        
        RuntimeManager.PlayOneShot(block.GetBrokenEvent(), transform.position);
        
        world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight].SetBackgroundBlock(null);
        backgroundTilemap.SetTile(tilePos, null);
    }

    /// <summary>
    /// Sets the block at FOREGROUND tilePos.
    /// </summary>
    /// <param name="tilePos"></param>
    /// <param name="newBlock"></param>
    private void PlaceForegroundBlockAt(Vector3Int tilePos, Block newBlock)
    {
        world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight].SetForegroundBlock(newBlock);
        chunkIt.SetTile(tilePos, newBlock.Tile);
        
        RuntimeManager.PlayOneShot(newBlock.GetPlacedEvent(), transform.position);
    }

    /// <summary>
    /// Sets the block at BACKGROUND tilePos.
    /// </summary>
    private void PlaceBackgroundBlockAt(Vector3Int tilePos, Block newBlock)
    {
        world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight].SetBackgroundBlock(newBlock);
        backgroundTilemap.SetTile(tilePos, newBlock.Tile);
        
        RuntimeManager.PlayOneShot(newBlock.GetPlacedEvent(), transform.position);
    }

    public BlockBundle GetBlockBundleAt(Vector3Int tilePos)
    {
        return world[tilePos.x + halfWorldWidth, tilePos.y + halfWorldHeight];
    }
    
    
    // Save an array of BlockBundles to represent the world.
    // Internally handle setting and removing blockBundles in the World-class.
    // Expose methods only for Removing and Adding blocks.
    // Internally handle the colliderTilemap with the dummyTiles, and synchronize the visuals with that.

    /*
    private List<ColliderChunk> GetColliderChunks(BlockBundle[,] world)
    {
        List<ColliderChunk> chunks = new List<ColliderChunk>();

        // For each chunk
        for (int chunkIndex = 0; chunkIndex < TerrainGenerator.Instance.worldWidth / colliderChunkWidth; chunkIndex++)
        {
            BlockBundle[,] chunkContents = new BlockBundle[colliderChunkWidth, TerrainGenerator.Instance.worldHeight];

            int worldOffset = chunkIndex * colliderChunkWidth;

            // For x-axis inside chunk bounds
            for (int x = 0; x < colliderChunkWidth; x++)
            {
                // For y-axis inside chunk bounds
                for (int y = 0; y < TerrainGenerator.Instance.worldHeight; y++)
                {
                    chunkContents[x, y] = world[x + worldOffset, y];
                }
            }

            //chunks.Add(new ColliderChunk(chunkContents, colliderChunkWidth, new Vector3Int(worldOffset- TerrainGenerator.Instance.worldWidth / 2, -TerrainGenerator.Instance.worldHeight / 2, 0)));
        }

        return chunks;
    }

    private void OnDrawGizmos()
    {
        if(World.Chunks != null)
        {
            foreach (ColliderChunk chunk in World.Chunks)
            {
                Gizmos.DrawWireCube(chunk.ChunkBounds.position, chunk.ChunkBounds.size);
            }
        }

        if (TerrainGenerator.Instance == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(TerrainGenerator.Instance.worldWidth, TerrainGenerator.Instance.worldHeight));
    }*/
}

/*

/// <summary>
/// Represents the whole world.
/// Contains all the chunks in the world.
/// </summary>
public static class World
{
    public static List<ColliderChunk> Chunks;

    public static Tilemap Foreground;
    public static Tilemap Background;

    public static void Initialize(List<ColliderChunk> chunks, Tilemap foreground, Tilemap background)
    {
        Chunks = chunks;
        Foreground = foreground;
        Background = background;
    }

    private static void GetChunkAt(Vector3Int pos)
    {
        
    }
}*/

/*
/// <summary>
/// Represents a 'colliderChunkWidth'-wide "column" of the map colliders, from the bottom of the world to the sky limit.
/// Stores the collider data contained inside the chunk.
/// </summary>
public class ColliderChunk
{
    public BoundsInt ChunkBounds { get; private set; }

    public readonly Tilemap ColliderTilemap;
    
    private bool[,] frozen;

    public ColliderChunk(bool[,] colliders, int width, Vector3Int position)
    {
        Vector3Int size = new Vector3Int(width, TerrainGenerator.Instance.worldHeight, 1);

        ChunkBounds = new BoundsInt(position, size);

        GameObject goSet = Object.Instantiate(WorldManager.Instance.colliderTilemapPrefab, ChunkBounds.position, Quaternion.identity, WorldManager.Instance.gridRoot);

        ColliderTilemap = goSet.GetComponent<Tilemap>();

        Load(colliders);
    }

    /// <summary>
    /// Adds a collider at 'position' and regenerates colliders.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>If the collider at position was changes successfully</returns>
    public bool AddCollider(Vector2 position)
    {
        Vector3Int localPos = ColliderTilemap.WorldToCell(position);

        if (frozen[localPos.x, localPos.y])
        {
            return false;
        }

        SetCollider(localPos, true);

        return true;
    }

    /// <summary>
    /// Removes a collider at 'position' and regenerates colliders.
    /// </summary>
    /// <param name="position"></param>
    /// <returns>If the collider at position was changes successfully</returns>
    public bool RemoveCollider(Vector2 position)
    {
        Vector3Int localPos = ColliderTilemap.WorldToCell(position);

        if (frozen[localPos.x, localPos.y])
        {
            return false;
        }

        SetCollider(localPos, false);

        return true;
    }

    /// <summary>
    /// Freezes the given cell position (in world coordinates).
    /// Freezing locks the cell for any incoming changes.
    /// </summary>
    /// <returns>If the cell was frozen</returns>
    public bool Freeze(Vector2 position)
    {
        Vector3Int localPos = ColliderTilemap.WorldToCell(position);
        return frozen[localPos.x, localPos.y] = true;
    }

    /// <summary>
    /// Unfreezes the given cell position (in world coordinates).
    /// Unfreezing allows all incoming changes to the cell.
    /// </summary>
    /// <returns>If the cell was unfrozen</returns>
    public bool Unfreeze(Vector2 position)
    {
        Vector3Int localPos = ColliderTilemap.WorldToCell(position);
        return frozen[localPos.x, localPos.y] = false;
    }

    /// <summary>
    /// Sets the world border colliders and freezes them.
    /// </summary>
    public void SetBorders()
    {
        Debug.LogError("Will freeze world borders. Implementation missing.");
        
        
        int rows = colliderData.GetLength(0);
        int cols = colliderData.GetLength(1);

        for (int x = 0; x < rows; x++)
        {
            colliderData[x, 0] = true;
            colliderData[x, cols - 1] = true;
            
            frozen[x, 0] = true;
            frozen[x, cols - 1] = true;
        }

        for (int y = 1; y < cols - 1; y++)
        {
            colliderData[0, y] = true;
            colliderData[rows - 1, y] = true;
            
            frozen[0, y] = true;
            frozen[rows - 1, y] = true;
        }
    }

    /// <summary>
    /// Sets collider tilemap collider state at position.
    /// </summary>
    /// <param name="localPos"></param>
    /// <param name="hasCollider"></param>
    private void SetCollider(Vector3Int localPos, bool hasCollider)
    {
        ColliderTilemap.SetTile(localPos, hasCollider ? WorldManager.Instance.dummyTile : null);
    }

    /// <summary>
    /// Initializes the colliderChunk.
    /// </summary>
    /// <param name="colliders"></param>
    private void Load(bool[,] colliders)
    {
        ColliderTilemap.ClearAllTiles();

        for (int x = 0; x < colliders.GetLength(0); x++)
        {
            for (int y = 0; y < colliders.GetLength(1); y++)
            {
                SetCollider(new Vector3Int(x, y, 0), colliders[x, y]);
            }
        }
    }
}
*/