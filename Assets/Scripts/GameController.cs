using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = Popcron.Gizmos;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public static GameObject Player;
    
    public static bool PlayerSpawned;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Debug.LogError("Multiple GameControllers!");
    }

    public void SpawnPlayer()
    {
        StartCoroutine(SummonPlayer());
    }

    private IEnumerator SummonPlayer()
    {
        yield return new WaitForSeconds(1);
        
        Vector2 origin = new Vector2(0, TerrainGenerator.Instance.worldHeight);
        RaycastHit2D hit = Physics2D.Linecast(origin, Vector2.down * 500, groundLayer);
        
        Player = Instantiate(playerPrefab, hit.point + new Vector2(0, 3), Quaternion.identity);

        CameraController.Instance.target = Player.transform;

        PlayerSpawned = true;

        yield return null;
    }
}
