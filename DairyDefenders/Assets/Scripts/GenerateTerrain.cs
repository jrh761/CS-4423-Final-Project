using System.Collections;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Material terrainMaterial;
    public Sprite backgroundSprite;
    public int terrainWidth = 10;
    public int terrainHeight = 5;
    public int detail = 100;
    public float heightRatio = 0.5f;

    private void Start()
    {
        GenerateTerrain();
        AdjustCamera();
    }

    private void GenerateTerrain()
    {
        Mesh terrainMesh = new Mesh();
        // Two extra vertices on each side
        Vector3[] vertices = new Vector3[2 * (detail + 3)];

        // Two extra triangles on each side
        int[] triangles = new int[6 * (detail + 2)];

        for (int i = 0; i <= detail; i++)
        {
            float x = i / (float)detail * terrainWidth;
            float y =
                Mathf.Exp(-Mathf.Pow((x - terrainWidth / 2f), 2) / Mathf.Pow(terrainWidth / 6f, 2))
                * terrainHeight;

            vertices[i + 2] = new Vector3(x, y, 0);
            vertices[i + detail + 3] = new Vector3(x, 0, 0);
        }

        // Add vertices to the bottom corners of the screen
        vertices[0] = new Vector3(0, -Camera.main.orthographicSize, 0);
        vertices[1] = new Vector3(terrainWidth, -Camera.main.orthographicSize, 0);

        // Add vertices to the bottom of the terrain
        vertices[detail + 2] = new Vector3(0, 0, 0);
        vertices[detail + 3] = new Vector3(terrainWidth, 0, 0);

        for (int i = 0; i <= detail; i++)
        {
            triangles[6 * i] = i + 2;
            triangles[6 * i + 1] = i + detail + 3;
            triangles[6 * i + 2] = i + 3;
            triangles[6 * i + 3] = i + 3;
            triangles[6 * i + 4] = i + detail + 3;
            triangles[6 * i + 5] = i + detail + 4;
        }

        // Add triangles for the additional sections
        triangles[6 * detail] = 0;
        triangles[6 * detail + 1] = detail + 2;
        triangles[6 * detail + 2] = detail + 3;
        triangles[6 * detail + 3] = detail + 3;
        triangles[6 * detail + 4] = 1;
        triangles[6 * detail + 5] = 0;

        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.RecalculateNormals();

        GameObject terrainObj = new GameObject("Terrain", typeof(MeshFilter), typeof(MeshRenderer));
        terrainObj.layer = LayerMask.NameToLayer("Terrain");
        terrainObj.GetComponent<MeshFilter>().mesh = terrainMesh;
        terrainObj.GetComponent<MeshRenderer>().material = terrainMaterial;
        terrainObj.tag = "Terrain";

        // Create background object
        GameObject backgroundObj = new GameObject("Background", typeof(SpriteRenderer));
        backgroundObj.transform.parent = terrainObj.transform; // Set terrain as parent
        backgroundObj.GetComponent<SpriteRenderer>().sprite = backgroundSprite;

        // Position and scale the background
        // Make sure the background is behind the terrain
        backgroundObj.transform.localPosition = new Vector3(terrainWidth / 2, 0, 10);
        backgroundObj.transform.localScale = new Vector3(terrainWidth, terrainHeight, 1);

        // Instantiate the player
        Vector3 playerPosition = new Vector3(1, 1, 0);
        Instantiate(playerPrefab, playerPosition, Quaternion.identity);

        // Instantiate the enemy
        Vector3 enemyPosition = new Vector3(terrainWidth - 1, 1, 0); // Change position to other side
        Instantiate(enemyPrefab, enemyPosition, Quaternion.identity);

        // Add a PolygonCollider2D to the terrain
        PolygonCollider2D collider = terrainObj.AddComponent<PolygonCollider2D>();
        collider.points = GenerateColliderPoints(vertices);
    }

    // Generate an array of 2D points for the PolygonCollider2D from the terrain vertices
    Vector2[] GenerateColliderPoints(Vector3[] vertices)
    {
        Vector2[] colliderPoints = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            colliderPoints[i] = new Vector2(vertices[i].x, vertices[i].y);
        }
        return colliderPoints;
    }

    private void AdjustCamera()
    {
        Camera mainCamera = Camera.main;

        float screenRatio = (float)Screen.width / Screen.height;
        float targetWidth = terrainWidth;
        float targetHeight = targetWidth / screenRatio;
        float positionY = targetHeight * heightRatio / 2f;

        mainCamera.orthographicSize = targetHeight / 2f;
        mainCamera.transform.position = new Vector3(
            terrainWidth / 2f,
            positionY,
            mainCamera.transform.position.z
        );
    }
}
