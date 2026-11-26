using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

public enum ScreenEffect { None, Blood, Wind, Fire, Divine, Honeycomb, Arcane, Frost, Winter, Stone, Poisoned, Bleeding, Water }

#pragma warning disable CS0414
[Serializable]
public class ShaderGlobals
{
    public const float DefaultContrast = 10, DefaultSaturation = 30;
    static readonly int ColorBalanceId = Shader.PropertyToID("_SceneColorBalance"), SunColorId = Shader.PropertyToID("_SunColor"), ContrastId = Shader.PropertyToID("_GlobalContrast"),
        SaturationId = Shader.PropertyToID("_GlobalSaturation"), AppliedId = Shader.PropertyToID("_ShaderGlobalsApplied");
    static readonly string[] SetByCode = { nameof(contrast), nameof(saturation), nameof(sunColor), nameof(warmth) };
    static readonly (int id, FieldInfo field)[] Tuning = typeof(ShaderGlobals).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => !SetByCode.Contains(f.Name)).ToList()
        .Select(f => (Shader.PropertyToID($"_{char.ToUpperInvariant(f.Name[0])}{f.Name[1..]}"), f)).ToArray();
    static float currentWarmth;

    [SerializeField, Range(-100, 100)] float contrast = DefaultContrast, saturation = DefaultSaturation;
    [SerializeField] Color sunColor = Color.white;
    [SerializeField, Range(-100, 100)] float warmth;
    [SerializeField, Range(0, 1)] float toonAmount = 0.6f;
    [SerializeField] float toonDimming = 0.6f;
    [SerializeField, Range(0, 2)] float toonBandingBias = 2;
    [SerializeField, Range(0.1f, 1)] float toonRimOffset = 0.4f;
    [SerializeField] float toonRimSoftness = 0.001f;
    [SerializeField] float textureSharpening = 1, textureSharpeningBlurMips = 2, heightGradient = 0.4f;
    [SerializeField, Min(0.1f)] float footPushHeight = 0.4f;
    [SerializeField, Min(0)] float footPushMeters = 0.2f;
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.05f;
    [SerializeField] Vector3 windDirection = new Vector3(1, 0, 0);
    [SerializeField] float breakSpeed = 1.5f, breakLift = 3, breakGravity = 9.8f, breakSpin = 8, breakShrink = 0.5f;
    [SerializeField] float billboardScreenSize = 0.03f, barLift, textLift = 0.5f;
    [SerializeField] float lightWrap = 0.3f;
    [SerializeField] float selectionFade = 10, ringRimWidth = 0.1f, circlePulseStrength = 0.5f, circlePulseSpeed = 8, circleHoverStrength = 0.35f, circleReach = 1;
    [SerializeField] float groundGrassPatchSize = 6, groundBlendSoftness = 0.1f, groundDetailBlurScale = 4, groundBrightness = 2;

    [SerializeField] ScreenEffect screenEffectPreview;
    [SerializeField, HideInInspector] Color frozenColor = new Color(0.38f, 0.52f, 0.68f), frozenRimColor = new Color(0.35f, 0.95f, 1f);

    public bool IsPreviewingScreenEffect => screenEffectPreview != ScreenEffect.None;
    public Color DaySunColor => sunColor;
    public float DayWarmth => warmth;

    public readonly struct Saved
    {
        public readonly Color Sun;
        public readonly float Warmth, Contrast, Saturation;
        public Saved(Color sun, float warmth, float contrast, float saturation) => (Sun, Warmth, Contrast, Saturation) = (sun, warmth, contrast, saturation);
    }

    public static Color SunColor { get => Shader.GetGlobalColor(SunColorId); set => Shader.SetGlobalColor(SunColorId, value); }
    public static float Contrast { get => Shader.GetGlobalFloat(ContrastId); set => Shader.SetGlobalFloat(ContrastId, value); }
    public static float Saturation { get => Shader.GetGlobalFloat(SaturationId); set => Shader.SetGlobalFloat(SaturationId, value); }

    public static float Warmth
    {
        get => currentWarmth;
        set => Shader.SetGlobalVector(ColorBalanceId, (Vector3)(ColorUtils.ColorBalanceToLMSCoeffs(currentWarmth = value, 0) - Vector3.one));
    }

    public static Saved MakeNeutral()
    {
        var saved = new Saved(SunColor, Warmth, Contrast, Saturation);
        (SunColor, Warmth, Contrast, Saturation) = (Color.white, 0, 0, 0);
        return saved;
    }

    public static void Restore(Saved saved) => (SunColor, Warmth, Contrast, Saturation) = (saved.Sun, saved.Warmth, saved.Contrast, saved.Saturation);

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyDefaultsWhileNoSceneAppliesThem() { if (Shader.GetGlobalFloat(AppliedId) == 0) new ShaderGlobals().Apply(); }

    public void Apply()
    {
        (Contrast, Saturation, SunColor, Warmth) = (contrast, saturation, sunColor, warmth);
        foreach (var (id, field) in Tuning) SetGlobal(id, field.GetValue(this));
        Shader.SetGlobalFloat(AppliedId, 1);
    }

    static void SetGlobal(int id, object value)
    {
        switch (value)
        {
            case float number: Shader.SetGlobalFloat(id, number); break;
            case Vector2 vector: Shader.SetGlobalVector(id, vector); break;
            case Vector3 vector: Shader.SetGlobalVector(id, vector); break;
            case Color color: Shader.SetGlobalColor(id, color); break;
            case Enum choice: Shader.SetGlobalFloat(id, Convert.ToInt32(choice)); break;
            default: throw new NotImplementedException($"A shader global of type {value.GetType()} is not handled");
        }
    }
}