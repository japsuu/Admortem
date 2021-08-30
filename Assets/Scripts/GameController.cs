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

    [SerializeField] private GameObject playerPrefab;
    
    // Set by the TerrainGenerator.
    public static Vector2 PlayerSpawnPos;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Debug.LogError("Multiple GameControllers!");
    }

    public void SpawnPlayer()
    {
        Player = Instantiate(playerPrefab, PlayerSpawnPos, Quaternion.identity);

        CameraController.Instance.target = Player.transform;

        PlayerSpawned = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.Circle(PlayerSpawnPos, 1, quaternion.identity);
    }
}
