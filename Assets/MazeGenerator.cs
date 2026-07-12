using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

// Builds the whole maze game automatically.
// Usage: add this to an empty GameObject and press Play. Everything else is automatic:
// floor, walls, torches, lava, exit portal, enemies, camera and visual effects.
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
    public bool spawnEnemies = true;
    public int enemyCount = 3;          // each one patrols a different region of the maze
    public int lavaCount = 5;
    public int torchCount = 14;
    public bool hideOldFloor = true;    // deactivates the old "Plane" object in the scene

    bool[,] wall;                       // true = wall, false = corridor
    Material wallMat, floorMat, lavaMat, torchMat, exitMat, enemyMat;
    System.Random rnd = new System.Random();

    void Start()
    {
        Time.timeScale = 1f;            // may still be frozen from a previous game over

        if (size % 2 == 0) size++;      // the maze algorithm needs an odd size
        if (size < 7) size = 7;

        PrepareMaterials();

        if (player == null)
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
            else player = CreatePlayer();   // no player in the scene, create one
        }

        // Burning effect (flame particles + light) on the player
        if (player != null && player.GetComponent<PlayerFireEffect>() == null)
            player.gameObject.AddComponent<PlayerFireEffect>();

        var health = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (health != null)
        {
            // Fire balance: less damage, rarer self-ignition
            health.fireDamageMoving = 4f;    // was 10
            health.fireDamageStill = 6f;     // was 15
            health.fireInterval = 12f;       // self-ignition every 12s instead of 4s

            // "YOU DIED" screen when health runs out
            if (player.GetComponent<GameOverScreen>() == null)
                player.gameObject.AddComponent<GameOverScreen>();

            // Fire controls: F to ignite, type "blow" to extinguish, speed boost while burning
            if (player.GetComponent<FireControls>() == null)
                player.gameObject.AddComponent<FireControls>();

            // Extinguish by blowing into the microphone: the kid's script was never
            // added to a scene, so set it up automatically
            if (FindFirstObjectByType<MicrophoneController>() == null)
            {
                var mic = gameObject.AddComponent<MicrophoneController>();
                mic.playerHealth = health;
            }
        }

        // Health bar UI
        if (player != null && FindFirstObjectByType<HealthBarBuilder>() == null)
        {
            var healthBar = gameObject.AddComponent<HealthBarBuilder>();
            healthBar.playerHealth = player.GetComponent<PlayerHealth>();
        }

        CarveMaze();
        BuildGeometry();
        BakeNavMesh();          // so enemies can find their way around the maze
        AddDecorations();
        PlaceCharacters();

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

    void BakeNavMesh()
    {
        var surface = gameObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();
    }

    void AddDecorations()
    {
        // --- Exit portal (far corner of the maze) ---
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

        // --- Collect empty corridor cells (except start and exit) ---
        var emptyCells = new List<Vector2Int>();
        for (int x = 1; x < size - 1; x++)
            for (int z = 1; z < size - 1; z++)
                if (!wall[x, z] && !(x == 1 && z == 1) && !(x == exitX && z == exitZ))
                    emptyCells.Add(new Vector2Int(x, z));

        // Shuffle the list
        for (int i = emptyCells.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (emptyCells[i], emptyCells[j]) = (emptyCells[j], emptyCells[i]);
        }

        int used = 0;

        // --- Torches (mounted on walls, with flickering light) ---
        for (int i = 0; used < torchCount && i < emptyCells.Count; i++)
        {
            if (PlaceTorch(emptyCells[i])) used++;
        }

        // --- Lava tiles (uses the kid's own LavaFloor script) ---
        int lavaPlaced = 0;
        for (int i = emptyCells.Count - 1; i >= 0 && lavaPlaced < lavaCount; i--)
        {
            PlaceLava(emptyCells[i]);
            lavaPlaced++;
        }
    }

    bool PlaceTorch(Vector2Int cell)
    {
        // Find a neighbouring wall and mount the torch against it
        Vector2Int[] directions = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (var d in directions)
        {
            int nx = cell.x + d.x, nz = cell.y + d.y;
            if (nx < 0 || nx >= size || nz < 0 || nz >= size || !wall[nx, nz]) continue;

            var center = CellPos(cell.x, cell.y, 2.2f);
            var position = center + new Vector3(d.x, 0, d.y) * (cellSize * 0.42f);

            var torch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torch.name = "Torch";
            torch.transform.parent = transform;
            torch.transform.position = position;
            torch.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            torch.GetComponent<Renderer>().material = torchMat;
            Destroy(torch.GetComponent<Collider>());

            var light = new GameObject("TorchLight").AddComponent<Light>();
            light.transform.parent = torch.transform;
            light.transform.position = position + Vector3.up * 0.4f;
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.15f);
            light.range = cellSize * 1.8f;
            light.intensity = 2f;
            light.gameObject.AddComponent<TorchFlicker>();

            return true;
        }
        return false;
    }

    void PlaceLava(Vector2Int cell)
    {
        var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lava.name = "Lava";
        lava.transform.parent = transform;
        lava.transform.position = CellPos(cell.x, cell.y, 0.06f);
        lava.transform.localScale = new Vector3(cellSize * 0.9f, 0.12f, cellSize * 0.9f);
        lava.GetComponent<Renderer>().material = lavaMat;
        lava.GetComponent<Collider>().isTrigger = true;

        var lavaScript = lava.AddComponent<LavaFloor>();
        lavaScript.damagePerSecond = 5f;  // kept low because burning damage is added on top
        lava.AddComponent<LavaIgnite>();  // stepping on it sets the player on fire

        var light = new GameObject("LavaLight").AddComponent<Light>();
        light.transform.parent = lava.transform;
        light.transform.position = lava.transform.position + Vector3.up * 0.5f;
        light.type = LightType.Point;
        light.color = new Color(1f, 0.3f, 0.05f);
        light.range = cellSize * 1.4f;
        light.intensity = 1.6f;
    }

    // Creates a player from scratch if the scene has none
    // (movement + health scripts are the kid's own scripts)
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
        p.AddComponent<PlayerHealth>();

        return p.transform;
    }

    void PlaceCharacters()
    {
        if (player != null)
        {
            player.position = CellPos(1, 1, 1f);
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = player.position;
                rb.linearVelocity = Vector3.zero;
            }
        }

        if (spawnEnemies && player != null)
        {
            var regions = PatrolRegions();
            for (int i = 0; i < enemyCount && regions.Count > 0; i++)
                SpawnEnemy(regions[i % regions.Count], i + 1);
        }
    }

    // Splits the maze into quadrants; every quadrant except the player's
    // starting corner becomes a patrol region of corridor cells
    List<List<Vector2Int>> PatrolRegions()
    {
        int half = size / 2;
        var regions = new List<List<Vector2Int>> { new List<Vector2Int>(), new List<Vector2Int>(), new List<Vector2Int>() };

        for (int x = 1; x < size - 1; x++)
            for (int z = 1; z < size - 1; z++)
            {
                if (wall[x, z]) continue;
                if (x <= half && z <= half) continue;   // the player's starting quadrant is safe

                int index = (x > half && z > half) ? 2 : (x > half ? 0 : 1);
                regions[index].Add(new Vector2Int(x, z));
            }

        regions.RemoveAll(r => r.Count < 2);
        return regions;
    }

    void SpawnEnemy(List<Vector2Int> region, int number)
    {
        // Pick 4 random patrol points inside the region
        var points = new Vector3[4];
        for (int i = 0; i < points.Length; i++)
        {
            var cell = region[rnd.Next(region.Count)];
            points[i] = CellPos(cell.x, cell.y, 1.1f);
        }

        var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy_" + number;
        enemy.transform.position = points[0];
        enemy.GetComponent<Renderer>().material = enemyMat;

        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.acceleration = 14f;
        agent.angularSpeed = 400f;
        agent.Warp(points[0]);

        var rb = enemy.AddComponent<Rigidbody>();
        rb.isKinematic = true;    // the NavMeshAgent drives the movement

        float playerSpeed = 5f;
        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null) playerSpeed = movement.speed;

        var patrol = enemy.AddComponent<EnemyPatrol>();
        patrol.player = player;
        patrol.patrolPoints = points;
        patrol.patrolSpeed = 2.5f;                  // calm while patrolling
        patrol.chaseSpeed = playerSpeed - 0.5f;     // speeds up when it spots you, but stays slower

        enemy.AddComponent<EnemyTouchDamage>();
    }

    void PrepareMaterials()
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit");

        wallMat = new Material(lit) { color = new Color(0.42f, 0.4f, 0.5f) };
        wallMat.SetFloat("_Smoothness", 0.1f);

        floorMat = new Material(lit) { color = new Color(0.15f, 0.15f, 0.19f) };
        floorMat.SetFloat("_Smoothness", 0.35f);

        lavaMat = GlowingMaterial(lit, new Color(1f, 0.35f, 0.05f), 3.5f);
        torchMat = GlowingMaterial(lit, new Color(1f, 0.6f, 0.2f), 4f);
        exitMat = GlowingMaterial(lit, new Color(0.2f, 1f, 0.6f), 3f);

        enemyMat = new Material(lit) { color = new Color(0.55f, 0.05f, 0.08f) };
        enemyMat.EnableKeyword("_EMISSION");
        enemyMat.SetColor("_EmissionColor", new Color(0.6f, 0f, 0f) * 1.5f);
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

// Makes a torch light flicker like a flame
public class TorchFlicker : MonoBehaviour
{
    Light flame;
    float baseIntensity;
    float seed;

    void Start()
    {
        flame = GetComponent<Light>();
        baseIntensity = flame.intensity;
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        float flicker = Mathf.PerlinNoise(seed, Time.time * 6f);
        flame.intensity = baseIntensity * (0.75f + flicker * 0.5f);
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
