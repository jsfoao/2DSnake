using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Header("Spawnable entities")]
    [SerializeField] private GameObject entitySnakePrefab;
    [SerializeField] private GameObject playerSnakePrefab;
    [SerializeField] private GameObject[] gridObjectPrefabs;
    
    [Header("Transform parents")]
    [SerializeField] private Transform spawnedObjectsEmpty;
    [SerializeField] private Transform spawnedSnakesEmpty;
    
    [SerializeField] public List<GridObject> spawnedObjects;
    [SerializeField] public List<Snake> spawnedSnakes;

    private Map map;
    private Transform parent;
    
    // Methods
    public void SpawnSnake(Tile tile, int size = 3, bool isControlled = false)
    {
        if (tile == null)
        {
            Debug.Log("Couldn't spawn snake. Tile outside of map range");
            return;
        }
        
        parent = spawnedSnakesEmpty;
        GameObject prefabToSpawn = isControlled ? playerSnakePrefab : entitySnakePrefab;
        GameObject instance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity, parent);
        Snake newSnake = instance.GetComponent<Snake>();
        newSnake.Create(tile, size);
        
        spawnedSnakes.Add(newSnake);
    }

    public void SpawnObjectOfTypeInRandomPosition(ObjectType objectType)
    {
        Tile randomValidTile = RandomValidTile();
        if (randomValidTile != null)
        {
            SpawnObjectOfType(objectType, randomValidTile);
        }
        else { Debug.Log("Couldn't spawn object: No possible valid tiles"); }
    }
    
    public GameObject SpawnObjectOfType(ObjectType objectType, Tile tile)
    {
        GameObject objectToSpawn = FindPrefabOfType(objectType);
        if (objectToSpawn == null)
        {
            Debug.Log($"Couldn't find object of type {objectType}");
            return null;
        }
        GameObject instance = Instantiate(objectToSpawn, tile.worldPosition, Quaternion.identity, spawnedObjectsEmpty);
        GridObject gridObject = instance.GetComponent<GridObject>();
        gridObject.currentTile = tile;
        tile.currentObjects.Add(gridObject);
        spawnedObjects.Add(gridObject);
        return instance;
    }

    public void DestroyObject(GridObject gridObject)
    {
        gridObject.currentTile.currentObjects.Remove(gridObject);
        spawnedObjects.Remove(gridObject);
        Destroy(gridObject.gameObject);
    }

    // Other
    private Tile RandomValidTile()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2Int randomPosition = new Vector2Int(Random.Range(0, map.size.x), Random.Range(0, map.size.y));
            Tile tileToSpawnOn = map.tileGrid[randomPosition.x, randomPosition.y];
            if (tileToSpawnOn.walkable && tileToSpawnOn.currentObjects.Count == 0) { return tileToSpawnOn; }
        }
        return null;
    }

    private GameObject FindPrefabOfType(ObjectType objectType)
    {
        foreach (var prefab in gridObjectPrefabs)
        {
            GridObject currentObject = prefab.GetComponent<GridObject>();
            if (currentObject.objectType == objectType)
            {
                return prefab;
            }
        }
        return null;
    }
    
    private void Awake()
    {
        map = GetComponent<Map>();
        parent = transform.GetChild(1);
    }

    private void Start()
    {
        SpawnObjectOfTypeInRandomPosition(ObjectType.Fruit);
        // SpawnObjectOfTypeInRandomPosition(ObjectType.Loot);
        // SpawnSnake(map.tileGrid[0, 0]);
        // SpawnSnake(map.tileGrid[0, 2]);
        SpawnSnake(map.tileGrid[0, 0], 5, true);
    }
}
