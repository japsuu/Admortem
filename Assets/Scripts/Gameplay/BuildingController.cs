using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingController : MonoBehaviour
{
    public Tilemap foregroundMap;
    public Tilemap backgroundMap;
    public Tilemap ghostsMap;

    private Color savedGhostsMapColor;
    private static Block selectedBlock;

    private Vector3Int lastTilePos = Vector3Int.zero;
    private Vector3 ghostTileWorldPos;
    private Camera mainCam;
    private ChunkIt chunkIt;

    [ReadOnly] [SerializeField] private bool buildingBlocked = false;

    private void Awake()
    {
        mainCam = Camera.main;
        chunkIt = foregroundMap.GetComponent<ChunkIt>();
    }

    public static void SetSelectedBlock(Block block)
    {
        selectedBlock = block;
    }

    private Tilemap GetSelectedTilemap()
    {
        Tilemap selectedTilemap = foregroundMap;

        if (Input.GetKey(Settings.BackgroundSelectKey))
            selectedTilemap = backgroundMap;

        return selectedTilemap;
    }

    private Color GetGhostsMapColor()
    {
        if (buildingBlocked)
        {
            return Color.red;
        }
        else
        {
            return savedGhostsMapColor * GetSelectedTilemap().color;
        }
    }

    private void Start()
    {
        savedGhostsMapColor = ghostsMap.color;
    }

    private void Update()
    {
        // Make sure we cannot place any block that's amount is zero or negative
        if (selectedBlock != null && selectedBlock.Amount < 1)
        {
            selectedBlock = null;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

        // Ghostblocks
        ghostsMap.ClearAllTiles();
        if (selectedBlock != null)
        {
            Vector3Int tilePos = foregroundMap.WorldToCell(worldPos);
            ghostTileWorldPos = foregroundMap.GetCellCenterWorld(tilePos);

            TileBase hoveredTile = GetSelectedTilemap().GetTile(tilePos);

            if (hoveredTile == null)
            {
                ghostsMap.SetTile(tilePos, selectedBlock.Tile);
            }

            // Block building if there is a collider in the spot we are hovering over
            Collider2D hit = Physics2D.OverlapBox(ghostTileWorldPos, ghostsMap.cellSize - new Vector3(0.1f, 0.1f), 0);
            if (hit != null && !hit.isTrigger && !hit.CompareTag("Pickup") && GetSelectedTilemap() == foregroundMap)
            {
                buildingBlocked = true;
            }
            else
            {
                buildingBlocked = false;
            }
        }
        else
        {
            buildingBlocked = false;
        }
        ghostsMap.color = GetGhostsMapColor();


        // Placing blocks
        if (Input.GetMouseButton(0) && !InventoryUIManager.IsPointerOverUIElement())
        {
            if (selectedBlock != null && !(buildingBlocked && GetSelectedTilemap() == foregroundMap))
            {
                Vector3Int tilePos = foregroundMap.WorldToCell(worldPos);

                if(!GetSelectedTilemap().HasTile(tilePos))
                {
                    chunkIt.SetTile(tilePos, selectedBlock.Tile);

                    Block duplicate = (Block)selectedBlock.CreateDuplicate(1);

                    InventoryUIManager.Instance.Inventory.RemoveItem(duplicate);
                }
            }
        }


        // Breaking blocks
        if (!Input.GetMouseButton(1) || InventoryUIManager.IsPointerOverUIElement()) return;
        {
            Vector3Int tilePos = foregroundMap.WorldToCell(worldPos);

            if(tilePos != lastTilePos)
            {
                TileBase clickedTile = GetSelectedTilemap().GetTile(tilePos);

                if(clickedTile != null)
                {
                    if (GetSelectedTilemap() == foregroundMap)
                    {
                        if(chunkIt != null)
                            chunkIt.SetTile(tilePos, null);
                        else
                        {
                            foregroundMap.SetTile(tilePos, null);
                        }
                    }
                    else
                        backgroundMap.SetTile(tilePos, null);

                    Vector3 tileWorldPos = GetSelectedTilemap().GetCellCenterWorld(tilePos);

                    Block duplicate = Block.CreateDebugDuplicate(ItemHolder.BlockDictionary[clickedTile], 1);

                    ItemPickup.SpawnItemPickup(duplicate, new Vector2(tileWorldPos.x, tileWorldPos.y + 0.1f), Quaternion.identity);
                }
            }

            lastTilePos = tilePos;
        }

        //TODO: Use tilemap.SwapTile to swap tiles duh
    }

    private void OnDrawGizmos()
    {
        if (foregroundMap == null || ghostsMap == null) return;

        Gizmos.color = GetGhostsMapColor();
        Gizmos.DrawWireCube(ghostTileWorldPos, ghostsMap.cellSize - new Vector3(0.1f, 0.1f));
    }
}
