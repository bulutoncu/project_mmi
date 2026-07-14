using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Makes the scene look good: fog, moonlight mood,
// bloom (glow), vignette (darkened edges) and color grading.
// MazeGenerator adds this automatically; it can also be added by hand.
public class SceneBeautifier : MonoBehaviour
{
    [Header("Fog")]
    public Color fogColor = new Color(0.04f, 0.045f, 0.1f);
    public float fogDensity = 0.022f;

    void Start()
    {
        // --- Fog: distant corridors sink into darkness, keeps the maze mysterious ---
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        // --- Ambient light: slightly blue, dim dungeon mood ---
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.35f);

        // --- Turn the sun into moonlight ---
        foreach (var light in FindObjectsByType<Light>())
        {
            if (light.type == LightType.Directional)
            {
                light.color = new Color(0.65f, 0.7f, 1f);
                light.intensity = 0.35f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            }
        }

        // --- Make sure post-processing is enabled on the camera ---
        var cam = Camera.main;
        if (cam != null)
        {
            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            cam.backgroundColor = fogColor;
        }

        // --- Post-processing effects (Volume created from code) ---
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var bloom = profile.Add<Bloom>();
        bloom.intensity.Override(1.3f);      // the exit portal glows brightly
        bloom.threshold.Override(1f);

        var vignette = profile.Add<Vignette>();
        vignette.intensity.Override(0.35f);  // darkened edges pull the eye to the center
        vignette.smoothness.Override(0.5f);

        var colors = profile.Add<ColorAdjustments>();
        colors.saturation.Override(12f);     // slightly more vivid colors
        colors.contrast.Override(12f);
        colors.postExposure.Override(0.15f);

        var volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;               // wins over the scene's old Global Volume
        volume.profile = profile;
    }
}
