using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance;

    [Space]

    [Header("Generation settings")]

    public AstarPath astar;

    public NoiseSettings noiseSettings;
    public Transform tilesRoot;

    public int seed = 1234;

    [Range(1, 1024)]
    public int worldWidth = 256;

    [Range(1, 1024)]
    public int worldHeight = 256;

    public int surfaceLevel = 128;
    public int mountainsLevel = 156;

    [Header("Caves")]

    public int cavesLevel = 64;

    public int cavesVerticalSize = 40;

    [Range(0, 100)]
    public float cavesFillPercent = 50f;

    public int cavesSmoothness = 5;

    [Space]

    [Header("Preview settings")]

    public bool linePreviewActive;
    public bool noiseMapPreviewActive;
    public bool noiseMapPreviewIgnoreAmplitude = true;
    
    public Renderer previewRenderer;
    public LineRenderer terrainPreviewLine;
    public LineRenderer terrainSurfacePreviewLine;

    private BlockBundle[,] world;
    private float[] terrainSurfaceNoiseMap;
    private List<Block> blocks;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        Random.InitState(seed);
    }

    private void Start()    //TODO: CREATE AND SCAN A GRIDGRAPH FOR PATHFINDING, AND SAVE IT TO A FILE!
    {
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        if (ItemHolder.Instance == null)
            ItemHolder.Instance = GameObject.Find("ItemHolder").GetComponent<ItemHolder>();

        blocks = ItemHolder.Instance.Blocks;

        world = new BlockBundle[worldWidth, worldHeight];

        GenerateTerrainSurfaceNoiseMap();

        // Generate all tiles below surface, and the caves region full of randomly placed blocks
        GenerateBelowSurface();

        // Smooth the randomly placed blocks to form caves
        for (int i = 0; i < cavesSmoothness; i++)
        {
            SmoothCaves();
        }

        // Assign the 2D world array to the tilemap
        WorldManager.Instance.CreateWorld(world);
    }

    void GenerateBelowSurface()
    {
        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                float surfaceOffset = terrainSurfaceNoiseMap[x];
                int surface = (int)Mathf.Round(surfaceLevel + surfaceOffset);
                Block foregroundBlock = null;
                Block backgroundBlock = null;

                if (y < surface)
                {
                    foregroundBlock = blocks[0];
                    backgroundBlock = blocks[0];

                    if (y < cavesLevel + cavesVerticalSize / 2)
                    {
                        backgroundBlock = blocks[1];
                    }

                    if (y > (int)Mathf.Round(mountainsLevel - surfaceOffset / 2))
                    {
                        foregroundBlock = blocks[1];
                    }

                    if(y < surface - 6 + terrainSurfaceNoiseMap[x] / 2)
                    {
                        foregroundBlock = blocks[1];
                    }
                }

                // Check if we are at the correct height      (int)Mathf.Round(cavesLevel + noiseMap[x, cavesLevel])
                //if (y >= cavesLevel - cavesVerticalSize / 2 == y < cavesLevel + cavesVerticalSize / 2)

                int offsetCavesLevel = (int)Mathf.Round(cavesLevel + terrainSurfaceNoiseMap[x] / 4);
                int lowerSpawningBounds = offsetCavesLevel - cavesVerticalSize / 2;
                int upperSpawningBounds = offsetCavesLevel + cavesVerticalSize / 2;

                lowerSpawningBounds = Mathf.Clamp(lowerSpawningBounds, 8, upperSpawningBounds);
                upperSpawningBounds = Mathf.Clamp(upperSpawningBounds, lowerSpawningBounds, surface);

                if (y >= lowerSpawningBounds == y < upperSpawningBounds)
                {
                    // Check if we are far enough from world's bottom and ceiling
                    

                    // Check if we are far enough from world's walls
                    if (x > 3 == x < worldWidth - 4)
                    {
                        // Destroy a block if random value is good enough
                        if (Random.Range(0f, 100f) > cavesFillPercent)
                        {
                            foregroundBlock = null;
                        }

                    }
                }
                world[x, y] = new BlockBundle(foregroundBlock, backgroundBlock);
            }
        }
    }

    void SmoothCaves()  //Loop from top to bottom, not from left to right, to better control the area which you loop.
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = cavesLevel - cavesVerticalSize / 2; y < cavesLevel + cavesVerticalSize / 2 + (int)terrainSurfaceNoiseMap[x] / 2; y++)
            {
                int neighboringBlocks = GetSurroundingWallCount(x, y);

                if (neighboringBlocks > 4)
                {
                    if (world[x, y].GetForegroundBlock() == null)
                        world[x, y].SetForegroundBlock(blocks[1]);
                }
                else if (neighboringBlocks < 4)    //TODO: This can be changed to alter how thin or thick the caves are.
                {
                    world[x, y].SetForegroundBlock(null);
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < worldWidth && neighborY >= 0 && neighborY < worldHeight)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        if (world[neighborX, neighborY].GetForegroundBlock() != null)
                        {
                            wallCount++;
                        }
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    // We do this to get around the unity OnValidate warnings.
#if UNITY_EDITOR

    private void OnValidate() { UnityEditor.EditorApplication.delayCall += _OnValidate; }
    private void _OnValidate()
    {
        if (this == null) return;

        Random.InitState(seed);

        UpdatePreview();

        if (astar == null)
            return;

        if (astar.graphs.Length <= 0) return;
        
        GridGraph graph = (GridGraph)astar.graphs[0];
        graph.SetDimensions(worldWidth * 2, worldHeight * 2, 0.5f);
        astar.Scan();
    }

#endif

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(new Vector3(0, -worldHeight / 2 + cavesLevel, 0), new Vector3(worldWidth, cavesVerticalSize, 1));
    }

    private void GenerateTerrainSurfaceNoiseMap()
    {
        float[] map = new float[worldWidth];

        for (int x = 0; x < worldWidth; x++)
        {
            Vector2 worldPos = new Vector2(x - worldWidth / 2, 0);

            map[x] = (noiseSettings.GetNoiseAt(worldPos, seed, false) + 1) / 2;
        }

        terrainSurfaceNoiseMap = map;
    }

    private float[,] GetNoiseMap(bool ignoreAmplitude)
    {
        float[,] map = new float[worldWidth, worldHeight];

        for (int y = 0; y < worldHeight; y++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                Vector2 worldPos = new Vector2(x - worldWidth / 2, y - worldHeight / 2);

                if (ignoreAmplitude)
                {
                    map[x, y] = (noiseSettings.GetNoiseAt(worldPos, seed, true) + 1) / 2;
                }
                else
                {
                    map[x, y] = (noiseSettings.GetNoiseAt(worldPos, seed, false) + 1) / 2;
                }
            }
        }

        return map;
    }

    #region PREVIEW STUFF

    Vector3[] GetLinePoints()
    {
        Vector3[] points = new Vector3[worldWidth];

        for (int x = 0; x < worldWidth; x++)
        {
            float xCoord = x - worldWidth / 2;
            // ReSharper disable once PossibleLossOfFraction
            float yCoord = surfaceLevel + terrainSurfaceNoiseMap[x] - worldHeight / 2;

            points[x] = new Vector3(xCoord, yCoord);
        }

        return points;
    }

    public void UpdatePreview()
    {
        if (noiseSettings == null)
        {
            return;
        }

        if (noiseMapPreviewActive)
        {
            if (!previewRenderer.gameObject.activeInHierarchy)
                previewRenderer.gameObject.SetActive(true);

            float[,] noiseMap = GetNoiseMap(noiseMapPreviewIgnoreAmplitude);

            Texture2D mainTexture = new Texture2D(worldWidth, worldHeight);

            Color32[] previewMap = new Color32[worldWidth * worldHeight];

            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                for (int x = 0; x < noiseMap.GetLength(0); x++)
                {
                    previewMap[y * worldWidth + x] = new Color(noiseMap[x, y], noiseMap[x, y], noiseMap[x, y], 1);
                }
            }

            mainTexture.filterMode = FilterMode.Point;
            mainTexture.SetPixels32(previewMap);
            mainTexture.Apply();

            previewRenderer.sharedMaterial.mainTexture = mainTexture;
            previewRenderer.transform.localScale = new Vector3(worldWidth, worldHeight, 1);
        }
        else
        {
            if (previewRenderer.gameObject != null && previewRenderer.gameObject.activeInHierarchy)
                previewRenderer.gameObject.SetActive(false);
        }

        if (linePreviewActive)
        {
            GenerateTerrainSurfaceNoiseMap();

            if (terrainPreviewLine != null)
            {
                terrainPreviewLine.positionCount = worldWidth;
                terrainPreviewLine.SetPositions(GetLinePoints());
            }
            else
            {
                if (terrainPreviewLine != null)
                    terrainPreviewLine.positionCount = 0;
            }

            if (terrainSurfacePreviewLine != null)
            {
                // ReSharper disable once PossibleLossOfFraction
                terrainSurfacePreviewLine.SetPosition(0, new Vector3(-worldWidth / 2, -worldHeight / 2 + surfaceLevel));
                // ReSharper disable once PossibleLossOfFraction
                terrainSurfacePreviewLine.SetPosition(1, new Vector3(worldWidth / 2, -worldHeight / 2 + surfaceLevel));
            }
        }
    }

    #endregion
}
