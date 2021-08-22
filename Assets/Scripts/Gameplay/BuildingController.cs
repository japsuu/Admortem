using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Gizmos = Popcron.Gizmos;

public class BuildingController : MonoBehaviour
{
    public static BuildingController Instance;
    
    public Tilemap foregroundMap;
    public Tilemap backgroundMap;
    public Tilemap shatterOverlayMap;
    public Tilemap ghostsMap;

    [SerializeField] private RuleTile[] shatterTiles;
    
    public int breakSpeed = 10;

    public float reachDistance = 7;

    public int blockDamageAmount = 25;

    private Color savedGhostsMapColor;
    private static Block selectedBlock;

    private Vector3 ghostTileWorldPos;
    private Camera mainCam;

    [ReadOnly] [SerializeField] private bool buildingBlocked;
    [ReadOnly] [SerializeField] private bool tooFar;
    [ReadOnly] [SerializeField] private float breakCooldownLeft;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        mainCam = Camera.main;
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
        breakCooldownLeft -= Time.deltaTime;
        
        // Make sure we cannot place any block that's amount is zero or negative
        if (selectedBlock != null && selectedBlock.Amount < 1)
        {
            selectedBlock = null;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        Vector3Int tilePos = foregroundMap.WorldToCell(worldPos);

        tooFar = Vector2.Distance(PlayerMovementController.Instance.transform.position, worldPos) > reachDistance;

        // GhostBlocks
        ghostsMap.ClearAllTiles();
        if (selectedBlock != null)
        {
            ghostTileWorldPos = foregroundMap.GetCellCenterWorld(tilePos);

            TileBase hoveredTile = GetSelectedTilemap().GetTile(tilePos);

            if (hoveredTile == null)
            {
                ghostsMap.SetTile(tilePos, selectedBlock.Tile);
            }

            // Block building if there is a collider in the spot we are hovering over
            Collider2D hit = Physics2D.OverlapBox(ghostTileWorldPos, ghostsMap.cellSize - new Vector3(0.1f, 0.1f), 0);
            if (hit != null && !hit.isTrigger && !hit.CompareTag("Pickup") && GetSelectedTilemap() == foregroundMap || tooFar)
            {
                buildingBlocked = true;
            }
            else
            {
                buildingBlocked = false;
            }
        }
        else if(!tooFar)
        {
            buildingBlocked = false;
        }
        
        ghostsMap.color = GetGhostsMapColor();
            
        
        if (tooFar)
        {
            return;
        }

        
        //TODO: Particles/Other feedback when placing blocks 
        // Placing blocks
        if (Input.GetMouseButton(0) && !InventoryUIManager.IsPointerOverUIElement())
        {
            if (selectedBlock != null && !(buildingBlocked && GetSelectedTilemap() == foregroundMap))
            {
                if(!GetSelectedTilemap().HasTile(tilePos))
                {
                    WorldManager.Instance.PlaceBlockAt(tilePos, GetSelectedTilemap(), (Block)selectedBlock.CreateDuplicate(true, 1));
                }
            }
        }

        
        // Breaking blocks
        if (!Input.GetMouseButton(1) || InventoryUIManager.IsPointerOverUIElement()) return;
        {
            if (breakCooldownLeft < 0)
            {
                switch (WorldManager.Instance.DamageBlockAt(tilePos, GetSelectedTilemap(), blockDamageAmount))
                {
                    case WorldManager.DamageResult.NotDamaged:
                    {
                        //TODO: Somehow tell the player that that block cannot be damaged. Maybe SFX?
                    }
                        break;
                    case WorldManager.DamageResult.Damaged:
                    {
                        // Handle the shatter overlay:
                        shatterOverlayMap.SetTile(tilePos, GetShatterTile(tilePos));
                    }
                        break;
                    case WorldManager.DamageResult.Destroyed:
                    {
                        shatterOverlayMap.SetTile(tilePos, null);
                    }
                        break;
                    default:
                        break;
                }

                breakCooldownLeft = 1 / (float)breakSpeed;
            }
        }
    }

    private RuleTile GetShatterTile(Vector3Int tilePos)
    {
        int timesUntilBreaking = 9999;
        if (GetSelectedTilemap() == foregroundMap)
        {
            Block block = WorldManager.Instance.GetBlockBundleAt(tilePos).GetForegroundBlock();
            
            if(block != null)
                timesUntilBreaking = Mathf.CeilToInt(block.Durability / (float)blockDamageAmount);
        }
        else
        {
            Block block = WorldManager.Instance.GetBlockBundleAt(tilePos).GetBackgroundBlock();
            
            if(block != null)
                timesUntilBreaking = Mathf.CeilToInt(block.Durability / (float)blockDamageAmount);
        }
        Debug.Log("Times until break is: " + timesUntilBreaking);
        

        if (timesUntilBreaking > shatterTiles.Length)
        {
            Debug.Log("Selecting sprite: 0.");
            return shatterTiles[0];
        }
        
        Debug.Log("Selecting sprite: " + (shatterTiles.Length - 2 - timesUntilBreaking));
        return shatterTiles[shatterTiles.Length - 2 - timesUntilBreaking];
    }

    private void OnDrawGizmos()
    {
        
        
        if (!Settings.DebugVisualsEnabled) return;
        
        if (PlayerMovementController.Instance != null)
        {
            Gizmos.Circle(PlayerMovementController.Instance.transform.position, reachDistance, quaternion.identity, Color.green, true);
        }
        
        //if (foregroundMap == null || ghostsMap == null) return;
        //Gizmos.Square(ghostTileWorldPos, ghostsMap.cellSize - new Vector3(0.1f, 0.1f), GetGhostsMapColor(), true);
    }
}
