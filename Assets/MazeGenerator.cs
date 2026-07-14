using System.Collections.Generic;
using UnityEngine;

// Builds the whole maze game automatically.
// Usage: add this to an empty GameObject and press Play. Everything else is automatic:
// floor, walls, exit portal, player and camera.
public class MazeGenerator : MonoBehaviour
{
    public enum CameraMode { TopDown, FollowBehind }

    [Header("Maze Settings")]
    public int size = 15;               // must be odd (15 -> 7x7 corridor cells)
    public float cellSize = 4f;         // corridor width
    public float wallHeight = 3.5f;

    [Header("Camera")]
    public CameraMode cameraMode = CameraMode.TopDown;

    [Header("Gameplay")]
    public Transform player;            // if empty, found in the scene or created automatically
    public bool hideOldFloor = true;    // deactivates the old "Plane" object in the scene

    bool[,] wall;                       // true = wall, false = corridor
    Material wallMat, floorMat, exitMat;
    System.Random rnd = new System.Random();

    void Start()
    {
        Time.timeScale = 1f;            // may still be frozen from a previous win

        if (size % 2 == 0) size++;      // the maze algorithm needs an odd size
        if (size < 7) size = 7;

        PrepareMaterials();

        if (player == null)
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
            else player = CreatePlayer();   // no player in the scene, create one
        }

        CarveMaze();
        BuildGeometry();
        PlaceExit();
        PlacePlayer();

        if (hideOldFloor)
        {
            var oldFloor = GameObject.Find("Plane");
            if (oldFloor != null) oldFloor.SetActive(false);
        }

        // Visual polish (fog, lighting, post-processing)
        var beautifier = FindFirstObjectByType<SceneBeautifier>();
        if (beautifier == null)
            beautifier = gameObject.AddComponent<SceneBeautifier>();

        SetupCamera(beautifier);
    }

    void SetupCamera(SceneBeautifier beautifier)
    {
        var cam = Camera.main;
        if (cam == null) return;

        if (cameraMode == CameraMode.TopDown)
        {
            // Fixed bird's-eye view: the whole maze fits in one shot
            var oldFollow = cam.GetComponent<CameraFollow>();
            if (oldFollow != null) Destroy(oldFollow);

            cam.orthographic = true;
            cam.orthographicSize = size * cellSize * 0.55f;   // maze + a small margin
            cam.transform.position = new Vector3(0f, 50f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Heavy fog would cover everything from above, tone it way down
            beautifier.fogDensity = 0.006f;
            RenderSettings.fogDensity = 0.006f;
        }
        else
        {
            cam.orthographic = false;   // back to perspective when leaving top-down
            if (cam.GetComponent<CameraFollow>() == null)
            {
                var follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.target = player;
            }
        }
    }

    // Classic "recursive backtracker" algorithm:
    // start with all walls, then carve random corridors from (1,1).
    void CarveMaze()
    {
        wall = new bool[size, size];
        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                wall[x, z] = true;

        var stack = new Stack<Vector2Int>();
        wall[1, 1] = false;
        stack.Push(new Vector2Int(1, 1));

        Vector2Int[] directions = {
            new Vector2Int(2, 0), new Vector2Int(-2, 0),
            new Vector2Int(0, 2), new Vector2Int(0, -2)
        };

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var neighbours = new List<Vector2Int>();

            foreach (var d in directions)
            {
                var n = current + d;
                if (n.x > 0 && n.x < size - 1 && n.y > 0 && n.y < size - 1 && wall[n.x, n.y])
                    neighbours.Add(n);
            }

            if (neighbours.Count > 0)
            {
                var chosen = neighbours[rnd.Next(neighbours.Count)];
                var between = (current + chosen) / 2;    // carve the wall in between too
                wall[between.x, between.y] = false;
                wall[chosen.x, chosen.y] = false;
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();    // dead end, backtrack
            }
        }
    }

    // Converts a cell coordinate to a world position (maze centered at 0,0)
    Vector3 CellPos(int x, int z, float y = 0)
    {
        float offset = (size - 1) * cellSize / 2f;
        return new Vector3(x * cellSize - offset, y, z * cellSize - offset);
    }

    void BuildGeometry()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = transform;
        floor.transform.position = new Vector3(0, -0.25f, 0);
        floor.transform.localScale = new Vector3(size * cellSize, 0.5f, size * cellSize);
        floor.GetComponent<Renderer>().material = floorMat;
        floor.isStatic = true;

        var walls = new GameObject("Walls");
        walls.transform.parent = transform;

        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                if (wall[x, z])
                {
                    var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    w.name = "Wall_" + x + "_" + z;
                    w.transform.parent = walls.transform;
                    w.transform.position = CellPos(x, z, wallHeight / 2f);
                    w.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);
                    w.GetComponent<Renderer>().material = wallMat;
                    w.isStatic = true;
                }
    }

    // Exit portal in the far corner of the maze
    void PlaceExit()
    {
        int exitX = size - 2, exitZ = size - 2;

        var exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exit.name = "Exit";
        exit.transform.parent = transform;
        exit.transform.position = CellPos(exitX, exitZ, 1.2f);
        exit.transform.localScale = Vector3.one * 1.4f;
        exit.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        exit.GetComponent<Renderer>().material = exitMat;
        exit.GetComponent<Collider>().isTrigger = true;
        exit.AddComponent<MazeExit>();
        exit.AddComponent<Spinner>();

        var exitLight = new GameObject("ExitLight").AddComponent<Light>();
        exitLight.transform.parent = exit.transform;
        exitLight.transform.position = exit.transform.position;
        exitLight.type = LightType.Point;
        exitLight.color = new Color(0.3f, 1f, 0.6f);
        exitLight.range = cellSize * 2.5f;
        exitLight.intensity = 2.5f;
    }

    // Creates a player from scratch if the scene has none
    Transform CreatePlayer()
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = "Player";
        p.tag = "Player";
        p.transform.localScale = Vector3.one * 1.4f;   // easy to spot from above

        var lit = Shader.Find("Universal Render Pipeline/Lit");
        var mat = new Material(lit) { color = new Color(0.2f, 0.6f, 1f) };
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.1f, 0.3f, 0.8f));
        p.GetComponent<Renderer>().material = mat;

        var rb = p.AddComponent<Rigidbody>();
        rb.freezeRotation = true;    // don't tumble when hitting walls
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        p.AddComponent<PlayerMovement>();

        return p.transform;
    }

    void PlacePlayer()
    {
        if (player == null) return;

        player.position = CellPos(1, 1, 1f);
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = player.position;
            rb.linearVelocity = Vector3.zero;
        }
    }

    void PrepareMaterials()
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit");

        wallMat = new Material(lit) { color = new Color(0.42f, 0.4f, 0.5f) };
        wallMat.SetFloat("_Smoothness", 0.1f);

        floorMat = new Material(lit) { color = new Color(0.15f, 0.15f, 0.19f) };
        floorMat.SetFloat("_Smoothness", 0.35f);

        exitMat = GlowingMaterial(lit, new Color(0.2f, 1f, 0.6f), 3f);
    }

    // Self-illuminating (emissive) material — glows with the bloom effect
    Material GlowingMaterial(Shader lit, Color color, float strength)
    {
        var m = new Material(lit) { color = color };
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", color * strength);
        return m;
    }
}

// Slowly rotates the exit portal
public class Spinner : MonoBehaviour
{
    public float speed = 60f;

    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
    }
}
