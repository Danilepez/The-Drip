using UnityEngine;

public class Highlightable : MonoBehaviour
{
    public Color highlightColor = Color.white;
    public bool useBaseColor = true;
    public bool useEmission = true;
    public float emissionIntensity = 1f;

    private Renderer[] _renderers;
    private MaterialPropertyBlock _block;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _block = new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_renderers == null) return;

        foreach (Renderer r in _renderers)
        {
            if (r == null) continue;

            if (!highlighted)
            {
                r.SetPropertyBlock(null);
                continue;
            }

            r.GetPropertyBlock(_block);
            Material mat = r.sharedMaterial;

            if (mat != null)
            {
                if (useBaseColor)
                {
                    if (mat.HasProperty(BaseColorId))
                        _block.SetColor(BaseColorId, highlightColor);
                    else if (mat.HasProperty(ColorId))
                        _block.SetColor(ColorId, highlightColor);
                }

                if (useEmission && mat.HasProperty(EmissionId))
                {
                    _block.SetColor(EmissionId, highlightColor * emissionIntensity);
                }
            }

            r.SetPropertyBlock(_block);
        }
    }
}
