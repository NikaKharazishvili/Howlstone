using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BlobShadows : MonoBehaviour
{
    // Must be the same as MAX_GROUND_CIRCLES in GroundCircles.hlsl
    const int MaxCircles = 256;
    // Circles: darkness, size against the half width of what casts it, and the ray that finds the ground under a character
    const float Strength = 0.85f, Scale = 1.5f, RayStart = 0.5f, RayLength = 20;
    // Baked map of the environment: sharpness and size limit
    const float TexelsPerMeter = 2;
    const int MaxMapSize = 2048;
    // Surface types of the world shader from this one up (water, glows) cast nothing
    const int FirstSurfaceWithoutShadow = 2;

    static readonly int CirclesId = Shader.PropertyToID("_GroundCircles"), CircleCountId = Shader.PropertyToID("_GroundCircleCount"), StrengthId = Shader.PropertyToID("_BlobShadowStrength"),
        MapId = Shader.PropertyToID("_BlobShadowMap"), RectId = Shader.PropertyToID("_BlobShadowRect"), SurfaceTypeId = Shader.PropertyToID("_SurfaceType");

    [SerializeField] Transform environment;
    readonly Vector4[] circles = new Vector4[MaxCircles];
    readonly List<(Collider body, Renderer look)> characters = new();
    readonly List<Vector3> casters = new(); // x, z and radius of every circle to bake
    Texture2D map;

    void OnEnable()
    {
        Rect area = FindCasters();
        float texelsPerMeter = Mathf.Min(TexelsPerMeter, MaxMapSize / Mathf.Max(area.width, area.height));
        map = new Texture2D(Mathf.CeilToInt(area.width * texelsPerMeter), Mathf.CeilToInt(area.height * texelsPerMeter), TextureFormat.R8, false, true)
            { name = "Blob Shadows", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
        Shader.SetGlobalTexture(MapId, map);
        Shader.SetGlobalVector(RectId, new Vector4(area.x, area.y, texelsPerMeter / map.width, texelsPerMeter / map.height));
        Shader.SetGlobalFloat(StrengthId, Strength);
        FindCharacters();
        BakeEnvironment(area, texelsPerMeter);
    }

    void OnDisable()
    {
        Shader.SetGlobalInteger(CircleCountId, 0);
        DestroyImmediate(map);
    }

#if UNITY_EDITOR
    // Outside Play this runs only when something in the scene changed
    void Update()
    {
        if (Application.isPlaying) return;
        OnDisable();
        OnEnable();
    }
#endif

    void LateUpdate()
    {
        int count = 0;
        foreach (var (body, look) in characters)
            if (look.isVisible && body.enabled && Physics.Raycast(body.transform.position + Vector3.up * RayStart, Vector3.down, out RaycastHit hit, RayLength, ~0, QueryTriggerInteraction.Ignore))
                circles[count++] = new Vector4(hit.point.x, hit.point.y, hit.point.z, HalfWidth(body) * Scale);
        Shader.SetGlobalVectorArray(CirclesId, circles);
        Shader.SetGlobalInteger(CircleCountId, count);
    }

    void FindCharacters()
    {
        characters.Clear();
        AddCharacter(FindFirstObjectByType<Player>(FindObjectsInactive.Include));
        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None)) AddCharacter(enemy);
    }

    void AddCharacter(Component character) => characters.Add((character.GetComponent<Collider>(), character.GetComponentInChildren<SkinnedMeshRenderer>(true)));

    static float HalfWidth(Collider body) => body.transform.lossyScale.x * body switch
    {
        CapsuleCollider capsule => capsule.radius,
        BoxCollider box => box.size.x * 0.5f,
        _ => throw new NotImplementedException(body.GetType().Name)
    };

    // Every object under the environment gets one circle; returns the area on X and Z that holds all of them
    Rect FindCasters()
    {
        casters.Clear();
        Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
        foreach (Renderer look in environment.GetComponentsInChildren<Renderer>())
        {
            if (!look.enabled || look.sharedMaterial.GetFloat(SurfaceTypeId) >= FirstSurfaceWithoutShadow) continue;
            Bounds bounds = look.bounds;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * Scale;
            casters.Add(new Vector3(bounds.center.x, bounds.center.z, radius));
            var center = new Vector2(bounds.center.x, bounds.center.z);
            min = Vector2.Min(min, center - Vector2.one * radius);
            max = Vector2.Max(max, center + Vector2.one * radius);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // Where circles overlap the darkest wins
    void BakeEnvironment(Rect area, float texelsPerMeter)
    {
        var shadows = new byte[map.width * map.height];
        foreach (Vector3 caster in casters)
        {
            float x = (caster.x - area.x) * texelsPerMeter, z = (caster.y - area.y) * texelsPerMeter, radius = caster.z * texelsPerMeter;
            for (int j = Mathf.Max(0, (int)(z - radius)); j < Mathf.Min(map.height, z + radius); j++)
                for (int i = Mathf.Max(0, (int)(x - radius)); i < Mathf.Min(map.width, x + radius); i++)
                {
                    float dx = i + 0.5f - x, dz = j + 0.5f - z, distanceSq = (dx * dx + dz * dz) / (radius * radius);
                    if (distanceSq < 1) shadows[j * map.width + i] = (byte)Mathf.Max(shadows[j * map.width + i], (1 - distanceSq) * 255);
                }
        }
        map.SetPixelData(shadows, 0);
        map.Apply(false);
    }
}