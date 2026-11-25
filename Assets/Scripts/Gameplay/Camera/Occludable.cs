using UnityEngine;
using static GameConstants;

[RequireComponent(typeof(Renderer))]
public class Occludable : MonoBehaviour
{
    static readonly int CoverFadeID = Shader.PropertyToID("_CoverFade");
    Renderer meshRenderer;
    MaterialPropertyBlock hiddenBlock;

    void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        hiddenBlock = new MaterialPropertyBlock();
        hiddenBlock.SetFloat(CoverFadeID, HiddenOccludableFade);
    }

    public void Hide() => meshRenderer.SetPropertyBlock(hiddenBlock);

    public void Show() => meshRenderer.SetPropertyBlock(null);
}