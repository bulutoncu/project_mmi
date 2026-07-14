using UnityEngine;
using UnityEngine.UI;

// Builds a health bar in the top-left corner (Canvas + Slider, entirely from code).
// The bar's value is updated by the kid's own HealthBarUI script;
// this script only builds the bar and tints it by health
// (full = green, low = red). MazeGenerator adds this automatically.
public class HealthBarBuilder : MonoBehaviour
{
    public PlayerHealth playerHealth;
    Image fill;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null) return;

        BuildBar();
    }

    void Update()
    {
        if (fill == null || playerHealth == null) return;
        float ratio = playerHealth.currentHealth / playerHealth.maxHealth;
        fill.color = Color.Lerp(new Color(0.9f, 0.15f, 0.1f), new Color(0.3f, 0.9f, 0.4f), ratio);
    }

    void BuildBar()
    {
        // --- Canvas ---
        var canvasObj = new GameObject("HealthBarCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // --- Slider root (top-left corner) ---
        var bar = new GameObject("HealthBar");
        bar.transform.SetParent(canvasObj.transform, false);
        var barRT = bar.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 1);
        barRT.anchorMax = new Vector2(0, 1);
        barRT.pivot = new Vector2(0, 1);
        barRT.anchoredPosition = new Vector2(30, -30);
        barRT.sizeDelta = new Vector2(420, 32);

        // Dark background
        var background = new GameObject("Background");
        background.transform.SetParent(bar.transform, false);
        StretchFull(background.AddComponent<RectTransform>());
        background.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        // Fill area (4px inside the edges)
        var fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(bar.transform, false);
        var fillAreaRT = fillArea.AddComponent<RectTransform>();
        StretchFull(fillAreaRT);
        fillAreaRT.offsetMin = new Vector2(4, 4);
        fillAreaRT.offsetMax = new Vector2(-4, -4);

        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        var fillRT = fillObj.AddComponent<RectTransform>();
        StretchFull(fillRT);
        fill = fillObj.AddComponent<Image>();
        fill.color = new Color(0.3f, 0.9f, 0.4f);

        // --- Slider component ---
        var slider = bar.AddComponent<Slider>();
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.fillRect = fillRT;
        slider.minValue = 0f;
        slider.maxValue = playerHealth.maxHealth;
        slider.value = playerHealth.currentHealth;

        // Let the kid's own script keep the value updated
        var ui = bar.AddComponent<HealthBarUI>();
        ui.playerHealth = playerHealth;
        ui.slider = slider;

        // --- "HP" label (inside the bar, on the left) ---
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(bar.transform, false);
        var labelRT = labelObj.AddComponent<RectTransform>();
        StretchFull(labelRT);
        labelRT.offsetMin = new Vector2(12, 0);
        var label = labelObj.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "HP";
        label.fontSize = 18;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(1f, 1f, 1f, 0.9f);

        // --- Key hints (below the bar) ---
        var hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(canvasObj.transform, false);
        var hintRT = hintObj.AddComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0, 1);
        hintRT.anchorMax = new Vector2(0, 1);
        hintRT.pivot = new Vector2(0, 1);
        hintRT.anchoredPosition = new Vector2(30, -68);
        hintRT.sizeDelta = new Vector2(800, 26);
        var hint = hintObj.AddComponent<Text>();
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.text = "WASD: move   •   avoid the enemies and reach the green portal!";
        hint.fontSize = 17;
        hint.alignment = TextAnchor.MiddleLeft;
        hint.color = new Color(1f, 1f, 1f, 0.55f);
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
