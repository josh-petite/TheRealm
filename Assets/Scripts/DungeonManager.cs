using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using Random = UnityEngine.Random;

public class DungeonManager : MonoBehaviour
{
    private int enemyCount, itemCount, spawnCount;
    private List<Vector3> floorTilePositions;

    public GameObject FloorPrefab, WallPrefab, TileSpawnerPrefab, ExitPrefab;
    public GameObject[] Items, Enemies, RoundedEdges;
    public bool ShouldRoundEdges;
    public DungeonType DungeonType;

    [Range(50, 5000)] public int TotalFloorTiles;
    [Range(0, 100)] public int ItemSpawnRate;
    [Range(0, 100)] public int EnemySpawnRate;    

    [HideInInspector] public float MinX, MaxX, MinY, MaxY;    

    void Awake()
    {
        floorTilePositions = new List<Vector3>();
    }

    void OnGUI()
    {
        GUILayout.Label(string.Format("Enemy Count: {0}, Item Count: {1}, SpawnCount: {2}", enemyCount, itemCount, spawnCount));
    }

    void Start()
    {
        switch(DungeonType)
        {
            case DungeonType.Caverns:
                GenerateRandomCaverns();
                break;
            case DungeonType.Rooms:
                GenerateRandomRooms();
                break;
        }

        StartCoroutine(BeginDungeonPopulation());
    }

    IEnumerator BeginDungeonPopulation()
    {
        while(GetComponents<TileSpawner>().Length != 0)
        {
            yield return null;
        }

        SpawnMapObjects();
    }

    void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Backspace))
        {
            EditorSceneManager.LoadScene(EditorSceneManager.GetActiveScene().name);
        }
    }

    void GenerateRandomRooms()
    {
        Vector3 currentPosition = Vector3.zero;
        floorTilePositions.Add(currentPosition);

        while (floorTilePositions.Count < TotalFloorTiles)
        {
            currentPosition = BuildHallway(currentPosition);
            BuildRoom(currentPosition);
        }

        floorTilePositions.ForEach(ftp => Instantiate(TileSpawnerPrefab, ftp, Quaternion.identity, transform));
    }

    private Vector3 BuildHallway(Vector3 currentPosition)
    {
        Vector3 randomDirection = ChooseRandomDirection();
        for (int i = 0; i < Random.Range(9, 18); i++)
        {
            if (TileDoesNotExist(currentPosition + randomDirection))
            {
                floorTilePositions.Add(currentPosition + randomDirection);
            }

            currentPosition += randomDirection;
        }

        return currentPosition;
    }

    private void BuildRoom(Vector3 currentPosition)
    {
        int width = Random.Range(1, 5);
        int height = Random.Range(1, 5);

        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector3 tileOffset = new Vector3(w, h, 0);
                if (TileDoesNotExist(currentPosition + tileOffset))
                {
                    floorTilePositions.Add(currentPosition + tileOffset);
                }
            }
        }
    }

    private bool TileDoesNotExist(Vector3 tile)
    {
        foreach (var ftp in floorTilePositions)
        {
            if (ftp.Equals(tile))
                return false;
        }

        return true;
    }

    void GenerateRandomCaverns()
    {        
        Vector3 currentPosition = Vector3.zero;
        floorTilePositions.Add(currentPosition);

        while (floorTilePositions.Count < TotalFloorTiles)
        {
            currentPosition = BuildCavern(currentPosition);
        }

        floorTilePositions.ForEach(ftp => Instantiate(TileSpawnerPrefab, ftp, Quaternion.identity, transform));
    }

    private Vector3 BuildCavern(Vector3 currentPosition)
    {
        currentPosition += ChooseRandomDirection();
        if (TileDoesNotExist(currentPosition))
        {
            floorTilePositions.Add(currentPosition);
        }

        return currentPosition;
    }

    private Vector3 ChooseRandomDirection()
    {        
        switch (Random.Range(1, 5))
        {
            case 1: return Vector3.up;
            case 2: return Vector3.right;
            case 3: return Vector3.down;
            case 4: return Vector3.left;
        }

        Debug.LogError("Returning unexpected Vector3.zero from ChooseRandomDirection()");
        return Vector3.zero;
    }

    private void GenerateExitDoor()
    {
        Vector3 exitDoorPosition = floorTilePositions[floorTilePositions.Count - 1];
        Instantiate(ExitPrefab, exitDoorPosition, Quaternion.identity, transform);
    }

    private void SpawnMapObjects()
    {
        GenerateExitDoor();

        LayerMask floorMask = LayerMask.GetMask("Floor");
        LayerMask wallMask = LayerMask.GetMask("Wall");
        Vector2 hitBox = Vector2.one * 0.8f;

        for (int x = (int)MinX - 2; x <= (int)MaxX +2; x++)
        {
            for (int y = (int)MinY - 2; y <= (int)MaxY + 2; y++)
            {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitBox, 0, floorMask);
                if (hitFloor && !IsExitTile(hitFloor)) 
                    SpawnObjectsOnFloorTile(wallMask, hitBox, x, y, hitFloor.transform);

                Collider2D hitWall = Physics2D.OverlapBox(new Vector2(x, y), hitBox, 0, wallMask);
                if (hitWall)
                    ApplyRoundedEdgesToWallTile(x, y, hitBox, floorMask, hitWall.transform);
            }
        }
    }

    private void ApplyRoundedEdgesToWallTile(int x, int y, Vector2 hitBox, LayerMask floorMask, Transform wallTransform)
    {
        if (!ShouldRoundEdges)
            return;

        int edgeType = CalculateTypeOfRoundedEdge(x, y, hitBox, floorMask);
        if (edgeType <= 0)
            return;

        Instantiate(RoundedEdges[edgeType], new Vector2(x, y), Quaternion.identity, wallTransform);
    }

    private int CalculateTypeOfRoundedEdge(int x, int y, Vector2 hitBox, LayerMask floorMask)
    {
        int edgeType = -1;

        if (Physics2D.OverlapBox(new Vector2(x, y + 1), hitBox, 0, floorMask))
            edgeType += 1;

        if (Physics2D.OverlapBox(new Vector2(x + 1, y), hitBox, 0, floorMask))
            edgeType += 2;

        if (Physics2D.OverlapBox(new Vector2(x, y - 1), hitBox, 0, floorMask))
            edgeType += 4;

        if (Physics2D.OverlapBox(new Vector2(x - 1, y), hitBox, 0, floorMask))
            edgeType += 8;
        
        return edgeType;
    }

    private void SpawnObjectsOnFloorTile(LayerMask wallMask, Vector2 hitBox, int x, int y, Transform floorTransform)
    {
        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitBox, 0, wallMask);
        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitBox, 0, wallMask);
        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitBox, 0, wallMask);
        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitBox, 0, wallMask);

        if (ShouldSpawnItemInLocation(hitTop, hitRight, hitBottom, hitLeft, floorTransform))
        {
            Instantiate(GetRandom(Items),
                        floorTransform.position,
                        Quaternion.identity,
                        floorTransform);
            itemCount++;
        }
        
        if (ShouldSpawnEnemyInLocation(hitTop, hitRight, hitBottom, hitLeft, floorTransform))
        {
            Instantiate(GetRandom(Enemies),
                        floorTransform.position,
                        Quaternion.identity,
                        floorTransform);
            enemyCount++;
        }

        spawnCount++;
    }

    private bool ShouldSpawnEnemyInLocation(Collider2D hitTop,
        Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft, Transform floorTransform)
    {
        return Random.Range(1, 101) <= EnemySpawnRate
            && floorTransform.childCount == 0 
            && !hitTop 
            && !hitRight 
            && !hitBottom 
            && !hitLeft;
    }

    private bool ShouldSpawnItemInLocation(Collider2D hitTop,
        Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft, Transform floorTransform)
    {
        return Random.Range(1, 101) <= ItemSpawnRate
            && floorTransform.childCount == 0
            && (hitTop || hitRight || hitBottom || hitLeft)
            && !(hitTop && hitBottom)
            && !(hitRight && hitLeft);
    }
    private GameObject GetRandom(GameObject[] collection)
    {
        int randomIndex = Random.Range(0, Enemies.Length);
        return collection[randomIndex];
    }

    private bool IsExitTile(Collider2D hitFloor)
    {
        return Equals(hitFloor.transform.position, floorTilePositions[floorTilePositions.Count - 1]);
    }
}
