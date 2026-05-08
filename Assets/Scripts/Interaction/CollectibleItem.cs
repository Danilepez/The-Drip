using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CollectibleItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;
    public InputActionReference collectAction;
    public float lookRange = 3f;
    public LayerMask collectMask = ~0;
    public bool allowPickupWhileExamining = true;

    [Header("Events")]
    public UnityEvent onCollected;

    [Header("Highlight")]
    public bool useHighlight = true;
    public Color highlightColor = Color.yellow;
    public bool useBaseColor = true;
    public bool useEmission = true;
    public float emissionIntensity = 1f;
    public Renderer[] highlightRenderers;

    private Camera _cam;
    private bool _isLookedAt;
    private int _baseCollectMask;
    private int _collectMaskWithExamine;
    private int _examineLayer;
    private Renderer[] _activeHighlightRenderers;
    private MaterialPropertyBlock _block;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        CacheHighlightRenderers();
    }

    private void Start()
    {
        PlayerLook pl = FindAnyObjectByType<PlayerLook>();
        _cam = pl != null ? pl.GetComponentInChildren<Camera>() : Camera.main;
        ConfigureDefaultMask();
        CacheCollectMasks();
        Debug.Log($"[Collectible] '{gameObject.name}' Start. cam={(_cam == null ? "NULL" : _cam.name)} | collectAction={( collectAction == null ? "NULL" : collectAction.action.name)} | collider={GetComponent<Collider>() != null}");
    }

    private void Update()
    {
        CheckLook();
        TryCollect();
    }

    private void CheckLook()
    {
        if (_cam == null) return;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * lookRange, Color.yellow);

        int mask = GetEffectiveCollectMask();
        if (TryGetHitThis(ray, mask, out RaycastHit hit))
        {
            if (!_isLookedAt)
            {
                _isLookedAt = true;
                Debug.Log($"[Collectible] Mirando '{gameObject.name}'");
                InputHintsUI.Instance?.SetPickupHint(this, true);
                SetHighlighted(true);
            }
        }
        else
        {
            if (_isLookedAt)
            {
                _isLookedAt = false;
                InputHintsUI.Instance?.SetPickupHint(this, false);
                SetHighlighted(false);
            }
        }
    }

    private void ConfigureDefaultMask()
    {
        if (collectMask.value != ~0) return;

        int examineLayer = LayerMask.NameToLayer("Examine");
        if (examineLayer >= 0)
        {
            collectMask = ~(1 << examineLayer);
        }
    }

    private void CacheCollectMasks()
    {
        _baseCollectMask = collectMask.value;
        _examineLayer = LayerMask.NameToLayer("Examine");
        if (_examineLayer >= 0)
        {
            _collectMaskWithExamine = _baseCollectMask | (1 << _examineLayer);
        }
        else
        {
            _collectMaskWithExamine = _baseCollectMask;
        }
    }

    private int GetEffectiveCollectMask()
    {
        if (allowPickupWhileExamining && ExamineManager.IsExamining && _examineLayer >= 0)
        {
            return _collectMaskWithExamine;
        }

        return _baseCollectMask;
    }

    private bool TryGetHitThis(Ray ray, int mask, out RaycastHit hit)
    {
        hit = default;
        RaycastHit[] hits = Physics.RaycastAll(ray, lookRange, mask, QueryTriggerInteraction.Collide);
        if (hits.Length == 0) return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            CollectibleItem owner = h.collider.GetComponentInParent<CollectibleItem>();
            if (owner == this)
            {
                hit = h;
                return true;
            }

            if (!h.collider.isTrigger)
            {
                break;
            }
        }

        return false;
    }

    private void TryCollect()
    {
        if (!_isLookedAt) return;

        if (itemData != null && itemData.itemType == ItemData.ItemType.Key)
        {
            if (!BloodInteractable.HasAllBloodPhotos)
            {
                Debug.Log("[Collectible] Necesitas fotografiar 2 manchas de sangre antes de recoger la llave.");
                return;
            }
        }

        if (collectAction == null)
        {
            Debug.LogError("[Collectible] collectAction es NULL.");
            return;
        }

        if (collectAction.action.WasPressedThisFrame())
        {
            Debug.Log("[Collectible] F presionado → recogiendo.");
            InputHintsUI.Instance?.SetPickupHint(this, false);
            InventoryManager.Instance?.AddItem(itemData, quantity);
            onCollected?.Invoke();
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (_isLookedAt)
        {
            InputHintsUI.Instance?.SetPickupHint(this, false);
        }
        SetHighlighted(false);
    }

    private void CacheHighlightRenderers()
    {
        if (highlightRenderers != null && highlightRenderers.Length > 0)
        {
            _activeHighlightRenderers = highlightRenderers;
        }
        else
        {
            _activeHighlightRenderers = GetComponentsInChildren<Renderer>(true);
        }

        if (_block == null) _block = new MaterialPropertyBlock();
    }

    private void SetHighlighted(bool highlighted)
    {
        if (!useHighlight) return;
        if (_activeHighlightRenderers == null || _activeHighlightRenderers.Length == 0) return;

        foreach (Renderer r in _activeHighlightRenderers)
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
