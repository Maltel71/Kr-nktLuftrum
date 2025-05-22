using UnityEngine;

public class NightLevelManager : MonoBehaviour
{
    [Header("Lighting")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Light[] spotlights; // Spotlights på marken
    [SerializeField] private Camera mainCamera;

    [Header("Night Settings")]
    [SerializeField, Range(0f, 1f)] private float darknessLevel = 0.8f;
    [SerializeField] private Color nightAmbientColor = new Color(0.01f, 0.01f, 0.02f);
    [SerializeField] private AnimationCurve darknessCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Extreme Darkness")]
    [SerializeField, Range(0f, 1f)] private float extremeDarkness = 0.9f;
    [SerializeField] private Color veryDarkColor = new Color(0.005f, 0.005f, 0.01f);
    [SerializeField] private bool useExtremeDarkness = true;

    [Header("Fog Settings")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color fogColor = new Color(0.02f, 0.02f, 0.05f);
    [SerializeField, Range(0f, 0.1f)] private float fogDensity = 0.03f;

    [Header("Ground Lights")]
    [SerializeField] private GameObject[] groundLights;
    [SerializeField] private bool enableGroundLights = true;

    // Spara tidigare värden för att upptäcka ändringar
    private float lastDarknessLevel = -1f;
    private float lastExtremeDarkness = -1f;
    private Color lastAmbientColor;
    private bool lastEnableGroundLights;
    private bool lastEnableFog;

    private void Start()
    {
        // Hitta komponenter automatiskt
        FindComponents();
        ApplyNightSettings();
        UpdateLastValues();
    }

    private void Update()
    {
        // Kontrollera om något har ändrats i Inspector
        if (HasValuesChanged())
        {
            ApplyNightSettings();
            UpdateLastValues();
        }
    }

    private void FindComponents()
    {
        // Hitta Directional Light automatiskt om det inte är tilldelat
        if (directionalLight == null)
        {
            Light[] allLights = FindObjectsOfType<Light>();
            foreach (var light in allLights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }

        // Hitta huvudkamera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    // Kontrollera om värden ändrats i Inspector
    private bool HasValuesChanged()
    {
        return darknessLevel != lastDarknessLevel ||
               extremeDarkness != lastExtremeDarkness ||
               nightAmbientColor != lastAmbientColor ||
               enableGroundLights != lastEnableGroundLights ||
               enableFog != lastEnableFog;
    }

    // Spara nuvarande värden
    private void UpdateLastValues()
    {
        lastDarknessLevel = darknessLevel;
        lastExtremeDarkness = extremeDarkness;
        lastAmbientColor = nightAmbientColor;
        lastEnableGroundLights = enableGroundLights;
        lastEnableFog = enableFog;
    }

    private void ApplyNightSettings()
    {
        Debug.Log($"Applying night settings - Darkness: {darknessLevel}, Extreme: {extremeDarkness}");

        ApplyLighting();
        ApplyFog();
        ApplyCameraSettings();
        SetGroundLights(enableGroundLights);
        SetSpotlights();
    }

    private void ApplyLighting()
    {
        // Beräkna slutlig mörkergrad
        float finalDarkness = useExtremeDarkness ?
            Mathf.Max(darknessLevel, extremeDarkness) : darknessLevel;

        // Extremt mörkt ambient ljus
        Color finalAmbientColor = useExtremeDarkness ?
            Color.Lerp(nightAmbientColor, veryDarkColor, extremeDarkness) :
            nightAmbientColor * (1f - darknessLevel * 0.8f);

        RenderSettings.ambientLight = finalAmbientColor;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientIntensity = Mathf.Lerp(1f, 0.05f, finalDarkness);

        // Justera huvudljus - extremt svagt
        if (directionalLight != null)
        {
            float lightIntensity = darknessCurve.Evaluate(1f - finalDarkness);
            directionalLight.intensity = useExtremeDarkness ?
                Mathf.Lerp(0.1f, 0.01f, extremeDarkness) :
                lightIntensity * 0.1f;

            // Mörkare blå månljus
            Color moonColor = useExtremeDarkness ?
                Color.Lerp(new Color(0.4f, 0.5f, 0.8f), new Color(0.1f, 0.1f, 0.2f), extremeDarkness) :
                Color.Lerp(Color.white, new Color(0.4f, 0.5f, 0.8f), darknessLevel);

            directionalLight.color = moonColor;

            Debug.Log($"Set directional light intensity to: {directionalLight.intensity}");
        }
    }

    private void ApplyFog()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;

            // Fog färg baserad på mörkergrad
            Color finalFogColor = useExtremeDarkness ?
                Color.Lerp(fogColor, Color.black, extremeDarkness) :
                fogColor;

            RenderSettings.fogColor = finalFogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            // Fog densitet
            float finalFogDensity = useExtremeDarkness ?
                fogDensity + (extremeDarkness * 0.02f) :
                darknessLevel * fogDensity;

            RenderSettings.fogDensity = finalFogDensity;
        }
        else
        {
            RenderSettings.fog = false;
        }
    }

    private void ApplyCameraSettings()
    {
        if (mainCamera != null)
        {
            // Extremt mörk bakgrundsfärg
            float finalDarkness = useExtremeDarkness ?
                Mathf.Max(darknessLevel, extremeDarkness) : darknessLevel;

            mainCamera.backgroundColor = Color.Lerp(
                new Color(0.1f, 0.1f, 0.15f),
                Color.black,
                finalDarkness
            );

            // Sätt clear flags till solid color för total mörker
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private void SetGroundLights(bool enabled)
    {
        float finalDarkness = useExtremeDarkness ?
            Mathf.Max(darknessLevel, extremeDarkness) : darknessLevel;

        foreach (var light in groundLights)
        {
            if (light != null)
            {
                // Aktivera bara om det är mörkt nog
                light.SetActive(enabled && finalDarkness > 0.2f);
            }
        }

        Debug.Log($"Ground lights set to: {enabled && finalDarkness > 0.2f}");
    }

    private void SetSpotlights()
    {
        float finalDarkness = useExtremeDarkness ?
            Mathf.Max(darknessLevel, extremeDarkness) : darknessLevel;

        foreach (var spotlight in spotlights)
        {
            if (spotlight != null)
            {
                spotlight.enabled = finalDarkness > 0.3f;
                spotlight.intensity = finalDarkness * 5f; // Starkare ljus när det är mörkare
                spotlight.color = new Color(1f, 0.9f, 0.7f); // Varm lampfärg
            }
        }
    }

    // Publika metoder för att ändra inställningar runtime
    public void SetDarkness(float darkness)
    {
        darknessLevel = Mathf.Clamp01(darkness);
        ApplyNightSettings();
    }

    public void SetExtremeDarkness(float darkness)
    {
        extremeDarkness = Mathf.Clamp01(darkness);
        ApplyNightSettings();
    }

    public void ToggleExtremeDarkness()
    {
        useExtremeDarkness = !useExtremeDarkness;
        ApplyNightSettings();
    }

    // Context Menu för snabb testning
    [ContextMenu("Apply Night Settings")]
    public void ApplyNightSettingsFromMenu()
    {
        ApplyNightSettings();
    }

    [ContextMenu("Maximum Darkness")]
    public void SetMaximumDarkness()
    {
        darknessLevel = 1f;
        extremeDarkness = 1f;
        useExtremeDarkness = true;
        ApplyNightSettings();
    }

    [ContextMenu("Reset to Day")]
    public void ResetToDay()
    {
        darknessLevel = 0f;
        extremeDarkness = 0f;
        useExtremeDarkness = false;
        nightAmbientColor = new Color(0.5f, 0.5f, 0.5f);
        enableGroundLights = false;
        enableFog = false;
        ApplyNightSettings();
    }
}