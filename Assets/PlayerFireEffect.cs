using UnityEngine;

// Shows flame particles and a flickering orange light on the player
// while they are burning (PlayerHealth.isOnFire). MazeGenerator adds this automatically.
public class PlayerFireEffect : MonoBehaviour
{
    PlayerHealth health;
    ParticleSystem flames;
    Light fireLight;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
        CreateFlames();
    }

    void Update()
    {
        bool burning = health != null && health.isOnFire;

        var emission = flames.emission;
        emission.enabled = burning;

        fireLight.enabled = burning;
        if (burning)
        {
            // flicker like a real flame
            fireLight.intensity = 2.5f + Mathf.PerlinNoise(0f, Time.time * 8f) * 2f;
        }
    }

    void CreateFlames()
    {
        var flameObj = new GameObject("FlameEffect");
        flameObj.transform.SetParent(transform, false);
        flameObj.transform.localPosition = Vector3.zero;

        flames = flameObj.AddComponent<ParticleSystem>();

        var main = flames.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);      // rising upwards
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.75f, 0.15f), new Color(1f, 0.3f, 0.03f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;      // flames trail behind while running
        main.maxParticles = 200;

        var emission = flames.emission;
        emission.rateOverTime = 50f;
        emission.enabled = false;    // switched on once burning starts

        var shape = flames.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.55f;

        // Particles shrink as they rise
        var size = flames.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        // Yellow to red, then fade out
        var color = flames.colorOverLifetime;
        color.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 0.35f, 0f), 0.5f),
                new GradientColorKey(new Color(0.35f, 0.05f, 0f), 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = g;

        var renderer = flameObj.GetComponent<ParticleSystemRenderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = SoftCircleTexture();
        renderer.material = mat;

        // Light that illuminates the surroundings while burning
        var lightObj = new GameObject("FireLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = Vector3.up * 0.8f;
        fireLight = lightObj.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.5f, 0.1f);
        fireLight.range = 6f;
        fireLight.enabled = false;
    }

    // A round texture, bright in the middle with softly fading edges (generated in code)
    Texture2D SoftCircleTexture()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - distance);
                alpha *= alpha;   // softer falloff at the edges
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

        tex.Apply();
        return tex;
    }
}
