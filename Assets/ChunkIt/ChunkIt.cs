using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Copyright Jasper Honkasalo 2021.
/// 
/// Requirements:
/// - Unity 2D-Extras package matching your editor version: https://github.com/Unity-Technologies/2d-extras.
/// </summary>

[DefaultExecutionOrder(800)]
[RequireComponent(typeof(Tilemap))]
public class ChunkIt : MonoBehaviour
{
    [SerializeField] private bool useSingleton = false;
    public static ChunkIt Instance;

    public bool generateCollidersOnStart;
    
    [Tooltip("Determines how wide chunks the collider shape gets broken in to." +
             "Larger value means more time to build the bigger collider.")]
    [SerializeField] private int colliderWidth = 25;

    [Tooltip("ChunkIt generates this automatically from the object it was placed on." +
             "Feel free to replace with a custom prefab.")]
    [SerializeField] private GameObject colliderTilemapPrefab;

    [Tooltip("ChunkIt generates this automatically from the object it was placed on." +
             "Dummy tile without any visuals, but with a collision shape.")]
    [SerializeField] private TileBase dummyTile;

    /// <summary>
    /// The Visual tilemap all the collider chunks are linked to.
    /// </summary>
    public Tilemap VisualTilemap { get; private set; }

    /// <summary>
    /// Key = chunk x-pos, Value = Chunk colliderMap
    /// </summary>
    private readonly Dictionary<int, Tilemap> colliderChunks = new Dictionary<int, Tilemap>();

    /// <summary>
    /// Same as Tilemap.SetTile, but handles collision maps correctly.
    /// </summary>
    /// <param name="pos">Tilemap position of the tile we want to change</param>
    /// <param name="newTile">Tile to change to</param>
    /// <param name="useTileCollisionShape">Set to true, if you want to use the tile's sprite's collision shape. Otherwise the collision shape will just occupy a single cell.</param>
    public void SetTile(Vector3Int pos, TileBase newTile, bool useTileCollisionShape = false)
    {
        SetColliderTile(pos, newTile, useTileCollisionShape);
        
        VisualTilemap.SetTile(pos, newTile);
    }

    /// <summary>
    /// Assigns the specified tile as an collider on the collision map.
    /// </summary>
    /// <param name="pos">Position where to add</param>
    /// <param name="newTile">Collision tile to add</param>
    /// <param name="useTileCollisionShape">If we should use the Tile's own collision shape, rather than a single grid cell.</param>
    private void SetColliderTile(Vector3Int pos, TileBase newTile, bool useTileCollisionShape)
    {
        bool setToNull = newTile == null;

        TileBase collisionTile = dummyTile;
        if (useTileCollisionShape)
            dummyTile = newTile;
        
        // Round the position to the nearest colliderTilemapWidth
        int key = GetChunkPos(pos);

        if (colliderChunks.TryGetValue(key, out Tilemap colliderMap))
        {
            // We already have a tilemap for this position, add this position to the tilemap.
            colliderMap.SetTile(pos, setToNull ? null : collisionTile);
        }             
        else          
        {
            // We need to create and setup a tilemap for this position, and add this position to that tilemap.
            GameObject colParent = Instantiate(colliderTilemapPrefab, transform);
            colParent.name = "Chunk " + pos.x;
                
            colliderMap = colParent.GetComponent<Tilemap>();
                
            colliderChunks.Add(key, colliderMap);
            
            colliderMap.SetTile(pos, setToNull ? null : collisionTile);
        }
    }
    
    /// <summary>
    /// Used to setup this component.
    /// </summary>
    private void Reset()
    {
        GameObject go = null;
        try
        {
            Debug.Log("Creating a collider chunk prefab...");
            
            VisualTilemap = GetComponent<Tilemap>();

            // Create a new GO to be used as the prefab.
            go = new GameObject();
            go.name = "Collider Chunk Prefab";

            // Copy the Tilemap if not null
            Tilemap tm = gameObject.GetComponent<Tilemap>();
            if (tm != null)
            {
                go.AddComponent(tm);
            }

            // Copy the TilemapCollider2D if not null
            TilemapCollider2D tmc = gameObject.GetComponent<TilemapCollider2D>();
            if (tmc != null)
            {
                go.AddComponent(tmc);
            }

            // Copy the 2DRB if not null
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                go.AddComponent(rb);
            }

            // Copy the CompositeCollider2D if not null
            CompositeCollider2D cc = gameObject.GetComponent<CompositeCollider2D>();
            if (cc != null)
            {
                go.AddComponent(cc);
            }

            // Create the asset directory if needed
            if (!Directory.Exists(Application.dataPath + "\\ChunkIt"))
            {
                Directory.CreateDirectory(Application.dataPath + "\\ChunkIt");
            }
            
            // Save our asset
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/ChunkIt/ColliderChunkPrefab.prefab");
        
            Debug.Log("Cleaning up...");
            
            // Destroy the old components on the old GO
            if(tmc != null)
                DestroyImmediate(tmc);
            
            if(cc != null)
                DestroyImmediate(cc);
            
            if(rb != null)
                DestroyImmediate(rb);

            DestroyImmediate(go);

            colliderTilemapPrefab = prefab;

            // Create a new instance of a ruletile to be used as a Prefab
            RuleTile rt = ScriptableObject.CreateInstance<RuleTile>();
            rt.m_DefaultColliderType = Tile.ColliderType.Grid;
            
            // Save the prefab
            AssetDatabase.CreateAsset(rt, "Assets/ChunkIt/DummyCollisionTile.asset");
            AssetDatabase.SaveAssets();

            dummyTile = rt;
        
            Debug.Log("Done! The prefabs are located at 'Assets/ChunkIt'");
        }
        catch (Exception e)
        {
            Debug.LogError("There was an error trying to create the collider chunk prefab: " + e);

            if (go == null)
            {
                DestroyImmediate(go);
            }
        }
    }
    
    private void Awake()
    {
        if (Instance == null && useSingleton)
            Instance = this;
        else if(useSingleton)
            Debug.LogError("Multiple " + this + "Detected");
        
        VisualTilemap = GetComponent<Tilemap>();
    }

    private void Start()
    {
        if(!generateCollidersOnStart) return;
        
        List<Vector3Int> tilePositions = GetAllPositions();
        
        // Initialize the collision tilemap with all the tiles in the visual tilemap.
        foreach (Vector3Int pos in tilePositions)
        {
            SetColliderTile(pos, dummyTile, false);
        }
    }

    public void GenerateColliders()
    {
        List<Vector3Int> tilePositions = GetAllPositions();
        
        // Initialize the collision tilemap with all the tiles in the visual tilemap.
        foreach (Vector3Int pos in tilePositions)
        {
            SetColliderTile(pos, dummyTile, false);
        }
        
        Debug.Log("Colliders set");
    }

    /// <summary>
    /// Get all positions of all the tiles in the local tilemap.
    /// </summary>
    /// <returns>All the tile positions in this tilemap.</returns>
    private List<Vector3Int> GetAllPositions()
    {
        List<Vector3Int> tilePositions = new List<Vector3Int>();

        foreach (Vector3Int position in VisualTilemap.cellBounds.allPositionsWithin)
        {
            if (!VisualTilemap.HasTile(position))
            {
                continue;
            }
 
            tilePositions.Add(position);
        }

        return tilePositions;
    }

    /// <summary>
    /// Rounds the specified tile position to a chunk position.
    /// </summary>
    /// <param name="pos">Tile position</param>
    /// <returns></returns>
    private int GetChunkPos(Vector3Int pos)
    {
        return Mathf.RoundToInt((float) pos.x / colliderWidth) * colliderWidth;
    }
}

public static class ComponentCopier
 {
     private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.DeclaredOnly;
 
     
     public static T GetCopyOf<T>(this Component comp, T other) where T : Component
     {
         Type type = comp.GetType();
         if (type != other.GetType()) return null; // type mis-match
 
         List<Type> derivedTypes = new List<Type>();
         Type derived = type.BaseType;
         while(derived != null)
         {
             if(derived == typeof(MonoBehaviour))
             {
                 break;
             }
             derivedTypes.Add(derived);
             derived = derived.BaseType;
         }
 
         IEnumerable<PropertyInfo> pinfos = type.GetProperties(BindingFlags);

         pinfos = derivedTypes.Aggregate(pinfos, (current, derivedType) => current.Concat(derivedType.GetProperties(BindingFlags)));

         pinfos = from property in pinfos
                  where !((type == typeof(Rigidbody) && property.Name == "inertiaTensor") || (type == typeof(CompositeCollider2D) && property.Name == "usedByComposite")|| property.Name == "density") // Special cases.
                  where property.CustomAttributes.All(attribute => attribute.AttributeType != typeof(ObsoleteAttribute))
                  select property;
         IEnumerable<PropertyInfo> propertyInfos = pinfos as PropertyInfo[] ?? pinfos.ToArray();
         foreach (var pinfo in propertyInfos)
         {
             if (!pinfo.CanWrite) continue;
             
             if (propertyInfos.Any(e => e.Name == $"shared{char.ToUpper(pinfo.Name[0])}{pinfo.Name.Substring(1)}"))
             {
                 continue;
             }
             try
             {
                 pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
             }
             catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
         }
 
         IEnumerable<FieldInfo> finfos = type.GetFields(BindingFlags);

         IEnumerable<FieldInfo> fieldInfos = finfos as FieldInfo[] ?? finfos.ToArray();
         foreach (var finfo in fieldInfos)
         {
             foreach (var derivedType in derivedTypes.Where(derivedType => fieldInfos.All(e => e.Name != $"shared{char.ToUpper(finfo.Name[0])}{finfo.Name.Substring(1)}")))
             {
                 finfos = fieldInfos.Concat(derivedType.GetFields(BindingFlags));
             }
         }
 
         foreach (var finfo in fieldInfos)
         {
             finfo.SetValue(comp, finfo.GetValue(other));
         }
 
         finfos = from field in fieldInfos
                  where field.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                  select field;
         foreach (var finfo in finfos)
         {
             finfo.SetValue(comp, finfo.GetValue(other));
         }
 
         return comp as T;
     }
 
     public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
     {
         return go.AddComponent(toAdd.GetType()).GetCopyOf(toAdd) as T;
     }
 }