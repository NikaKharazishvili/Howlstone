using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class CavityShadingFeature : ScriptableRendererFeature
{
    [SerializeField] Shader shader;
    [Tooltip("Meters around a pixel that are compared. 0 turns edge light and crack dark off"), Range(0, 0.5f), FormerlySerializedAs("radius")] public float edgeRadius = 0.15f;
    [Tooltip("How strong the light on outer edges is"), Range(0, 5), FormerlySerializedAs("brights")] public float edgeLight = 5;
    [Tooltip("How dark the cracks get"), Range(0, 5), FormerlySerializedAs("darks")] public float crackDark;
    CavityShadingPass pass;

    public override void Create() => pass = new CavityShadingPass();

    protected override void Dispose(bool disposing) => pass?.Cleanup();

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!shader || renderingData.cameraData.cameraType is CameraType.Preview or CameraType.Reflection) return;
        pass ??= new CavityShadingPass();
        pass.Setup(shader, this);
        renderer.EnqueuePass(pass);
    }

    class CavityShadingPass : ScriptableRenderPass
    {
        const int CopyPass = 0, FinalPass = 1;
        static readonly int MainTexId = Shader.PropertyToID("_MainTex"), TexelSizeId = Shader.PropertyToID("_EdgeTexelSize"), RadiusId = Shader.PropertyToID("_EdgeRadius"), BrightsId = Shader.PropertyToID("_EdgeBrights"), DarksId = Shader.PropertyToID("_EdgeDarks"), WorldToCameraId = Shader.PropertyToID("_EdgeWorldToCamera");
        static Mesh fullscreenMesh;
        Material material;
        CavityShadingFeature settings;

        class PassData { public Material material; public int pass; public TextureHandle target, source; }

        public void Setup(Shader shader, CavityShadingFeature feature)
        {
            if (!material) material = CoreUtils.CreateEngineMaterial(shader);
            settings = feature;
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        }

        public void Cleanup() => CoreUtils.Destroy(material);

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var desc = cameraData.cameraTargetDescriptor;
            (desc.msaaSamples, desc.depthBufferBits) = (1, 0);
            material.SetVector(TexelSizeId, new Vector4(1f / desc.width, 1f / desc.height, desc.width, desc.height));
            material.SetMatrix(WorldToCameraId, cameraData.camera.worldToCameraMatrix);
            material.SetFloat(RadiusId, settings.edgeRadius);
            material.SetFloat(BrightsId, settings.edgeLight * 2);
            material.SetFloat(DarksId, settings.crackDark * 2);
            var color = resources.activeColorTexture;
            var copy = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_CavityShadingCopy", false, FilterMode.Point);
            AddPass(renderGraph, resources, "Cavity Shading Copy", CopyPass, copy, color);
            AddPass(renderGraph, resources, "Cavity Shading", FinalPass, color, copy);
        }

        void AddPass(RenderGraph renderGraph, UniversalResourceData resources, string name, int shaderPass, TextureHandle target, TextureHandle source)
        {
            using var builder = renderGraph.AddUnsafePass<PassData>(name, out var data);
            (data.material, data.pass, data.target, data.source) = (material, shaderPass, target, source);
            builder.UseTexture(target, AccessFlags.Write);
            builder.UseTexture(source);
            if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture);
            if (resources.cameraNormalsTexture.IsValid()) builder.UseTexture(resources.cameraNormalsTexture);
            builder.AllowGlobalStateModification(true);
            builder.SetRenderFunc(static (PassData d, UnsafeGraphContext context) =>
            {
                var cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                cmd.SetGlobalTexture(MainTexId, d.source);
                cmd.SetRenderTarget(d.target, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(FullscreenMesh, Matrix4x4.identity, d.material, 0, d.pass);
            });
        }

        static Mesh FullscreenMesh
        {
            get
            {
                if (fullscreenMesh) return fullscreenMesh;
                fullscreenMesh = new Mesh { name = "Fullscreen Quad" };
                fullscreenMesh.SetVertices(new List<Vector3> { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) });
                fullscreenMesh.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) });
                fullscreenMesh.SetIndices(new[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0, false);
                fullscreenMesh.UploadMeshData(true);
                return fullscreenMesh;
            }
        }
    }
}