using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    private DungeonManager dungeonManager;
    
    void Awake()
    {
        dungeonManager = FindObjectOfType<DungeonManager>();

        CreateDefaultFloorTile();
        EnsureDungeonManagerBoundsAreAccurate();        
    }

    private void CreateDefaultFloorTile()
    {
        Instantiate(dungeonManager.FloorPrefab,
                    transform.position,
                    Quaternion.identity,
                    dungeonManager.transform);
    }

    private void EnsureDungeonManagerBoundsAreAccurate()
    {
        if (transform.position.x > dungeonManager.MaxX)
        {
            dungeonManager.MaxX = transform.position.x;
        }

        if (transform.position.x < dungeonManager.MinX)
        {
            dungeonManager.MinX = transform.position.x;
        }

        if (transform.position.y > dungeonManager.MaxY)
        {
            dungeonManager.MaxY = transform.position.y;
        }

        if (transform.position.y < dungeonManager.MinY)
        {
            dungeonManager.MinY = transform.position.y;
        }
    }

    void Start()
    {
        LayerMask environmentMask = LayerMask.GetMask("Wall", "Floor");
        Vector2 hitSize = Vector2.one * 0.8f;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 targetPosition = new Vector2(transform.position.x + x, transform.position.y + y);
                Collider2D hit = Physics2D.OverlapBox(targetPosition, hitSize, 0, environmentMask);
                if (!hit)
                {
                    Instantiate(dungeonManager.WallPrefab, targetPosition, Quaternion.identity, dungeonManager.transform);                    
                }
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
