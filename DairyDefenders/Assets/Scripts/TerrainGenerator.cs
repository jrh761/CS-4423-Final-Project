using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TerrainGenerator : MonoBehaviour
{

    public static TerrainGenerator Instance { get; private set; }

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Canvas gameCanvas;

    public Material terrainMaterial;
    public Sprite backgroundSprite;
    public int terrainWidth = 10;
    public int terrainHeight = 5;
    public int detail = 100;
    public float heightRatio = 0.5f;
    public float initialGas = 100f;

    public Button moveForwardButton;
    public Button moveBackwardButton;
    public Button shootButton;

    public Slider angleSlider;
    public Slider powerSlider;
    public Slider gasSlider;

    private GameObject playerInstance;
    private float currentGas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        GenerateTerrain();
        AdjustCamera();
        SetupPlayerControls();

        currentGas = initialGas;


        if (gasSlider != null)
        {
            gasSlider.maxValue = initialGas;
            gasSlider.value = initialGas;
        }
        else
        {
            Debug.LogWarning("Gas Slider not assigned");
        }
    }

    private void SetupPlayerControls()
    {
        // Assign the Move Forward event
        EventTrigger.Entry forwardEntry = new EventTrigger.Entry();
        forwardEntry.eventID = EventTriggerType.PointerDown;
        forwardEntry.callback.AddListener((eventData) => { playerInstance.GetComponent<PlayerController>().StartMoveForward(); });
        moveForwardButton.gameObject.GetComponent<EventTrigger>().triggers.Add(forwardEntry);

        EventTrigger.Entry stopForwardEntry = new EventTrigger.Entry();
        stopForwardEntry.eventID = EventTriggerType.PointerUp;
        stopForwardEntry.callback.AddListener((eventData) => { playerInstance.GetComponent<PlayerController>().StopMove(); });
        moveForwardButton.gameObject.GetComponent<EventTrigger>().triggers.Add(stopForwardEntry);

        // Assign the Move Backward event
        EventTrigger.Entry backwardEntry = new EventTrigger.Entry();
        backwardEntry.eventID = EventTriggerType.PointerDown;
        backwardEntry.callback.AddListener((eventData) => { playerInstance.GetComponent<PlayerController>().StartMoveBackward(); });
        moveBackwardButton.gameObject.GetComponent<EventTrigger>().triggers.Add(backwardEntry);

        EventTrigger.Entry stopBackwardEntry = new EventTrigger.Entry();
        stopBackwardEntry.eventID = EventTriggerType.PointerUp;
        stopBackwardEntry.callback.AddListener((eventData) => { playerInstance.GetComponent<PlayerController>().StopMove(); });
        moveBackwardButton.gameObject.GetComponent<EventTrigger>().triggers.Add(stopBackwardEntry);

        EventTrigger.Entry shootEntry = new EventTrigger.Entry();
        shootEntry.eventID = EventTriggerType.PointerDown;
        shootEntry.callback.AddListener((eventData) => { playerInstance.GetComponent<PlayerController>().FireProjectile(); });
        shootButton.gameObject.GetComponent<EventTrigger>().triggers.Add(shootEntry);

        // Angle and power sliders
        angleSlider.onValueChanged.AddListener((value) => { playerInstance.GetComponent<PlayerController>().SetAngle(value); });
        powerSlider.onValueChanged.AddListener((value) => { playerInstance.GetComponent<PlayerController>().SetPower(value); });
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

        MeshRenderer terrainRenderer = terrainObj.GetComponent<MeshRenderer>();
        terrainRenderer.sortingLayerName = "Terrain";
        terrainRenderer.sortingOrder = 1;

        GameObject backgroundObj = new GameObject("Background", typeof(SpriteRenderer));
        backgroundObj.transform.parent = terrainObj.transform;
        SpriteRenderer backgroundSpriteRenderer = backgroundObj.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = backgroundSprite;

        backgroundSpriteRenderer.sortingLayerName = "Background";
        backgroundSpriteRenderer.sortingOrder = 0;


        backgroundObj.transform.localPosition = new Vector3(terrainWidth / 2, 0, 10);
        backgroundObj.transform.localScale = new Vector3(terrainWidth, terrainHeight, 1);

        Vector3 playerPosition = new Vector3(1, 1, 0);
        playerInstance = Instantiate(playerPrefab, playerPosition, Quaternion.identity);

        SpriteRenderer playerSpriteRenderer = playerInstance.GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer)
        {
            playerSpriteRenderer.sortingLayerName = "Player";
            playerSpriteRenderer.sortingOrder = 10;
        }

        Vector3 enemyPosition = new Vector3(terrainWidth - 1, 1, 0);
        GameObject enemyInstance = Instantiate(enemyPrefab, enemyPosition, Quaternion.identity);

        SpriteRenderer enemySpriteRenderer = enemyInstance.GetComponent<SpriteRenderer>();
        if (enemySpriteRenderer)
        {
            enemySpriteRenderer.sortingLayerName = "Player";
            enemySpriteRenderer.sortingOrder = 9;
        }

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

        // Adjust the game canvas based on the camera
        gameCanvas.worldCamera = mainCamera;
        gameCanvas.planeDistance = 1; // This places the canvas in front of everything else

        // Adjust canvas size - this will effectively resize UI elements
        RectTransform canvasRect = gameCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    public float GetGasLeft()
    {
        return currentGas;
    }

    public void ReduceGas(float amount)
    {
        currentGas -= amount;
        if (currentGas < 0)
            currentGas = 0;

        gasSlider.value = currentGas;

        if (currentGas == 0)
        {

        }
    }

    public void UpdateGasSlider(float gasAmount)
    {
        if (gasSlider != null)
        {
            gasSlider.value = gasAmount;
        }
    }
}
