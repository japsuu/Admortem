using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Tilemap))]
public class DemoTilemapInteraction : MonoBehaviour
{
    public ChunkIt chunkIt;
    public Tilemap tilemap;
    public Text selectedTilemapText;
    public GameObject ballPrefab;
    
    private new Camera camera;
    private bool useChunkIt = false;

    private void Awake()
    {
        camera = Camera.main;

        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // Setting selected state
        if (Input.GetKeyDown(KeyCode.Space)) useChunkIt = !useChunkIt;
        
        // Getting mouse position
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);

        // Disabling/enabling tilemaps based on selected option
        if (useChunkIt)
        {
            if (!chunkIt.gameObject.activeSelf) chunkIt.gameObject.SetActive(true);
            if (tilemap.gameObject.activeSelf) tilemap.gameObject.SetActive(false);
        }
        else
        {
            if (!tilemap.gameObject.activeSelf) tilemap.gameObject.SetActive(true);
            if (chunkIt.gameObject.activeSelf) chunkIt.gameObject.SetActive(false);
        }
        
        // Setting info text
        if (selectedTilemapText != null)
        {
            selectedTilemapText.text = useChunkIt ? "Currently selected: ChunkIt." : "Currently selected: Unity standard.";
        }

        // Spawning balls
        if (Input.GetMouseButtonDown(1))
        {
            Instantiate(ballPrefab, new Vector3(worldPos.x, worldPos.y, 0), quaternion.identity);
        }
        
        // Breaking blocks
        if (!Input.GetMouseButton(0)) return;
        
        // Destroying tiles
        if (useChunkIt)
        {
            // We call the chunkIt method instead of the normal Tilemap
            Vector3Int tilePos = chunkIt.VisualTilemap.WorldToCell(worldPos);
            chunkIt.SetTile(tilePos, null);
        }
        else
        {
            // We call the normal Tilemap method
            Vector3Int tilePos = tilemap.WorldToCell(worldPos);
            tilemap.SetTile(tilePos, null);
        }
    }
}
