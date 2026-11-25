#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class ShaderGUIBase : ShaderGUI
{
    protected static void DrawProps(MaterialEditor editor, MaterialProperty[] props, params string[] names)
    {
        foreach (string n in names) if (FindProperty(n, props, false) is { } p) editor.ShaderProperty(p, p.displayName);
    }
}

public class UniversalGUI : ShaderGUIBase
{
    enum SurfaceType { Environment = 0, Character = 1, Water = 2, Additive = 3, XRay = 4, Particle2ndColor = 5 }
    static bool showAdvanced;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        Material mat = materialEditor.target as Material;

        MaterialProperty surfaceTypeProp = FindProperty("_SurfaceType", props);
        SurfaceType current = (SurfaceType)surfaceTypeProp.floatValue;

        EditorGUI.BeginChangeCheck();
        SurfaceType newType = (SurfaceType)EditorGUILayout.EnumPopup("Surface Type", current);
        if (EditorGUI.EndChangeCheck())
        {
            surfaceTypeProp.floatValue = (float)newType;
            ApplyRenderState(mat, newType);
        }

        EditorGUILayout.Space();

        if (newType is SurfaceType.Environment or SurfaceType.Character or SurfaceType.Additive) DrawProps(materialEditor, props, "_BaseMap", "_BaseColor");
        if (newType == SurfaceType.Additive) DrawProps(materialEditor, props, "_GlowEmission", "_DstBlend", "_GlowScroll", "_GlowRegion");
        else if (newType == SurfaceType.XRay) DrawProps(materialEditor, props, "_BaseColor");

        if (newType == SurfaceType.Particle2ndColor) DrawProps(materialEditor, props, "_BaseMap");

        if (newType is SurfaceType.XRay or SurfaceType.Water) DrawProps(materialEditor, props, "_Alpha");

        if (newType == SurfaceType.Environment) DrawProps(materialEditor, props, "_UseFoliageCutoff", "_AlphaCutoff", "_WindPreset");

        if (newType == SurfaceType.Character) DrawProps(materialEditor, props, "_UseFoliageCutoff", "_AlphaCutoff", "_UseHueSwap", "_HueTarget", "_HueBase");

        if (newType == SurfaceType.Character) EditorGUILayout.HelpBox("Saturation / Flash / Sink / Break are set by code at runtime, not exposed here.", MessageType.Info);

        if (newType == SurfaceType.Water)
        {
            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
            DrawProps(materialEditor, props, "_ColorA", "_ColorB", "_DeepWater");

            EditorGUILayout.LabelField("Foam", EditorStyles.boldLabel);
            DrawProps(materialEditor, props, "_FoamColor", "_FoamThickness", "_FoamNoiseScale");

            EditorGUILayout.LabelField("Flow Distortion", EditorStyles.boldLabel);
            DrawProps(materialEditor, props, "_FlowStrength", "_FlowSpeed", "_Waves");

            EditorGUILayout.HelpBox("Requires 'Depth Texture' enabled on your URP Asset for depth color and foam to work.", MessageType.Info);
        }

        if (newType == SurfaceType.XRay)
        {
            DrawProps(materialEditor, props, "_XRayRimPower", "_XRayRimIntensity");
            EditorGUILayout.HelpBox("Rim tint = white, set by code (XRayColor). Base Color only for what has none. Alpha = dither amount. Stays opaque (dithered), not transparent. " +
                "Where it draws (not over a seen character) is the stencil test of its Render Objects in PC_Renderer.", MessageType.Info);
        }

        if (newType == SurfaceType.Particle2ndColor) EditorGUILayout.HelpBox("The Base Map is the mask; colors and dissolve come from the particle's vertex streams. Blend and depth are in Advanced and are kept when the type changes to this.", MessageType.Info);

        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true);
        if (!showAdvanced) return;
        DrawProps(materialEditor, props, "_SrcBlend", "_ZWriteMode", "_ZTestMode", "_CullMode", "_CameraPush", "_StencilComp");
        if (newType != SurfaceType.Additive) DrawProps(materialEditor, props, "_DstBlend");
    }

    static void ApplyRenderState(Material mat, SurfaceType type)
    {
        if (type == SurfaceType.Particle2ndColor) return;
        bool water = type == SurfaceType.Water, additive = type == SurfaceType.Additive, xRay = type == SurfaceType.XRay;
        mat.SetFloat("_SrcBlend", (float)(water ? BlendMode.SrcAlpha : BlendMode.One));
        mat.SetFloat("_DstBlend", (float)(water ? BlendMode.OneMinusSrcAlpha : additive ? BlendMode.One : BlendMode.Zero));
        mat.SetFloat("_ZWriteMode", water || additive || xRay ? 0 : 1);
        mat.SetFloat("_ZTestMode", (float)(xRay ? CompareFunction.Greater : CompareFunction.LessEqual));
        mat.SetFloat("_CullMode", (float)(xRay ? CullMode.Back : CullMode.Off));
        mat.renderQueue = (int)(water || additive ? RenderQueue.Transparent : RenderQueue.AlphaTest);
        if (xRay) mat.SetFloat("_Alpha", 1);
        else if (water && mat.GetFloat("_Alpha") <= 0) mat.SetFloat("_Alpha", 0.5f);
    }
}

public class WorldUIGUI : ShaderGUIBase
{
    enum EffectType { HealthBar = 0, Circle = 1, Text = 4, Sparkle = 5, Ring = 6 }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        MaterialProperty effectTypeProp = FindProperty("_EffectType", props);
        EffectType current = (EffectType)effectTypeProp.floatValue;

        EditorGUI.BeginChangeCheck();
        EffectType newType = (EffectType)EditorGUILayout.EnumPopup("Effect Type", current);
        if (EditorGUI.EndChangeCheck()) effectTypeProp.floatValue = (float)newType;

        EditorGUILayout.Space();

        if (newType == EffectType.HealthBar) DrawProps(materialEditor, props, "_BackgroundColor", "_BorderSize", "_BarSize", "_ZTest");
        else if (newType is EffectType.Text or EffectType.Ring or EffectType.Sparkle) DrawProps(materialEditor, props, "_ZTest");
        else DrawProps(materialEditor, props, "_FlashSpeed", "_RingThickness", "_OutlineSize", "_ZTest");

        EditorGUILayout.Space();
        materialEditor.EnableInstancingField();
        materialEditor.RenderQueueField();
    }
}
#endif